using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Users;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.JellyDirectGuard.Enforcement;

/// <summary>
/// Outcome of enforcing the policy on a single user.
/// </summary>
public enum EnforceOutcome
{
    /// <summary>User is excluded (admin or on the exclusion list).</summary>
    Skipped,

    /// <summary>User already complied; nothing written.</summary>
    Compliant,

    /// <summary>Policy was updated.</summary>
    Changed,
}

/// <summary>
/// Applies the direct-play policy to users. Idempotent: only writes when a
/// user is non-compliant, which makes it safe to call from events, timers
/// and the dashboard button without feedback loops.
/// </summary>
public class PolicyEnforcer
{
    private readonly IUserManager _userManager;
    private readonly ILogger<PolicyEnforcer> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyEnforcer"/> class.
    /// </summary>
    /// <param name="userManager">User manager.</param>
    /// <param name="logger">Logger.</param>
    public PolicyEnforcer(IUserManager userManager, ILogger<PolicyEnforcer> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Parses the exclusion list from configuration.
    /// </summary>
    /// <param name="raw">Raw comma/newline separated value.</param>
    /// <returns>Trimmed entries.</returns>
    public static List<string> ParseNameList(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new List<string>();
        }

        return raw
            .Split(new[] { ',', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => s.Length > 0)
            .ToList();
    }

    /// <summary>
    /// Enforces the policy on every user.
    /// </summary>
    /// <returns>Names of the users that were changed, per outcome.</returns>
    public async Task<Dictionary<EnforceOutcome, List<string>>> EnforceAllAsync()
    {
        var result = new Dictionary<EnforceOutcome, List<string>>
        {
            [EnforceOutcome.Skipped] = new(),
            [EnforceOutcome.Compliant] = new(),
            [EnforceOutcome.Changed] = new(),
        };

        foreach (var user in _userManager.GetUsers().ToList())
        {
            var outcome = await EnforceUserAsync(user).ConfigureAwait(false);
            result[outcome].Add(user.Username);
        }

        var changed = result[EnforceOutcome.Changed];
        _logger.LogInformation(
            "JellyDirectGuard sweep: {Changed} changed, {Compliant} compliant, {Skipped} skipped{Names}",
            changed.Count,
            result[EnforceOutcome.Compliant].Count,
            result[EnforceOutcome.Skipped].Count,
            changed.Count > 0 ? " (" + string.Join(", ", changed) + ")" : string.Empty);

        return result;
    }

    /// <summary>
    /// Enforces the policy on one user by id, when it still exists.
    /// </summary>
    /// <param name="userId">User id.</param>
    /// <returns>The outcome.</returns>
    public async Task<EnforceOutcome> EnforceUserAsync(Guid userId)
    {
        var user = _userManager.GetUserById(userId);
        return user is null ? EnforceOutcome.Skipped : await EnforceUserAsync(user).ConfigureAwait(false);
    }

    /// <summary>
    /// Enforces the policy on one user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>The outcome.</returns>
    public async Task<EnforceOutcome> EnforceUserAsync(User user)
    {
        var config = Plugin.Instance!.Configuration;
        if (!config.EnforcementEnabled)
        {
            return EnforceOutcome.Skipped;
        }

        var excluded = ParseNameList(config.ExcludedUsers);
        if (excluded.Contains(user.Username.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            return EnforceOutcome.Skipped;
        }

        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            var policy = _userManager.GetUserDto(user).Policy;
            if (policy is null)
            {
                return EnforceOutcome.Skipped;
            }

            if (config.SkipAdministrators && policy.IsAdministrator)
            {
                return EnforceOutcome.Skipped;
            }

            var compliant =
                (!config.DisableVideoTranscoding || !policy.EnableVideoPlaybackTranscoding)
                && policy.EnablePlaybackRemuxing == config.AllowRemuxing
                && policy.EnableAudioPlaybackTranscoding == config.AllowAudioTranscoding;

            if (compliant)
            {
                return EnforceOutcome.Compliant;
            }

            if (config.DisableVideoTranscoding)
            {
                policy.EnableVideoPlaybackTranscoding = false;
            }

            policy.EnablePlaybackRemuxing = config.AllowRemuxing;
            policy.EnableAudioPlaybackTranscoding = config.AllowAudioTranscoding;

            await _userManager.UpdatePolicyAsync(user.Id, policy).ConfigureAwait(false);
            _logger.LogInformation("JellyDirectGuard clamped user {User} to direct play", user.Username);
            return EnforceOutcome.Changed;
        }
        finally
        {
            _lock.Release();
        }
    }
}

using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Plugin.JellyDirectGuard.Enforcement;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.JellyDirectGuard.Api;

/// <summary>
/// Admin endpoints used by the configuration page.
/// </summary>
[ApiController]
[Authorize(Policy = "RequiresElevation")]
[Route("JellyDirectGuard")]
public class JellyDirectGuardController : ControllerBase
{
    private readonly PolicyEnforcer _enforcer;
    private readonly IUserManager _userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="JellyDirectGuardController"/> class.
    /// </summary>
    /// <param name="enforcer">Policy enforcer.</param>
    /// <param name="userManager">User manager.</param>
    public JellyDirectGuardController(PolicyEnforcer enforcer, IUserManager userManager)
    {
        _enforcer = enforcer;
        _userManager = userManager;
    }

    /// <summary>
    /// Runs a full enforcement sweep now.
    /// </summary>
    /// <returns>Summary with user names per outcome.</returns>
    [HttpPost("Enforce")]
    public async Task<ActionResult> Enforce()
    {
        var result = await _enforcer.EnforceAllAsync().ConfigureAwait(false);
        return Ok(new
        {
            ok = true,
            changed = result[EnforceOutcome.Changed],
            compliant = result[EnforceOutcome.Compliant],
            skipped = result[EnforceOutcome.Skipped],
        });
    }

    /// <summary>
    /// Lists every user and their current transcoding policy.
    /// </summary>
    /// <returns>Per-user policy snapshot.</returns>
    [HttpGet("Status")]
    public ActionResult Status()
    {
        var config = Plugin.Instance!.Configuration;
        var excluded = PolicyEnforcer.ParseNameList(config.ExcludedUsers);

        var users = _userManager.GetUsers()
            .OrderBy(u => u.Username)
            .Select(u =>
            {
                var policy = _userManager.GetUserDto(u).Policy;
                return new
                {
                    name = u.Username,
                    isAdmin = policy?.IsAdministrator ?? false,
                    videoTranscoding = policy?.EnableVideoPlaybackTranscoding ?? true,
                    remuxing = policy?.EnablePlaybackRemuxing ?? true,
                    audioTranscoding = policy?.EnableAudioPlaybackTranscoding ?? true,
                    excluded = excluded.Contains(u.Username.Trim(), System.StringComparer.OrdinalIgnoreCase),
                };
            })
            .ToList();

        return Ok(new { ok = true, users });
    }
}

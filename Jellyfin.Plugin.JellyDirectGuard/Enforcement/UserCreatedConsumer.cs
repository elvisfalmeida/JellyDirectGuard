using System;
using System.Threading.Tasks;
using Jellyfin.Data.Events.Users;
using MediaBrowser.Controller.Events;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.JellyDirectGuard.Enforcement;

/// <summary>
/// Clamps newly created users the moment they appear, then re-checks after a
/// configurable delay to override the policy tools like Wizarr write right
/// after creating the account.
/// </summary>
public class UserCreatedConsumer : IEventConsumer<UserCreatedEventArgs>
{
    private readonly PolicyEnforcer _enforcer;
    private readonly ILogger<UserCreatedConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserCreatedConsumer"/> class.
    /// </summary>
    /// <param name="enforcer">Policy enforcer.</param>
    /// <param name="logger">Logger.</param>
    public UserCreatedConsumer(PolicyEnforcer enforcer, ILogger<UserCreatedConsumer> logger)
    {
        _enforcer = enforcer;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task OnEvent(UserCreatedEventArgs eventArgs)
    {
        var user = eventArgs.Argument;
        _logger.LogInformation("JellyDirectGuard: user created ({User}), enforcing direct play", user.Username);

        var userId = user.Id;
        await _enforcer.EnforceUserAsync(userId).ConfigureAwait(false);

        var delay = Math.Max(0, Plugin.Instance!.Configuration.RecheckDelaySeconds);
        if (delay > 0)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(delay)).ConfigureAwait(false);
                try
                {
                    await _enforcer.EnforceUserAsync(userId).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "JellyDirectGuard: delayed re-check failed");
                }
            });
        }
    }
}

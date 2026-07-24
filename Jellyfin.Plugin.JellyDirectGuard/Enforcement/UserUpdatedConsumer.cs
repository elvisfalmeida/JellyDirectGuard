using System.Threading.Tasks;
using Jellyfin.Data.Events.Users;
using MediaBrowser.Controller.Events;

namespace Jellyfin.Plugin.JellyDirectGuard.Enforcement;

/// <summary>
/// Re-clamps users whenever they are updated, so a policy edit that re-enables
/// video transcoding does not stick. Loop-safe: the enforcer only writes when
/// the user is non-compliant.
/// </summary>
public class UserUpdatedConsumer : IEventConsumer<UserUpdatedEventArgs>
{
    private readonly PolicyEnforcer _enforcer;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserUpdatedConsumer"/> class.
    /// </summary>
    /// <param name="enforcer">Policy enforcer.</param>
    public UserUpdatedConsumer(PolicyEnforcer enforcer)
    {
        _enforcer = enforcer;
    }

    /// <inheritdoc />
    public Task OnEvent(UserUpdatedEventArgs eventArgs)
        => _enforcer.EnforceUserAsync(eventArgs.Argument.Id);
}

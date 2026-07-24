using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.JellyDirectGuard.Configuration;

/// <summary>
/// Plugin configuration for JellyDirectGuard.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether enforcement is active.
    /// </summary>
    public bool EnforcementEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether video transcoding is disabled on enforced users.
    /// </summary>
    public bool DisableVideoTranscoding { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether remuxing (container swap) stays allowed.
    /// </summary>
    public bool AllowRemuxing { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether audio transcoding stays allowed.
    /// </summary>
    public bool AllowAudioTranscoding { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether administrators are left untouched.
    /// </summary>
    public bool SkipAdministrators { get; set; } = true;

    /// <summary>
    /// Gets or sets a comma/newline separated list of user names never touched by the plugin.
    /// </summary>
    public string ExcludedUsers { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the delay, in seconds, before re-checking a newly created user.
    /// Tools like Wizarr write their own policy right after creating the user;
    /// the re-check clamps whatever they set.
    /// </summary>
    public int RecheckDelaySeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets a value indicating whether a full sweep runs when the server starts.
    /// </summary>
    public bool SweepOnStartup { get; set; } = true;

    /// <summary>
    /// Gets or sets the interval, in minutes, of the built-in periodic sweep.
    /// Policy edits (dashboard or API) fire no plugin-visible event, so the
    /// sweep is what guarantees they cannot stick. 0 disables it.
    /// </summary>
    public int PeriodicSweepMinutes { get; set; } = 10;
}

using System;
using System.Collections.Generic;
using Jellyfin.Plugin.JellyDirectGuard.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.JellyDirectGuard;

/// <summary>
/// JellyDirectGuard plugin: enforces a direct-play policy on user accounts,
/// disabling video transcoding so playback never burns server CPU.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <inheritdoc />
    public override string Name => "JellyDirectGuard";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("c4d062be-e4e4-469f-b3d1-5c84a50d1c06");

    /// <inheritdoc />
    public override string Description =>
        "Enforces direct play by disabling video transcoding on user accounts, automatically applied to new users.";

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        yield return new PluginPageInfo
        {
            Name = Name,
            EmbeddedResourcePath = GetType().Namespace + ".Configuration.configPage.html",
            EnableInMainMenu = true,
            DisplayName = "JellyDirectGuard",
            MenuIcon = "speed"
        };
    }
}

using Jellyfin.Data.Events.Users;
using Jellyfin.Plugin.JellyDirectGuard.Enforcement;
using Jellyfin.Plugin.JellyDirectGuard.ScheduledTasks;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Events;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.JellyDirectGuard;

/// <summary>
/// Registers the plugin services in the DI container.
/// </summary>
public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddSingleton<PolicyEnforcer>();
        serviceCollection.AddScoped<IEventConsumer<UserCreatedEventArgs>, UserCreatedConsumer>();
        serviceCollection.AddScoped<IEventConsumer<UserUpdatedEventArgs>, UserUpdatedConsumer>();
        serviceCollection.AddSingleton<IScheduledTask, EnforceSweepTask>();
        serviceCollection.AddHostedService<StartupSweepService>();
    }
}

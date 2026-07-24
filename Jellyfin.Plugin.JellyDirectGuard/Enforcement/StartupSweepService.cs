using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.JellyDirectGuard.Enforcement;

/// <summary>
/// Runs a full enforcement sweep shortly after the server starts and then
/// periodically. The periodic sweep matters because policy edits made through
/// the dashboard or the REST API fire no event a plugin can consume.
/// </summary>
public class StartupSweepService : IHostedService, IDisposable
{
    private readonly PolicyEnforcer _enforcer;
    private readonly ILogger<StartupSweepService> _logger;
    private readonly CancellationTokenSource _cts = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="StartupSweepService"/> class.
    /// </summary>
    /// <param name="enforcer">Policy enforcer.</param>
    /// <param name="logger">Logger.</param>
    public StartupSweepService(PolicyEnforcer enforcer, ILogger<StartupSweepService> logger)
    {
        _enforcer = enforcer;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(() => RunLoopAsync(_cts.Token), CancellationToken.None);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task RunLoopAsync(CancellationToken token)
    {
        await Task.Delay(TimeSpan.FromSeconds(20), token).ConfigureAwait(false);

        if (Plugin.Instance!.Configuration.SweepOnStartup)
        {
            await SweepSafeAsync().ConfigureAwait(false);
        }

        while (!token.IsCancellationRequested)
        {
            var minutes = Plugin.Instance!.Configuration.PeriodicSweepMinutes;
            if (minutes <= 0)
            {
                // Disabled: check again in a minute in case the config changes.
                await Task.Delay(TimeSpan.FromMinutes(1), token).ConfigureAwait(false);
                continue;
            }

            await Task.Delay(TimeSpan.FromMinutes(Math.Max(1, minutes)), token).ConfigureAwait(false);
            await SweepSafeAsync().ConfigureAwait(false);
        }
    }

    private async Task SweepSafeAsync()
    {
        try
        {
            await _enforcer.EnforceAllAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "JellyDirectGuard: sweep failed");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.JellyDirectGuard.Enforcement;
using MediaBrowser.Model.Tasks;

namespace Jellyfin.Plugin.JellyDirectGuard.ScheduledTasks;

/// <summary>
/// Periodic safety-net sweep, visible under Dashboard → Scheduled Tasks.
/// The event consumers do the real-time work; this catches anything missed.
/// </summary>
public class EnforceSweepTask : IScheduledTask
{
    private readonly PolicyEnforcer _enforcer;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnforceSweepTask"/> class.
    /// </summary>
    /// <param name="enforcer">Policy enforcer.</param>
    public EnforceSweepTask(PolicyEnforcer enforcer)
    {
        _enforcer = enforcer;
    }

    /// <inheritdoc />
    public string Name => "Aplicar política de direct play";

    /// <inheritdoc />
    public string Key => "JellyDirectGuardSweep";

    /// <inheritdoc />
    public string Description => "Reaplica a política de direct play do JellyDirectGuard em todos os usuários.";

    /// <inheritdoc />
    public string Category => "JellyDirectGuard";

    /// <inheritdoc />
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        progress.Report(0);
        await _enforcer.EnforceAllAsync().ConfigureAwait(false);
        progress.Report(100);
    }

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        yield return new TaskTriggerInfo
        {
            Type = TaskTriggerInfoType.IntervalTrigger,
            IntervalTicks = TimeSpan.FromHours(12).Ticks,
        };
    }
}

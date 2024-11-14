using Quartz;
using QuartzSchedule;

public class QuartzSchedulerListener : ISchedulerListener {
    private readonly ILogger<QuartzSchedulerListener> _logger;
    private readonly Startup _startup;

    public QuartzSchedulerListener(ILogger<QuartzSchedulerListener> logger, Startup startup) {
        _logger = logger;
        _startup = startup;

        _logger.LogInformation("QuartzSchedulerListener Created");
    }

    public Task JobAdded(IJobDetail jobDetail, CancellationToken cancellationToken = default) {
        return Task.CompletedTask;
    }

    public Task JobDeleted(JobKey jobKey, CancellationToken cancellationToken = default) {
        return Task.CompletedTask;
    }

    public Task JobInterrupted(JobKey jobKey, CancellationToken cancellationToken = default) {
        return Task.CompletedTask;
    }

    public Task JobPaused(JobKey jobKey, CancellationToken cancellationToken = default) {
        return Task.CompletedTask;
    }

    public Task JobResumed(JobKey jobKey, CancellationToken cancellationToken = default) {
        return Task.CompletedTask;
    }

    public Task JobScheduled(ITrigger trigger, CancellationToken cancellationToken = default) {
        return Task.CompletedTask;
    }

    public Task JobsPaused(string jobGroup, CancellationToken cancellationToken = default) {
        return Task.CompletedTask;
    }

    public Task JobsResumed(string jobGroup, CancellationToken cancellationToken = default) {
        return Task.CompletedTask;
    }

    public Task JobUnscheduled(TriggerKey triggerKey, CancellationToken cancellationToken = default) {
        return Task.CompletedTask;
    }

    public Task SchedulerError(string msg, SchedulerException cause, CancellationToken cancellationToken = default) {
        return Task.CompletedTask;
    }

    public Task SchedulerInStandbyMode(CancellationToken cancellationToken = default) {
        return Task.CompletedTask;
    }

    public Task SchedulerShutdown(CancellationToken cancellationToken = default) {
        return Task.CompletedTask;
    }

    public Task SchedulerShuttingdown(CancellationToken cancellationToken = default) {
        return Task.CompletedTask;
    }

    public Task SchedulerStarted(CancellationToken cancellationToken = default) {
        _logger.LogInformation("Scheduler Started.");
        _startup.Started();
        return Task.CompletedTask;
    }

    public Task SchedulerStarting(CancellationToken cancellationToken = default) {
        return Task.CompletedTask;
    }

    public Task SchedulingDataCleared(CancellationToken cancellationToken = default) {
        return Task.CompletedTask;
    }

    public Task TriggerFinalized(ITrigger trigger, CancellationToken cancellationToken = default) {
        return Task.CompletedTask;
    }

    public Task TriggerPaused(TriggerKey triggerKey, CancellationToken cancellationToken = default) {
        return Task.CompletedTask;
    }

    public Task TriggerResumed(TriggerKey triggerKey, CancellationToken cancellationToken = default) {
        return Task.CompletedTask;
    }

    public Task TriggersPaused(string triggerGroup, CancellationToken cancellationToken = default) {
        return Task.CompletedTask;
    }

    public Task TriggersResumed(string triggerGroup, CancellationToken cancellationToken = default) {
        return Task.CompletedTask;
    }
}
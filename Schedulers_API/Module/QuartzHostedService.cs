using Quartz;

public class QuartzHostedService : IHostedService {
    private readonly ILogger<QuartzHostedService> _logger;
    private readonly IScheduler _scheduler;
    private readonly IServiceScopeFactory _factory;

    public QuartzHostedService(ILogger<QuartzHostedService> logger, IScheduler scheduler, IServiceScopeFactory factory) {
        _logger = logger;
        _scheduler = scheduler;
        _factory = factory;
        using (var scope = _factory.CreateScope()) {
            var provider = scope.ServiceProvider;
            _scheduler.JobFactory = provider.GetRequiredService(typeof(QuartzJobFactory)) as QuartzJobFactory;
            _scheduler.ListenerManager.AddSchedulerListener(provider.GetRequiredService(typeof(QuartzSchedulerListener)) as QuartzSchedulerListener);
        }
    }

    public Task StartAsync(CancellationToken cancellationToken) {
        return _scheduler.Start(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        return _scheduler.Shutdown(cancellationToken);
    }
}
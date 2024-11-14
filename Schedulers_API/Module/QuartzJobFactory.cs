using Quartz;
using Quartz.Spi;

public class QuartzJobFactory : IJobFactory {

    private readonly IServiceProvider _provider;

    public QuartzJobFactory(IServiceProvider provider) {
        _provider = provider;
    }

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler) {
        return _provider.GetRequiredService<QuartzJobRunner>();
    }

    public void ReturnJob(IJob job) {
        /*
        if (job is IDisposable disposableJob) {
            disposableJob.Dispose();
        }
        */
    }
}
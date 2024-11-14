using Quartz;

public class QuartzJobRunner : IJob {
    private readonly IServiceProvider _provider;

    public QuartzJobRunner(IServiceProvider provider) {
        _provider = provider;
    }
    public async Task Execute(IJobExecutionContext context) {
        using (var scope = _provider.CreateScope()) {
            var jobType = context.JobDetail.JobType;
            var job = scope.ServiceProvider.GetRequiredService(jobType) as IJob;

            await job.Execute(context);
        }
    }
}

using AGL.Api.API_Schedulers.Interfaces;
using AGL.Api.Schedulers_API.Jobs;
using Quartz;

namespace AGL.Api.Schedulers_API.Jobs
{
    /// <summary>
    /// 싱크 티타임 작업
    /// </summary>
    [DisallowConcurrentExecution]
    public class SyncTeeTimeJob : IJob
    {
        private readonly ILogger<SampleJob> _logger;
        private readonly ISyncTeeTimeService _service;

        public SyncTeeTimeJob(ILogger<SampleJob> logger, ISyncTeeTimeService service)
        {
            _logger = logger;
            _service = service;
        }

        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                _service.SyncTeeTime();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
            }
            return Task.CompletedTask;
        }
    }
}

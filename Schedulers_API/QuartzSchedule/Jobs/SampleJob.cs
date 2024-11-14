using AGL.Api.Schedulers_API.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace AGL.Api.Schedulers_API.Jobs
{
    [DisallowConcurrentExecution]
    public class SampleJob : IJob
    {
        private readonly ILogger<SampleJob> _logger;
        private readonly ISampleService _service;

        public SampleJob(ILogger<SampleJob> logger, ISampleService service)
        {
            _logger = logger;
            _service = service;
        }

        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                _service.Test("SampleJob123");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
            }
            return Task.CompletedTask;
        }
    }
}

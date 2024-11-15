using AGL.Api.Schedulers_API.Schedulers;
using AGL.Api.Schedulers_API.Jobs;
using AGL.Api.Schedulers_API.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace QuartzSchedule {
    /// <summary>
    /// 
    /// </summary>
    public class Startup {
        private readonly ILogger<Startup> _logger;
        private readonly SampleScheduler _sampleScheduler;
        private readonly SyncTeeTimeScheduler _syncTeeTimeScheduler;

        public Startup(ILogger<Startup> logger,
            SampleScheduler sampleScheduler,
            SyncTeeTimeScheduler syncTeeTimeScheduler
            ) {
            _logger = logger;
            _sampleScheduler = sampleScheduler;
            _syncTeeTimeScheduler = syncTeeTimeScheduler;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task Started() {
#if DEBUG
            //await _sampleScheduler.SampleJob();
            await _syncTeeTimeScheduler.SyncTeeTimeJob();
#else
            await _sampleScheduler.SampleJob();
            await _syncTeeTimeScheduler.SyncTeeTimeJob();
#endif
        }
    }
}
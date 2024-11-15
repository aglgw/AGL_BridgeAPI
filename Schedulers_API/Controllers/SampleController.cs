using AGL.Api.Schedulers_API.Interfaces;
using AGL.Api.ApplicationCore.Utilities;
using Microsoft.AspNetCore.Mvc;
using AGL.Api.API_Schedulers.Interfaces;

namespace AGL.Api.Schedulers_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SampleController : ControllerBase
    {

        private readonly ILogger<SampleController> _logger;
        private readonly ISampleService _sampleService;
        private readonly ISyncTeeTimeService _syncTeeTimeService;

        public SampleController(ILogger<SampleController> logger,
            ISampleService sampleService
            , ISyncTeeTimeService syncTeeTimeService)
        {
            _logger = logger;
            _sampleService = sampleService;
            _syncTeeTimeService = syncTeeTimeService;
        }


        /// <summary>
        /// 샘플 - 체크인 티타임 리스트 조회(Type EFCore ORM)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetCheckInTeeTimeList()
        {
            LogService.logInformation("SampleController>GetCheckInTeeTimeList Start");

            var rst = await _sampleService.GetCheckInTeeTimeList();

            return Ok(rst);
        }

        /// <summary>
        /// 샘플 - SP 조회 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> CallSp([FromQuery] string fieldId)
        {
            LogService.logInformation("SampleController>CallSp Start");

            var rst = await _sampleService.CallSp(fieldId);

            return Ok(rst);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> TestProtocol()
        {
            LogService.logInformation("SampleController>TestProtocol Start");

            var rst = await _sampleService.TestProtocol();

            return Ok(rst);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> SyncTeeTime()
        {
            LogService.logInformation("SampleController>SyncTeeTime Start");

            var rst = await _syncTeeTimeService.SyncTeeTime();

            return Ok(rst);
        }
    }
}

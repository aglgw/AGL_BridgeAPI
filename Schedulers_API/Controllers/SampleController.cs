using AGL.Api.Schedulers_API.Interfaces;
using AGL.Api.ApplicationCore.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace AGL.Api.Schedulers_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SampleController : ControllerBase
    {

        private readonly ILogger<SampleController> _logger;
        private readonly ISampleService _sampleService;

        public SampleController(ILogger<SampleController> logger,
            ISampleService sampleService)
        {
            _logger = logger;
            _sampleService = sampleService;
        }


        /// <summary>
        /// ���� - üũ�� ƼŸ�� ����Ʈ ��ȸ(Type EFCore ORM)
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
        /// ���� - SP ��ȸ 
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
    }
}
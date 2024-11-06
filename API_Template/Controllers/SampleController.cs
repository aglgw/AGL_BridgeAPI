using AGL.Api.Bridge_API.Interfaces;
using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models;
using AGL.Api.ApplicationCore.Models.Enum;
using AGL.Api.ApplicationCore.Models.Queries;
using AGL.Api.ApplicationCore.Utilities;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AGL.Api.Bridge_API.Controllers
{
    public class SampleController : ApiControllerBase
    {

        private readonly ILogger<SampleController> _logger;
        private readonly ISampleService _sampleService;

        public SampleController(ILogger<SampleController> logger, 
            ISampleService sampleService)
        {
            _logger = logger;
            _sampleService = sampleService;
        }

/*
        /// <summary>
        /// ���� - üũ�� ƼŸ�� ����Ʈ ��ȸ(Type EFCore ORM)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetCheckInTeeTimeList()
        {
            LogService.logInformation("dd");

            var rst = await _sampleService.GetCheckInTeeTimeList();

            return Ok(rst);
        }

        /// <returns></returns>
        /// <summary>
        /// ���� - SP ��ȸ 
        /// </summary>
        /// <param name="fieldId">�ʵ�����</param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> CallSp([FromQuery] string fieldId)
        {
            LogService.logInformation("dd");

            var rst = await _sampleService.CallSp(fieldId);

            return Ok(rst);
        }
        /// <summary>
        /// ���� - HTT20 ��ȸ
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetHTT()
        {
            LogService.logInformation("dd");

            var rst = await _sampleService.GetHTT();

            return Ok(rst);
        }


        /// <summary>
        /// ������ ����� ���̱׷��̼�
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> MigNation()
        {

            await _sampleService.MigNation();


            return Ok();

        }

        /// <summary>
        /// RquestTest_SUCCESS
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> RquestTest_SUCCESS()
        {

            var resultData = "resultData: SUCCESS 123";
            var result = new Success<dynamic>
            {
                Data = resultData
            };

            return Ok(result);
        }

        /// <summary>
        /// ExceptionTest
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> RquestTest_Exception_UNAUTHORIZED()
        {
            var testint = 1;
            if (testint == 1)
            {
                throw new DomainException(ResultCode.UNAUTHORIZED, "Unauthorized(StatusCode:401) �׽�Ʈ 123");
            }

            return Ok();
        }

        /// <summary>
        /// ExceptionTest
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> RquestTest_Exception_INVALID_INPUT()
        {
            var testint = 1;
            if (testint == 1)
            {
                throw new DomainException(ResultCode.INVALID_INPUT, "INVALID_INPUT(StatusCode:400) �׽�Ʈ 456");
            }

            return Ok();
        }

        /// <summary>
        /// ExceptionTest
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> RquestTest_Exception_Forbidden()
        {
            var testint = 1;
            if (testint == 1)
            {
                throw new DomainException(ResultCode.Forbidden, "Forbidden(StatusCode:403) �׽�Ʈ 789");
            }

            return Ok();
        }

        /// <summary>
        /// ExceptionTest
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> RquestTest_Exception_NOT_FOUND()
        {
            var testint = 1;
            if (testint == 1)
            {
                throw new DomainException(ResultCode.NOT_FOUND, "NOT_FOUND(StatusCode:404) �׽�Ʈ 0");
            }

            return Ok();
        }

        /// <summary>
        /// ExceptionTest
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> RquestTest_Exception_SERVER_ERROR()
        {
            var testint = 1;
            if (testint == 1)
            {
                throw new DomainException(ResultCode.SERVER_ERROR, "SERVER_ERROR(StatusCode:500) �׽�Ʈ 1");
            }

            return Ok();
        }

        /// <summary>
        /// LogTestWrite
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> LogTestWrite(
            [FromHeader(Name = "folderName")][Required] string folderName,
            [FromHeader(Name = "fileName")][Required] string fileName,
            [FromHeader(Name = "logTitle")][Required] string logTitle,
            [FromHeader(Name = "logTxt")] string logTxt
            )
        {
            Utils.UtilLogs.LogRegHour(folderName, fileName, logTitle, logTxt);

            Utils.UtilLogs.LogRegHour(folderName, fileName, "Error: " + logTitle, "Error: " + logTxt, true);

            Utils.UtilLogs.LogRegDay(folderName, fileName, logTitle, logTxt);

            Utils.UtilLogs.LogRegDay(folderName, fileName, "Error: " + logTitle, "Error: " + logTxt, true);

            await Task.CompletedTask;

            return Ok();
        }

        /// <summary>
        /// LogTestDelete
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> LogTestDelete(
            [FromHeader(Name = "logName")][Required] string logName,
            [FromHeader(Name = "limitDays")][Required] int limitDays
            )
        {
            Utils.UtilLogs.LogDelete(logName, limitDays);

            await Task.CompletedTask;

            return Ok();
        }*/
    }

}

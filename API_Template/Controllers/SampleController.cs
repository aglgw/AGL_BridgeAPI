using AGL.Api.API_Template.Interfaces;
using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using AGL.Api.ApplicationCore.Models.Queries;
using AGL.Api.ApplicationCore.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace AGL.Api.API_Template.Controllers
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


        /// <summary>
        /// 샘플 - 체크인 티타임 리스트 조회(Type EFCore ORM)
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
        /// 샘플 - SP 조회 
        /// </summary>
        /// <param name="fieldId">필드정보</param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> CallSp([FromQuery]string fieldId)
        {
            LogService.logInformation("dd");

            var rst = await _sampleService.CallSp(fieldId);

            return Ok(rst);
        }

        /// <summary>
        /// 샘플 - 마리아DB 조회
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetMaria()
        {
            LogService.logInformation("dd");

            var rst = await _sampleService.GetMaria();

            return Ok(rst);
        }


        /// <summary>
        /// 샘플 - HTT20 조회
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
        /// Maria DB Insert Test
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> SetMariaInsertTest(CancellationToken cancellationToken)
        {

            var rst = await _sampleService.SetMariaInsertTest(cancellationToken);

            return Ok(rst);
        }



        /// <summary>
        /// Maria DB update Test
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("[action]")]
        public async Task<IActionResult> SetMariaUpdateTest(CancellationToken cancellationToken)
        {

            var rst = await _sampleService.SetMariaUpdateTest(cancellationToken);

            return Ok(rst);
        }



        /// <summary>
        /// Maria DB delete Test
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("[action]")]
        public async Task<IActionResult> DelMariaDeleteTest(CancellationToken cancellationToken)
        {
            var rst = await _sampleService.DelMariaDeleteTest(cancellationToken);



            return Ok(rst);
        }



        /// <summary>
        /// 연관된 테이블 조회_ver.1
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetMariaMultiSelectSample()
        {
            var rst = await _sampleService.GetMariaMultiSelectSample();



            return Ok(rst);
        }


        /// <summary>
        /// 연관된 테이블 조회_ver.2 하위 엔티티 기준(inner Join 동일)
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetMariaSubEntitySelectSample()
        {
            var rst = await _sampleService.GetMariaSubEntitySelectSample();

            return Ok(rst);
        }


        /// <summary>
        /// 테이블 조인하기 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetMariaJoinSelectSample()
        {
            var rst = await _sampleService.GetMariaJoinSelectSample();

            return Ok(rst);
        }

        /// <summary>
        /// Expression 을 활용한 조회
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetMariaSelectExpressionSample()
        {
            var rst = await _sampleService.GetMariaSelectExpressionSample();

            return Ok(rst);
        }


        /// <summary>
        /// QueryFilter 을 활용한 조회
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetMariaSelectQueryFilterSample()
        {
            var rst = await _sampleService.GetMariaSelectQueryFilterSample();

            return Ok(rst);
        }


        /// <summary>
        /// 페이징
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetMariaSelectPagingSample([FromQuery]DomainQuery query)
        {
            var rst = await _sampleService.GetMariaSelectPagingSample(query);

            return Ok(rst);
        }


        /// <summary>
        /// 대용량 데이터 처리-BulkInsert
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> SetMariaBulkSample()
        {
            var rst = await _sampleService.SetMariaBulkSample();

            return Ok(rst);
        }


        /// <summary>
        /// 국가별 사용언어 마이그레이션
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

            return Ok();
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
                throw new DomainException(ResultCode.UNAUTHORIZED, "Unauthorized(StatusCode:401) 테스트 123");
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
                throw new DomainException(ResultCode.INVALID_INPUT, "INVALID_INPUT(StatusCode:400) 테스트 456");
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
                throw new DomainException(ResultCode.Forbidden, "Forbidden(StatusCode:403) 테스트 789");
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
                throw new DomainException(ResultCode.NOT_FOUND, "NOT_FOUND(StatusCode:404) 테스트 0");
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
                throw new DomainException(ResultCode.SERVER_ERROR, "SERVER_ERROR(StatusCode:500) 테스트 1");
            }

            return Ok();
        }
    }

}

using AGL.Api.API_Template.Interfaces;
using AGL.Api.API_Template.Models.OAPI;
using AGL.Api.ApplicationCore.Extensions;
using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using Microsoft.AspNetCore.Mvc;
using static AGL.Api.API_Template.Models.OAPI.OAPIResponse;

namespace AGL.Api.API_Template.Controllers
{
    public class OAPIController : ApiControllerBase
    {
        private readonly ILogger<OAPIController> _logger;
        private readonly IOAPIService _oapiService;

        public OAPIController(ILogger<OAPIController> logger,IOAPIService oapiService)
        {
            _logger = logger;
            _oapiService = oapiService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="Exception"></exception>
        [Route("teetime/{golfclubCode}")]
        [HttpPost]
        public async Task<IDataResult> PostTeeTime(OAPITeeTimePostRequest request)
        {
            // 인증
            AuthorizeRequest();

            // 헤더에서 공급사 코드 가져오기
            string supplierCode = Request.Headers["X-Supplier-Code"];

            var result = await _oapiService.PostTeeTime(request, supplierCode);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="Exception"></exception>
        [Route("teetime")]
        [HttpPut]
        public async Task<IDataResult> UpdateTeeTime(string golfclubCode, OAPITeeTimePutRequest request)
        {
            // 인증
            AuthorizeRequest();

            // 헤더에서 공급사 코드 가져오기
            string supplierCode = Request.Headers["X-Supplier-Code"];

            var result = await _oapiService.UpdateTeeTime(golfclubCode, request, supplierCode);

            return result;
        }

    }
}

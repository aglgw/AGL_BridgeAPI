using AGL.Api.API_Template.Interfaces;
using AGL.Api.API_Template.Models.OAPI;
using AGL.Api.ApplicationCore.Extensions;
using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
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
        /// 티타임 등록
        /// </summary>
        /// <returns></returns>
        [Route("teetime")]
        [HttpPost]
        public async Task<IDataResult> PostTeeTime(
            [FromHeader(Name = "X-Supplier-Code")][Required] string X_Supplier_Code, OAPITeeTimeRequest request)
        {
            //OAPIResponseBase response = new OAPIResponseBase();

            //if (!ModelState.IsValid)
            //{
            //    _logger.LogError("ModelState 오류: {@ModelState}", ModelState);
            //    return response;
            //}

            var result = await _oapiService.PostTeeTime(request, X_Supplier_Code);

            return result;
        }

        /// <summary>
        /// 티타임 수정
        /// </summary>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="Exception"></exception>
        [Route("teetime/update")]
        [HttpPut]
        public async Task<IDataResult> UpdateTeeTime(
        [FromHeader(Name = "X-Supplier-Code")][Required] string X_Supplier_Code, OAPITeeTimeRequest request)
            {
            //OAPIResponseBase response = new OAPIResponseBase();

            //if (!ModelState.IsValid)
            //{
            //    _logger.LogError("ModelState 오류: {@ModelState}", ModelState);
            //    return response;
            //}

            var result = await _oapiService.UpdateTeeTime(request, X_Supplier_Code);

            return result;
        }

        /// <summary>
        /// 티타임 등록
        /// </summary>
        /// <returns></returns>
        [Route("reservation/confirm")]
        [HttpPost]
        public async Task<IDataResult> PostReservatioConfirm(
            [FromHeader(Name = "X-Supplier-Code")][Required] string X_Supplier_Code, OAPIReservationRequest request)
        {
            //OAPIResponseBase response = new OAPIResponseBase();

            //if (!ModelState.IsValid)
            //{
            //    _logger.LogError("ModelState 오류: {@ModelState}", ModelState);
            //    return response;
            //}

            var result = await _oapiService.PostReservatioConfirm(request, X_Supplier_Code);

            return result;
        }

        /// <summary>
        /// 티타임 조회
        /// </summary>
        /// <returns></returns>
        [Route("teetimeList")]
        [HttpGet]
        public async Task<IDataResult> GetTeeTime(
            [FromHeader(Name = "X-Supplier-Code")][Required] string X_Supplier_Code, OAPITeeTimeGetRequest request)
        {
            //OAPIResponseBase response = new OAPIResponseBase();

            //if (!ModelState.IsValid)
            //{
            //    _logger.LogError("ModelState 오류: {@ModelState}", ModelState);
            //    return response;
            //}

            var result = await _oapiService.GetTeeTime(request, X_Supplier_Code);

            return result;
        }

        /// <summary>
        /// 티타임 상태 수정
        /// </summary>
        /// <returns></returns>
        [Route("teetimeList")]
        [HttpPut]
        public async Task<IDataResult> PutTeeTimeAvailability(
            [FromHeader(Name = "X-Supplier-Code")][Required] string X_Supplier_Code, OAPITeeTimetAvailabilityRequest request)
        {
            //OAPIResponseBase response = new OAPIResponseBase();

            //if (!ModelState.IsValid)
            //{
            //    _logger.LogError("ModelState 오류: {@ModelState}", ModelState);
            //    return response;
            //}

            var result = await _oapiService.PutTeeTimeAvailability(request, X_Supplier_Code);

            return result;
        }
        
    }
}

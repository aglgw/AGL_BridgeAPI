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
    public class TeeTimeController : ApiControllerBase
    {
        private readonly ILogger<TeeTimeController> _logger;
        private readonly ITeeTimeService _teetimeService;

        public TeeTimeController(ILogger<TeeTimeController> logger,ITeeTimeService teetimeService)
        {
            _logger = logger;
            _teetimeService = teetimeService;
        }

        /// <summary>
        /// 티타임 등록
        /// </summary>
        /// <returns></returns>
        [Route("teetime")]
        [HttpPost]
        public async Task<IDataResult> PostTeeTime(
            [FromHeader(Name = "X-Supplier-Code")][Required] string SupplierCode, TeeTimeRequest request)
        {
            var result = await _teetimeService.PostTeeTime(request, SupplierCode);

            return result;
        }

        /// <summary>
        /// 티타임 수정
        /// </summary>
        /// <returns></returns>
        [Route("teetime/update")]
        [HttpPut]
        public async Task<IDataResult> PutTeeTime(
            [FromHeader(Name = "X-Supplier-Code")][Required] string SupplierCode, TeeTimeRequest request)
        {
            var result = await _teetimeService.PutTeeTime(request, SupplierCode);

            return result;
        }

        /// <summary>
        /// 티타임 조회
        /// </summary>
        /// <returns></returns>
        [Route("teetime/list")]
        [HttpGet]
        public async Task<IDataResult> GetTeeTime(
            [FromHeader(Name = "X-Supplier-Code")][Required] string SupplierCode, TeeTimeGetRequest request)
        {
            var result = await _teetimeService.GetTeeTime(request, SupplierCode);

            return result;
        }

        /// <summary>
        /// 티타임 상태 수정
        /// </summary>
        /// <returns></returns>
        [Route("teetime/availability")]
        [HttpPut]
        public async Task<IDataResult> PutTeeTimeAvailability(
            [FromHeader(Name = "X-Supplier-Code")][Required] string SupplierCode, TeeTimetAvailabilityRequest request)
        {
            var result = await _teetimeService.PutTeeTimeAvailability(request, SupplierCode);

            return result;
        }
        
    }
}

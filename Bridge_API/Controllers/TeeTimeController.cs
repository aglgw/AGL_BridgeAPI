using AGL.Api.Bridge_API.Interfaces;
using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using AGL.Api.ApplicationCore.Filters;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIRequest;

namespace AGL.Api.Bridge_API.Controllers
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
            Utils.UtilLogs.LogRegHour(SupplierCode, request.golfClubCode, "TeeTime", "등록 처리 시작");
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
            Utils.UtilLogs.LogRegHour(SupplierCode, request.golfClubCode, "TeeTime", "변경 처리 시작");
            var result = await _teetimeService.PutTeeTime(request, SupplierCode);

            return result;
        }

        /// <summary>
        /// 티타임 조회
        /// </summary>
        /// <returns></returns>
        [Route("teetime/list")]
        [HttpGet]
        [DisableValidation]
        public async Task<IDataResult> GetTeeTime(
            [FromHeader(Name = "X-Supplier-Code")][Required] string SupplierCode,
            [FromQuery][Required] TeeTimeGetRequest request)
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
            [FromHeader(Name = "X-Supplier-Code")][Required] string SupplierCode, TeeTimeAvailabilityRequest request)
        {
            var result = await _teetimeService.PutTeeTimeAvailability(request, SupplierCode);

            return result;
        }
        
    }
}

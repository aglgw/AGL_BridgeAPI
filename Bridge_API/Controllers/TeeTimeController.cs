using AGL.Api.Bridge_API.Interfaces;
using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using AGL.Api.ApplicationCore.Filters;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIRequest;
using static AGL.Api.Bridge_API.Models.OAPI.OAPI;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIResponse;
using static AGL.Api.Bridge_API.Utils.Util;
using Microsoft.AspNetCore.Http.Extensions;

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
        /// Registers a new tee time for a golf club
        /// </summary>
        /// <returns></returns>
        [Route("teetime")]
        [HttpPost]
        public async Task<ActionResult<IDataResult>> PostTeeTime(
            [FromHeader(Name = "X-Supplier-Code")][Required] string SupplierCode, TeeTimeRequest request)
        {
            Utils.UtilLogs.LogRegHour(SupplierCode, request.golfClubCode, "ProcessTeeTime", "등록 처리 시작");
            var result = await _teetimeService.PostTeeTime(request, SupplierCode);

            return ResponseUtil.HandleResponse(result);
        }

        /// <summary>
        /// Updates the details of an existing tee time
        /// </summary>
        /// <returns></returns>
        [Route("teetime/update")]
        [HttpPut]
        public async Task<ActionResult<IDataResult>> PutTeeTime(
            [FromHeader(Name = "X-Supplier-Code")][Required] string supplierCode, TeeTimeRequest request)
        {
            Utils.UtilLogs.LogRegHour(supplierCode, request.golfClubCode, "ProcessTeeTime", "변경 처리 시작");
            var result = await _teetimeService.PutTeeTime(request, supplierCode);

            return ResponseUtil.HandleResponse(result);
        }

        /// <summary>
        /// Retrieves tee time information based on specified criteria
        /// </summary>
        /// <returns></returns>
        [Route("teetime/list")]
        [HttpGet]
        [DisableValidation]
        public async Task<ActionResult<TeeTimeResponse>> GetTeeTime(
            [FromHeader(Name = "X-Supplier-Code")][Required] string supplierCode,
            [FromQuery][Required] TeeTimeGetRequest request)
        {
            var currentUrl = HttpContext.Request.GetDisplayUrl();
            Utils.UtilLogs.LogRegHour(supplierCode, request.golfClubCode, "TeeTimeList", $"티타임 검색 URL : {currentUrl}");

            var result = await _teetimeService.GetTeeTime(request, supplierCode);

            return ResponseUtil.HandleResponse(result);
        }

        /// <summary>
        /// Changes the status of a tee time (e.g., available, reserved).
        /// </summary>
        /// <returns></returns>
        [Route("teetime/availability")]
        [HttpPut]
        public async Task<ActionResult<IDataResult>> PutTeeTimeAvailability(
            [FromHeader(Name = "X-Supplier-Code")][Required] string supplierCode, TeeTimeAvailabilityRequest request)
        {
            var result = await _teetimeService.PutTeeTimeAvailability(request, supplierCode);

            return ResponseUtil.HandleResponse(result);
        }
        
    }
}

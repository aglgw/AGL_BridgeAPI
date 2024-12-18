using AGL.Api.Bridge_API.Interfaces;
using AGL.Api.Bridge_API.Models.OAPI;
using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIResponse;
using static AGL.Api.Bridge_API.Models.OAPI.OAPI;
using static AGL.Api.Bridge_API.Utils.Util;
using Microsoft.AspNetCore.Http.Extensions;
using Azure.Core;


namespace AGL.Api.Bridge_API.Controllers
{
    [ApiController]
    public class GolfClubController : ApiControllerBase
    {
        private readonly ILogger<GolfClubController> _logger;
        private readonly IGolfClubService _golfService;

        public GolfClubController(ILogger<GolfClubController> logger, IGolfClubService golfService)
        {
            _logger = logger;
            _golfService = golfService;
        }

        /// <summary>
        /// Registers a new golf club in the system
        /// </summary>
        /// <returns></returns>
        [Route("golfclub")]
        [HttpPost]
        public async Task<ActionResult<IDataResult>> PostGolfClub(
            [FromHeader(Name = "X-Supplier-Code")][Required] string supplierCode, OAPI.GolfClubInfo request)
        {
            Utils.UtilLogs.LogRegHour(supplierCode, request.golfClubCode, "ProcessGolfClub", "골프장 등록 처리 시작");
            var result = await _golfService.PostGolfClub(request, supplierCode);

            return ResponseUtil.HandleResponse(result);
        }

        /// <summary>
        /// Updates information for an existing golf club
        /// </summary>
        /// <returns></returns>
        [Route("golfclub/update")]
        [HttpPut]
        public async Task<ActionResult<IDataResult>> PutGolfClub(
            [FromHeader(Name = "X-Supplier-Code")][Required] string supplierCode, OAPI.GolfClubInfo request)
        {
            Utils.UtilLogs.LogRegHour(supplierCode, request.golfClubCode, "ProcessGolfClub", "골프장 변경 처리 시작");
            var result = await _golfService.PutGolfClub(request, supplierCode);

            return ResponseUtil.HandleResponse(result);
        }

        /// <summary>
        /// Retrieves details of a specific golf club or a list of golf clubs
        /// </summary>
        /// <returns></returns>
        [Route("golfclub/list")]
        [HttpGet]
        //[DisableValidation]
        public async Task<ActionResult<OAPIDataResponse<List<GolfClubInfo>>>> GetGolfClub(
            [FromHeader(Name = "X-Supplier-Code")][Required] string supplierCode,
            [FromQuery(Name = "GolfclubCode")] string? golfclubCode)
        {
            var currentUrl = HttpContext.Request.GetDisplayUrl();
            Utils.UtilLogs.LogRegHour(supplierCode, golfclubCode, "GolfClubList", $"골프장 검색 URL : {currentUrl}");

            var result = await _golfService.GetGolfClub(supplierCode, golfclubCode);

            return ResponseUtil.HandleResponse(result);
        }

    }
}

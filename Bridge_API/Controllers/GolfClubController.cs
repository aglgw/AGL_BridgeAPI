using AGL.Api.Bridge_API.Interfaces;
using AGL.Api.Bridge_API.Models.OAPI;
using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIResponse;
using static AGL.Api.Bridge_API.Models.OAPI.OAPI;


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
        public async Task<IDataResult> PostGolfClub(
            [FromHeader(Name = "X-Supplier-Code")][Required] string SupplierCode, OAPI.GolfClubInfo request)
        {
            Utils.UtilLogs.LogRegHour(SupplierCode, request.golfClubCode, "GolfClub", "골프장 등록 처리 시작");
            var result = await _golfService.PostGolfClub(request, SupplierCode);

            return result;
        }

        /// <summary>
        /// Updates information for an existing golf club
        /// </summary>
        /// <returns></returns>
        [Route("golfclub/update")]
        [HttpPut]
        public async Task<IDataResult> PutGolfClub(
            [FromHeader(Name = "X-Supplier-Code")][Required] string SupplierCode, OAPI.GolfClubInfo request)
        {
            Utils.UtilLogs.LogRegHour(SupplierCode, request.golfClubCode, "GolfClub", "골프장 변경 처리 시작");
            var result = await _golfService.PutGolfClub(request, SupplierCode);

            return result;
        }

        /// <summary>
        /// Retrieves details of a specific golf club or a list of golf clubs
        /// </summary>
        /// <returns></returns>
        [Route("golfclub/list")]
        [HttpGet]
        //[DisableValidation]
        public async Task<OAPIDataResponse<List<GolfClubInfo>>> GetGolfClub(
            [FromHeader(Name = "X-Supplier-Code")][Required] string SupplierCode,
            [FromQuery(Name = "GolfclubCode")] string? GolfclubCode)
        {
            var result = await _golfService.GetGolfClub(SupplierCode, GolfclubCode);

            return result;
        }

    }
}

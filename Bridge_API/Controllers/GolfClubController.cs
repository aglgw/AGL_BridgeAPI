using AGL.Api.Bridge_API.Interfaces;
using AGL.Api.Bridge_API.Models.OAPI;
using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using static AGL.Api.Bridge_API.Models.OAPI.OAPI;
using AGL.Api.ApplicationCore.Models.Enum;
using AGL.Api.Bridge_API.Utils;
using AGL.Api.ApplicationCore.Filters;


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
        /// 골프장 등록
        /// </summary>
        /// <returns></returns>
        [Route("golfclub")]
        [HttpPost]
        public async Task<IDataResult> PostGolfClub(
            [FromHeader(Name = "X-Supplier-Code")][Required] string SupplierCode, GolfClubInfo request)
        {
            Utils.UtilLogs.LogRegHour(SupplierCode, request.golfClubCode, "GolfClub", "골프장 등록 처리 시작");
            var result = await _golfService.PostGolfClub(request, SupplierCode);

            return result;
        }

        /// <summary>
        /// 골프장 수정
        /// </summary>
        /// <returns></returns>
        [Route("golfclub/update")]
        [HttpPut]
        public async Task<IDataResult> PutGolfClub(
            [FromHeader(Name = "X-Supplier-Code")][Required] string SupplierCode, GolfClubInfo request)
        {
            Utils.UtilLogs.LogRegHour(SupplierCode, request.golfClubCode, "GolfClub", "골프장 변경 처리 시작");
            var result = await _golfService.PutGolfClub(request, SupplierCode);

            return result;
        }

        /// <summary>
        /// 골프장 조회
        /// </summary>
        /// <returns></returns>
        [Route("golfclub/list")]
        [HttpGet]
        //[DisableValidation]
        public async Task<IDataResult> GetGolfClub(
            [FromHeader(Name = "X-Supplier-Code")][Required] string SupplierCode,
            [FromQuery(Name = "GolfclubCode")] string? GolfclubCode)
        {
            var result = await _golfService.GetGolfClub(SupplierCode, GolfclubCode);

            return result;
        }

    }
}

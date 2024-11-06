using AGL.Api.API_Template.Interfaces;
using AGL.Api.API_Template.Models.OAPI;
using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using static AGL.Api.API_Template.Models.OAPI.OAPI;

namespace AGL.Api.API_Template.Controllers
{
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
        public async Task<IDataResult> PostTeeTime(
            [FromHeader(Name = "X-Supplier-Code")][Required] string SupplierCode, GolfClubInfo request)
        {
            var result = await _golfService.PostGolfClub(request, SupplierCode);

            return result;
        }

        /// <summary>
        /// 골프장 수정
        /// </summary>
        /// <returns></returns>
        [Route("golfclub/update")]
        [HttpPut]
        public async Task<IDataResult> PutTeeTime(
            [FromHeader(Name = "X-Supplier-Code")][Required] string SupplierCode, GolfClubInfo request)
        {
            var result = await _golfService.PutGolfClub(request, SupplierCode);

            return result;
        }

        /// <summary>
        /// 골프장 조회
        /// </summary>
        /// <returns></returns>
        [Route("golfclubList")]
        [HttpGet]
        public async Task<IDataResult> GetTeeTime(
            [FromHeader(Name = "X-Supplier-Code")][Required] string SupplierCode,
            [FromQuery(Name = "GolfclubCode")] string? GolfclubCode)
        {
            var result = await _golfService.GetGolfClub(SupplierCode, GolfclubCode);

            return result;
        }

    }
}

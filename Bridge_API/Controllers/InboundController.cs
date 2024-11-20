using AGL.Api.Bridge_API.Interfaces;
using AGL.Api.Bridge_API.Models.OAPI;
using AGL.Api.Bridge_API.Services;
using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using static AGL.Api.Bridge_API.Models.OAPI.Inbound;
using AGL.Api.ApplicationCore.Filters;
using static AGL.Api.Bridge_API.Models.OAPI.OAPI;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIResponse;

namespace AGL.Api.Bridge_API.Controllers
{
    public class InboundController : ApiControllerBase
    {
        private readonly ILogger<InboundController> _logger;
        private readonly IInboundService _InboundService;

        public InboundController(ILogger<InboundController> logger, IInboundService InboundService)
        {
            _logger = logger;
            _InboundService = InboundService;
        }

        /// <summary>
        /// 내부연동 티타임 기간조회
        /// </summary>
        /// <returns></returns>
        [DisableValidation]
        [Route("inbound/TeeTimeTable")]
        [HttpGet]
        public async Task<IDataResult> GetInboundTeeTime(
            [FromQuery] InboundTeeTimeRequest request)
        {
            var result = await _InboundService.GetInboundTeeTime(request);

            return result;
        }

        /// <summary>
        /// 골프장 조회
        /// </summary>
        /// <returns></returns>
        [Route("inbound/golfclub/list")]
        [HttpGet]
        //[DisableValidation]
        public async Task<OAPICommonListResponse<GolfClubInfoWithInboundCode>> GetInboundGolfClub(
            [FromQuery(Name = "inboundCode")] string? inboundCode)
        {
            var result = await _InboundService.GetInboundGolfClub(inboundCode);

            return result;
        }

    }
}

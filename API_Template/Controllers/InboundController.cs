using AGL.Api.API_Template.Interfaces;
using AGL.Api.API_Template.Models.OAPI;
using AGL.Api.API_Template.Services;
using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using static AGL.Api.API_Template.Models.OAPI.Inbound;

namespace AGL.Api.API_Template.Controllers
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
        [Route("inbound/TeeTimeTable")]
        [HttpGet]
        public async Task<IDataResult> GetInboundTeeTime([FromQuery] InboundTeeTimeRequest request)
        {
            var result = await _InboundService.GetInboundTeeTime(request);

            return result;
        }


    }
}

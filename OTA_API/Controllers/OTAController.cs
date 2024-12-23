using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.OTA_API.Interfaces;
using AGL.Api.OTA_API.Services;
using Microsoft.AspNetCore.Mvc;
using static AGL.Api.OTA_API.Utils.Util;
using System.ComponentModel.DataAnnotations;
using AGL.Api.OTA_API.Models.OAPI;
using static AGL.Api.OTA_API.Models.OAPI.OTARequest;
using AGL.Api.ApplicationCore.Filters;

namespace AGL.Api.OTA_API.Controllers
{
    public class OTAController : ApiControllerBase
    {
        private readonly ILogger<OTAController> _logger;
        private readonly IOTAService _otaService;

        public OTAController(ILogger<OTAController> logger,
            IOTAService otaService)
        {
            _logger = logger;
            _otaService = otaService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("ota/tracking")]
        [HttpPost]
        public async Task<ActionResult<IOTAResult>> CreateTracking(OTARequestBase request)
        {
            var result = await _otaService.CreateTracking(request);

            return ResponseUtil.HandleResponse(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("ota/tracking/{trackingId}")]
        [HttpPut]
        public async Task<ActionResult<IOTAResult>> UpdateTracking(OTARequestBase request,
            [FromQuery(Name = "trackingId")][Required] string trackingId)
        {
            var result = await _otaService.UpdateTracking(request);

            return ResponseUtil.HandleResponse(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("ota/tracking/{trackingId}")]
        [HttpDelete]
        public async Task<ActionResult<IOTAResult>> DeleteTracking(OTARequestBase request,
            [FromQuery(Name = "trackingId")][Required] string trackingId)
        {
            var result = await _otaService.DeleteTracking(request);

            return ResponseUtil.HandleResponse(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("ota/tracking/list")]
        [HttpGet]
        [DisableValidation]
        public async Task<ActionResult<IOTAResult>> GetTrackingList(OTARequestBase request)
        {
            var result = await _otaService.GetTrackingList(request);

            return ResponseUtil.HandleResponse(result);
        }
    }
}

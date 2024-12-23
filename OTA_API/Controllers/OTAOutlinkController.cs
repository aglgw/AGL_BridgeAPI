using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;
using static AGL.Api.OTA_API.Models.OAPI.OTARequest;
using static AGL.Api.OTA_API.Utils.Util;
using System.ComponentModel.DataAnnotations;
using AGL.Api.OTA_API.Interfaces;
using AGL.Api.ApplicationCore.Filters;
using static AGL.Api.OTA_API.Models.OAPI.OTA;
using static AGL.Api.OTA_API.Models.OAPI.OTAResponse;

namespace AGL.Api.OTA_API.Controllers
{
    public class OTAOutlinkController : ApiControllerBase
    {
        private readonly ILogger<OTAOutlinkController> _logger;
        private readonly IOTAOutlinkService _otaOutlinkService;

        public OTAOutlinkController(ILogger<OTAOutlinkController> logger,
            IOTAOutlinkService otaOutlinkService)
        {
            _logger = logger;
            _otaOutlinkService = otaOutlinkService;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("ota/outlink/list")]
        [HttpGet]
        [DisableValidation]
        public async Task<ActionResult<OTADataResponseBase<List<Outlink>>>> GetOutlinkData(OTARequestBase request,
            [FromQuery(Name = "trackingId")][Required] string trackingId)
        {
            var result = await _otaOutlinkService.GetOutlinkData(request);

            return ResponseUtil.HandleResponse(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("ota/outlink/updatelist")]
        [HttpGet]
        [DisableValidation]
        public async Task<ActionResult<OTADataResponseBase<List<Outlink>>>> GetUpdatedOutlink(OTARequestBase request,
            [FromQuery(Name = "trackingId")][Required] string trackingId)
        {
            var result = await _otaOutlinkService.GetUpdatedOutlink(request);

            return ResponseUtil.HandleResponse(result);
        }

    }
}

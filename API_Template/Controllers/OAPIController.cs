using AGL.Api.API_Template.Interfaces;
using AGL.Api.API_Template.Models.OAPI;
using AGL.Api.ApplicationCore.Extensions;
using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using static AGL.Api.API_Template.Models.OAPI.OAPIResponse;

namespace AGL.Api.API_Template.Controllers
{
    public class OAPIController : ApiControllerBase
    {
        private readonly ILogger<OAPIController> _logger;
        private readonly IOAPIService _oapiService;

        public OAPIController(ILogger<OAPIController> logger,IOAPIService oapiService)
        {
            _logger = logger;
            _oapiService = oapiService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("teetime/{golfclubCode}")]
        [HttpPost]
        public async Task<IDataResult> PostTeeTime(
            [FromHeader(Name = "X-Supplier-Code")][Required] string X_Supplier_Code,
            [FromHeader(Name = "Authorization")][Required] string Authorization,
            OAPITeeTimePostRequest request)
        {
            var result = await _oapiService.PostTeeTime(request, X_Supplier_Code);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="Exception"></exception>
        [Route("teetime")]
        [HttpPut]
        public async Task<IDataResult> UpdateTeeTime(
            [FromHeader(Name = "X-Supplier-Code")][Required] string X_Supplier_Code,
            [FromHeader(Name = "Authorization")][Required] string Authorization,
            string golfclubCode, OAPITeeTimePutRequest request)
        {
            var result = await _oapiService.UpdateTeeTime(golfclubCode, request, X_Supplier_Code);

            return result;
        }

    }
}

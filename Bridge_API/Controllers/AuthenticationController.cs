using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.Bridge_API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using AGL.Api.ApplicationCore.Filters;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIRequest;

namespace AGL.Api.Bridge_API.Controllers
{
    public class AuthenticationController
    {
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(ILogger<AuthenticationController> logger, IAuthenticationService authenticationService)
        {
            _logger = logger;
            _authenticationService = authenticationService;
        }

#if DEBUG
        /// <summary>
        /// 인증 등록
        /// </summary>
        /// <returns></returns>
        [Route("auth")]
        [HttpPost]
        [SkipAuthentication] // 인증 미들웨어 패스
        public async Task<IDataResult> PostAuthentication(
            [FromHeader][Required] string token,
            AuthenticationRequest request)
        {
            var result = await _authenticationService.PostAuthentication(request, token);

            return result;
        }

        /// <summary>
        /// 인증 검색
        /// </summary>
        /// <returns></returns>
        [Route("auth/list")]
        [HttpGet]
        [SkipAuthentication] // 인증 미들웨어 패스
        public async Task<IDataResult> GetAuthentication(
            [FromHeader][Required] string token,
            [FromQuery][Required] AuthenticationRequest request)
        {
            var result = await _authenticationService.GetAuthentication(request, token);

            return result;
        }
#endif

    }
}

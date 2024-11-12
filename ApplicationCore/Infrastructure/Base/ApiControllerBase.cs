using AGL.Api.ApplicationCore.Filters;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using AGL.Api.ApplicationCore.Models.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace AGL.Api.ApplicationCore.Infrastructure
{
    [ApiController]
    [ValidateJsonPropertiesFilter] // dto 유효성 체크 필터
    [CustomResponseFilter] // 커스텀 response 필터
    //[DisableValidation] // 메서드 전용 유효성 제외
    //[Route("api/v1/[controller]")]
    [Route("api")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class ApiControllerBase : ControllerBase
    {
        private IIdentityService _identityService;
        protected IIdentityService IdentityService => _identityService ??= HttpContext.RequestServices.GetService<IIdentityService>();

        protected string ClientId => IdentityService.ClientId;

        protected int UserId => IdentityService.UserId;
        protected string Language => IdentityService.Language;
        protected string Currency => IdentityService.Currency;
        protected ClientQuery ClientQuery => new ClientQuery
        {
            ClientId = this.ClientId,
            UserId = this.UserId,
            Language = this.Language,
            Currency = this.Currency,
        };

        protected string ReqeustDevice => HttpContext.Request.Headers.ContainsKey("x-request-device") ? HttpContext.Request.Headers["x-request-device"] : "PC";

        protected string ReqeustIP => HttpContext.Connection.RemoteIpAddress.ToString();
    }
}

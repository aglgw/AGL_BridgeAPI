using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace AGL.Api.ApplicationCore.Infrastructure
{
    [ApiController]
    [Route("api/v1/[controller]")]
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

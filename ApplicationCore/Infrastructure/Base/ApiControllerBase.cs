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

        // 인증 메서드 추가
        protected void AuthorizeRequest()
        {
            if (!HttpContext.Request.Headers.TryGetValue("Authorization", out var authorizationHeader) ||
                !HttpContext.Request.Headers.TryGetValue("X-Supplier-Code", out var supplierCodeHeader))
            {
                throw new DomainException(ResultCode.UNAUTHORIZED, "Unauthorized(StatusCode:401) Missing Authorization or Supplier Code header");
            }

            var token = authorizationHeader.ToString().Replace("Bearer ", "").Trim();
            var supplierCode = supplierCodeHeader.ToString();

            // DB에서 조회 및 인증 로직 추가
            var dbService = HttpContext.RequestServices.GetService<IMyDatabaseService>();
            var supplier = dbService?.GetSupplierByCodeAsync(supplierCode).Result;

            if (supplier == null)
            {
                throw new DomainException(ResultCode.UNAUTHORIZED, "Unauthorized(StatusCode:401) Invalid Supplier Code");
            }

            using (var sha256 = SHA256.Create())
            {
                var tokenBytes = Encoding.UTF8.GetBytes(supplier.TokenSupplierToAgl);
                var hashedBytes = sha256.ComputeHash(tokenBytes);
                var hashedToken = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();

                if (hashedToken != token)
                {
                    throw new DomainException(ResultCode.UNAUTHORIZED, "Unauthorized(StatusCode:401) Invalid token");
                }
            }
        }


    }
}

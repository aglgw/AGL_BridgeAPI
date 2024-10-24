using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using System.Text;
using System.Security.Cryptography;
using AGL.Api.Domain.Entities.OAPI;

namespace AGL.Api.ApplicationCore.Middleware
{
    public class SupplierAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SupplierAuthorizationMiddleware> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public SupplierAuthorizationMiddleware(RequestDelegate next, ILogger<SupplierAuthorizationMiddleware> logger, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 헤더에서 값을 가져옵니다.
            var supplierCodeHeader = context.Request.Headers["X-Supplier-Code"].FirstOrDefault();
            var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");

            if (string.IsNullOrEmpty(supplierCodeHeader) || string.IsNullOrEmpty(authorizationHeader))
            {
                throw new DomainException(ResultCode.UNAUTHORIZED, "Unauthorized(StatusCode:401)");
            }

            // IServiceScopeFactory를 사용하여 스코프 생성 후 OAPI_DbContext 사용
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<OAPI_DbContext>();
                //var dbContext = context.RequestServices.GetRequiredService<OAPI_DbContext>();

                // DB에서 SupplierCode를 조회합니다.
                var supplier = dbContext.Suppliers.FirstOrDefault(s => s.SupplierCode == supplierCodeHeader);

                if (supplier == null)
                {
                    throw new DomainException(ResultCode.UNAUTHORIZED, "Unauthorized(StatusCode:401) Invalid Supplier Code");
                }

                // TokenSupplierToAgl 값을 SHA-256 해싱합니다.
                using (var sha256 = SHA256.Create())
                {
                    var tokenHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(supplier.TokenSupplierToAgl));
                    var tokenHashString = BitConverter.ToString(tokenHash).Replace("-", "").ToLower();

                    // Authorization 헤더와 해시값 비교
                    if (authorizationHeader != tokenHashString)
                    {
                        throw new DomainException(ResultCode.UNAUTHORIZED, "Unauthorized(StatusCode:401) Invalid Authorization token");
                    }
                }
            }
            // 다음 미들웨어로 요청 전달
            await _next(context);
        }

        
    }

}

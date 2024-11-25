using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using System.Text;
using System.Security.Cryptography;
using AGL.Api.Domain.Entities.OAPI;
using Microsoft.EntityFrameworkCore;
using AGL.Api.ApplicationCore.Utilities;
using AGL.Api.ApplicationCore.Filters;

namespace AGL.Api.ApplicationCore.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            // Endpoint에서 특성을 확인
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<SkipAuthenticationAttribute>() != null)
            {
                await _next(context); // 인증을 건너뜀
                return;
            }

            using (var scope = _scopeFactory.CreateScope())
            {
                var myDatabaseService = scope.ServiceProvider.GetRequiredService<IMyDatabaseService>();

                // 헤더에서 값을 가져옵니다.
                var headerSupplierCode = context.Request.Headers["X-Supplier-Code"].FirstOrDefault();
                var signature = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");

                if (string.IsNullOrEmpty(headerSupplierCode) || string.IsNullOrEmpty(signature))
                {
                    throw new DomainException(ResultCode.UNAUTHORIZED, "Unauthorized(StatusCode:401)");
                }

                // DB에서 공급업체 코드를 조회하고 서명 검증 로직 수행
                var supplier = await myDatabaseService.GetSupplierByCodeAsync(headerSupplierCode);
                if (supplier == null)
                {
                    throw new DomainException(ResultCode.UNAUTHORIZED, "Unauthorized(StatusCode:401) supplier not found");
                }

                // SHA-256으로 해싱된 서명 비교
                var expectedSignature = ComputeSha256.ComputeSha256Hash(supplier.Authentication.TokenSupplier);
                if (signature != expectedSignature)
                {
                    throw new DomainException(ResultCode.UNAUTHORIZED, "Unauthorized(StatusCode:401) signature Unauthorized");
                }
            }

            // 다음 미들웨어로 요청을 전달
            await _next(context);
        }


    }

}

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using System.Text;
using System.Security.Cryptography;


namespace AGL.Api.ApplicationCore.Middleware
{
    public class SupplierAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SupplierAuthorizationMiddleware> _logger;
        private readonly IOAPIDbContext _dbContext;

        public SupplierAuthorizationMiddleware(RequestDelegate next, ILogger<SupplierAuthorizationMiddleware> logger, IOAPIDbContext dbContext)
        {
            _next = next;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task Invoke(HttpContext context)
        {
            // 헤더에서 값을 가져옵니다.
            var supplierCodeHeader = context.Request.Headers["X-Supplier-Code"].FirstOrDefault();
            var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");

            if (string.IsNullOrEmpty(supplierCodeHeader) || string.IsNullOrEmpty(authorizationHeader) ) {
                throw new DomainException(ResultCode.UNAUTHORIZED, "Unauthorized(StatusCode:401)");
            }

            // DB에서 SupplierCode를 조회합니다.
            var supplier = _dbContext.Suppliers.FirstOrDefault(s => s.SupplierCode == supplierCodeHeader);

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

    }
}

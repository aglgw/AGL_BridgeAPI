using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;

namespace AGL.Api.ApplicationCore.Middleware
{
    public static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}

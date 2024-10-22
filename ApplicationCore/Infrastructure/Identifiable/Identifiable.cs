using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace AGL.Api.ApplicationCore.Infrastructure
{
    public class Identifiable : IIdentifiable
    {
        protected IHttpContextAccessor _httpContextAccessor;
        public Identifiable(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }
        public ClaimsPrincipal GetClaimsPrincipal()
        {
            return _httpContextAccessor.HttpContext.User;
        }

        public string GetHttpMethod()
        {
            return _httpContextAccessor.HttpContext.Request.Method;
        }

        public string GetIdentityId()
        {
            return _httpContextAccessor.HttpContext.User.FindFirst("sub").Value;
        }

        public string GetRequestId()
        {
            return _httpContextAccessor.HttpContext.Request.Headers["x-requestid"];
        }

        public string GetRequestUrl()
        {
            return $"{_httpContextAccessor.HttpContext.Request.Host}{_httpContextAccessor.HttpContext.Request.Path}";
        }

        public string GetUserName()
        {
            return _httpContextAccessor.HttpContext.User.Identity.Name;
        }
    }
}

using System.Security.Claims;

namespace AGL.Api.ApplicationCore.Infrastructure
{
    public interface IIdentifiable
    {
        string GetIdentityId();
        string GetUserName();
        string GetRequestId();
        string GetRequestUrl();
        string GetHttpMethod();
        ClaimsPrincipal GetClaimsPrincipal();
    }
}

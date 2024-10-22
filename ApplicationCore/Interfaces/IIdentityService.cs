using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Models.Queries;
using Microsoft.AspNetCore.Http;

namespace AGL.Api.ApplicationCore.Interfaces
{
    public interface IIdentityService : IIdentifiable
    {
        string ClientId { get; }
        int UserId { get; }
        string Language { get; }
        string Currency { get; }
        ClientQuery ClientQuery { get; }

        public void UserCheck();

        string Referer { get; }
        string IpAddress { get; }
    }

    public class IdentityService : Identifiable, IIdentityService
    {
        public IdentityService(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor) { }

        public int UserId
        {
            get
            {
                int.TryParse(_httpContextAccessor.HttpContext.Request.Headers["user-id"], out int i);
                return i;
            }
        }

        public string Language
        {
            get
            {
                return _httpContextAccessor.HttpContext.Request.Headers.ContainsKey("language") ? _httpContextAccessor.HttpContext.Request.Headers["language"] : "KO";
            }
        }

        public string Currency
        {
            get
            {
                return _httpContextAccessor.HttpContext.Request.Headers.ContainsKey("currency") ? _httpContextAccessor.HttpContext.Request.Headers["currency"] : "KRW";

            }
        }

        public string ClientId
        {
            get
            {
                return _httpContextAccessor.HttpContext.Request.Headers.ContainsKey("client-id") ? _httpContextAccessor.HttpContext.Request.Headers["client-id"] : "";
            }
        }

        public ClientQuery ClientQuery
        {
            get
            {
                return new ClientQuery
                {
                    ClientId = this.ClientId,
                    UserId = this.UserId,
                    Language = this.Language,
                    Currency = this.Currency
                };
            }
        }

        public void UserCheck()
        {
            if (UserId == 0)
            {
                throw new DomainException(ApplicationCore.Models.Enum.ResultCode.비로그인, "Not Allowed User");
            }

        }


        // Referer 헤더를 가져옴
        public string Referer
        {
            get
            {
                var headers = _httpContextAccessor.HttpContext?.Request.Headers;
                return headers != null && headers.ContainsKey("Referer") ? headers["Referer"].ToString() : string.Empty;
            }
        }

        // IP 주소를 가져옴
        public string IpAddress
        {
            get
            {
                return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            }
        }
    }
}

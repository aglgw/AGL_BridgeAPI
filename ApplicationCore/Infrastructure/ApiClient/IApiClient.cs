using System.Net.Http;
using System.Threading.Tasks;

namespace AGL.Api.ApplicationCore.Infrastructure.ApiClient
{
    public interface IApiClient
    {
        HttpClient HttpClient { get; }
        /// <summary>
        /// 엑세스 토큰 요청
        /// </summary>
        /// <param name="tokenRequest"></param>
        /// <returns></returns>
        /// <remarks>
        /// var tokenRequest = new ClientCredentialsTokenRequest
        /// {
        ///     Address = "http://httpclient-idsrv/connect/token",
        ///     ClientId = "client-app",
        ///     ClientSecret = "secret",
        ///     Scope = "read:entity"
        /// })
        /// </remarks>
        
    }
}

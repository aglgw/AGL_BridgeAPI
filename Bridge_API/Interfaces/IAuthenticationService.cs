using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.Bridge_API.Models.OAPI;

namespace AGL.Api.Bridge_API.Interfaces
{
    public interface IAuthenticationService
    {
        /// <summary>
        /// 인증 등록
        /// </summary>
        /// <returns></returns>
        Task<IDataResult> PostAuthentication(AuthenticationRequest request, string token);

        /// <summary>
        /// 인증 조회
        /// </summary>
        /// <returns></returns>
        Task<IDataResult> GetAuthentication(AuthenticationRequest request, string token);

    }
}

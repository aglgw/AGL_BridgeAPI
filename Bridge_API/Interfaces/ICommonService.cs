using AGL.Api.Bridge_API.Models.OAPI;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using Microsoft.AspNetCore.Mvc;
using static AGL.Api.Bridge_API.Models.OAPI.OAPI;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIResponse;

namespace AGL.Api.Bridge_API.Interfaces
{
    /// <summary>
    /// OPEN API
    /// </summary>
    public interface ICommonService
    {
        /// <summary>
        /// 공통 함수_리턴 result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="isSuccess"></param>
        /// <param name="resultCode"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<dynamic> CreateResponse<T>(bool isSuccess, ResultCode resultCode, string message, T? data);
    }
}

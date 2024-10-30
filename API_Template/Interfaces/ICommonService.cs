using AGL.Api.API_Template.Models.OAPI;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using Microsoft.AspNetCore.Mvc;
using static AGL.Api.API_Template.Models.OAPI.OAPI;
using static AGL.Api.API_Template.Models.OAPI.OAPIResponse;

namespace AGL.Api.API_Template.Interfaces
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
        Task<OAPICommonResponse<T>> CreateResponse<T>(bool isSuccess, ResultCode resultCode, string message, List<T>? data = null);
    }
}

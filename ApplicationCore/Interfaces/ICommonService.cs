using AGL.Api.ApplicationCore.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGL.Api.ApplicationCore.Interfaces
{
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

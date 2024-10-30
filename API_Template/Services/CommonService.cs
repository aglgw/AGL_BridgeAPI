using AGL.Api.API_Template.Interfaces;
using AGL.Api.API_Template.Models.OAPI;
using AGL.Api.API_Template.Utils;
using AGL.Api.ApplicationCore.Extensions;
using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using AGL.Api.Domain.Entities.OAPI;
using AGL.Api.Infrastructure.Data;
using Azure;
using Azure.Core;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using static AGL.Api.API_Template.Models.OAPI.OAPI;
using static AGL.Api.API_Template.Models.OAPI.OAPIResponse;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AGL.Api.API_Template.Services
{
    public class CommonService : BaseService
    {
        private readonly OAPI_DbContext _context;
        private IConfiguration _configuration { get; }

        public CommonService(OAPI_DbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        /// <summary>
        /// 공통 함수_list 형 result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="isSuccess"></param>
        /// <param name="resultCode"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<OAPICommonListResponse<T>> CreateListResponse<T>(bool isSuccess, ResultCode resultCode, string message, List<T>? data = null)
        {
            var description = ExtensionMethods.GetDescription(resultCode);
            var response = new OAPICommonListResponse<T>
            {
                IsSuccess = isSuccess,
                RstCd = description,
                RstMsg = $"{description} (StatusCode: {(int)resultCode}) {message}",
                StatusCode = (int)resultCode,
                
            };

            if (data != null)
                response.data = data;


            await Task.CompletedTask;

            return response;
        }

        /// <summary>
        /// 공통 함수_object 형 result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="isSuccess"></param>
        /// <param name="resultCode"></param>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<OAPICommonResponse<T>> CreateListResponse<T>(bool isSuccess, ResultCode resultCode, string message, T? data)
        {
            var description = ExtensionMethods.GetDescription(resultCode);
            var response = new OAPICommonResponse<T>
            {
                IsSuccess = isSuccess,
                RstCd = description,
                RstMsg = $"{description} (StatusCode: {(int)resultCode}) {message}",
                StatusCode = (int)resultCode,

            };

            if (data != null)
                response.data = data;


            await Task.CompletedTask;

            return response;
        }

    }
}

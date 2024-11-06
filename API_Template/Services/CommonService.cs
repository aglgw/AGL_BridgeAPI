using AGL.Api.Bridge_API.Interfaces;
using AGL.Api.Bridge_API.Models.OAPI;
using AGL.Api.Bridge_API.Utils;
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
using static AGL.Api.Bridge_API.Models.OAPI.OAPI;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIResponse;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AGL.Api.Bridge_API.Services
{
    public class CommonService : BaseService, ICommonService
    {
        private readonly OAPI_DbContext _context;
        private IConfiguration _configuration { get; }

        public CommonService(OAPI_DbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<dynamic> CreateResponse<T>(bool isSuccess, ResultCode resultCode, string message, T? data)
        {
            var description = ExtensionMethods.GetDescription(resultCode);

            // 요청 데이터 타입에 따라 반환 타입 결정
            if (data == null)
            {
                // 데이터가 없는 경우 기본 응답
                return new OAPIResponseBase
                {
                    IsSuccess = isSuccess,
                    RstCd = description,
                    RstMsg = $"{description} (StatusCode: {(int)resultCode}) {message}",
                    StatusCode = (int)resultCode
                };
            }
            else if (data is List<string> stringList)
            {
                return new OAPICommonListResponse<string>
                {
                    IsSuccess = isSuccess,
                    RstCd = description,
                    RstMsg = $"{description} (StatusCode: {(int)resultCode}) {message}",
                    StatusCode = (int)resultCode,
                    data = stringList
                };
            }
            else if (data is Dictionary<string, List<TeeTimeInfo>> teeTimeDict)
            {
                return new OAPITeeTimeGetResponse
                {
                    IsSuccess = isSuccess,
                    RstCd = description,
                    RstMsg = $"{description} (StatusCode: {(int)resultCode}) {message}",
                    StatusCode = (int)resultCode,
                    data = teeTimeDict
                };
            }
            else if (data is string singleString)
            {
                return new OAPICommonResponse<string>
                {
                    IsSuccess = isSuccess,
                    RstCd = description,
                    RstMsg = $"{description} (StatusCode: {(int)resultCode}) {message}",
                    StatusCode = (int)resultCode,
                    data = singleString
                };
            }
            else
            {
                // 기타 타입에 대한 공통 응답
                return new OAPICommonResponse<T>
                {
                    IsSuccess = isSuccess,
                    RstCd = description,
                    RstMsg = $"{description} (StatusCode: {(int)resultCode}) {message}",
                    StatusCode = (int)resultCode,
                    data = data
                };
            }

        }
    }
}

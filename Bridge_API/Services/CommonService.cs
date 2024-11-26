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
using static AGL.Api.Bridge_API.Models.OAPI.Inbound;

namespace AGL.Api.Bridge_API.Services
{
    public class CommonService : BaseService, ICommonService
    {
        private IConfiguration _configuration { get; }

        public CommonService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<dynamic> CreateResponse<T>(bool isSuccess, ResultCode resultCode, string message, T? data)
        {
            var description = ExtensionMethods.GetDescription(resultCode);

            if (data == null) // 데이터가 없는 경우 기본 응답
            {
                return new OAPIResponseBase
                {
                    isSuccess = isSuccess,
                    rstCd = description,
                    rstMsg = $"{description} (StatusCode: {(int)resultCode}) {message}",
                    statusCode = (int)resultCode
                };
            }
            else if (typeof(T) == typeof(TeeTimeData)) // 티타임 조회용
            {
                return new TeeTimeResponse
                {
                    isSuccess = isSuccess,
                    rstCd = description,
                    rstMsg = $"{description} (StatusCode: {(int)resultCode}) {message}",
                    statusCode = (int)resultCode,
                    data = data as TeeTimeData
                };
            }
            else
            {
                return new OAPIDataResponse<T>
                {
                    isSuccess = isSuccess,
                    rstCd = description,
                    rstMsg = $"{description} (StatusCode: {(int)resultCode}) {message}",
                    statusCode = (int)resultCode,
                    data = data

                };
            };

        }
    }
}

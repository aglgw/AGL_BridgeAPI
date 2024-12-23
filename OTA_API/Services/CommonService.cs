using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using AGL.Api.OTA_API.Models.OAPI;
using static AGL.Api.OTA_API.Models.OAPI.OTAResponse;

namespace AGL.Api.OTA_API.Services
{
    public class CommonService : BaseService, ICommonService
    {
        public async Task<dynamic> CreateResponse<T>(bool isSuccess, ResultCode resultCode, string message, T? data)
        {
            return new OTADataResponseBase<T>
            {
                success = isSuccess,
                message = message,
                data = data
            };
        }
    }
}

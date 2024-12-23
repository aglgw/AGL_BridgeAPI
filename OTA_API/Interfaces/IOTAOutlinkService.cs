using AGL.Api.ApplicationCore.Interfaces;
using static AGL.Api.OTA_API.Models.OAPI.OTA;
using static AGL.Api.OTA_API.Models.OAPI.OTARequest;
using static AGL.Api.OTA_API.Models.OAPI.OTAResponse;

namespace AGL.Api.OTA_API.Interfaces
{
    public interface IOTAOutlinkService
    {
        /// <summary>
        /// 전체 아웃링크 데이터 조회
        /// </summary>3
        /// <param name="request"></param>
        /// <returns></returns>
        Task<OTADataResponseBase<List<Outlink>>> GetOutlinkData(OTARequestBase request);

        /// <summary>
        /// 업데이트된 아웃링크 데이터만 조회
        /// </summary>3
        /// <param name="request"></param>
        /// <returns></returns>
        Task<OTADataResponseBase<List<Outlink>>> GetUpdatedOutlink(OTARequestBase request);
    }
}

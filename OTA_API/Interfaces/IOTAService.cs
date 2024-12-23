using AGL.Api.ApplicationCore.Interfaces;
using static AGL.Api.OTA_API.Models.OAPI.OTARequest;

namespace AGL.Api.OTA_API.Interfaces
{
    public interface IOTAService
    {
        /// <summary>
        /// 특정 산하 업체의 트래킹 생성
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IOTAResult> CreateTracking(OTARequestBase request);

        /// <summary>
        /// 트래킹 정보 수정
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IOTAResult> UpdateTracking(OTARequestBase request);

        /// <summary>
        /// 특정 트래킹 정보 삭제
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IOTAResult> DeleteTracking(OTARequestBase request);


        /// <summary>
        /// OTA 산하 업체의 모든 트래킹 정보 조회
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IOTAResult> GetTrackingList(OTARequestBase request);
    }
}

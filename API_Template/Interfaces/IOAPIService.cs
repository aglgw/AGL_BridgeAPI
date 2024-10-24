using AGL.Api.API_Template.Models.OAPI;
using AGL.Api.ApplicationCore.Interfaces;
using static AGL.Api.API_Template.Models.OAPI.OAPI;
using static AGL.Api.API_Template.Models.OAPI.OAPIResponse;

namespace AGL.Api.API_Template.Interfaces
{
    /// <summary>
    /// OPEN API
    /// </summary>
    public interface IOAPIService
    {
        /// <summary>
        /// 티타임 등록
        /// </summary>
        /// <returns></returns>
        Task<IDataResult> PostTeeTime(OAPITeeTimePostRequest request);
        /// <summary>
        /// 티타임 수정
        /// </summary>
        /// <returns></returns>
        Task<IDataResult> PutTeeTime(OAPITeeTimePutRequest request);
        /// <summary>
        /// 티타임 조회
        /// </summary>
        /// <returns></returns>
        Task<OAPITeeTimeGetResponse> GetTeeTime(OAPITeeTimeGetRequest request);
        /// <summary>
        /// 티타임 상태수정
        /// </summary>
        /// <returns></returns>
        Task<IDataResult> PostTeeTimeAvailability(OAPITeeTimetAvailabilityRequest request);
        /// <summary>
        /// 예약확정
        /// </summary>
        /// <returns></returns>
        Task<IDataResult> PostReservatioConfirm(OAPIReservationRequest request);
    }
}

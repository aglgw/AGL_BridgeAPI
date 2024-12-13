using AGL.Api.ApplicationCore.Interfaces;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIRequest;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIResponse;

namespace AGL.Api.Bridge_API.Interfaces
{
    /// <summary>
    /// OPEN API
    /// </summary>
    public interface IBookingService
    {

        /*
                /// <summary>
                /// 예약조회
                /// </summary>
                /// <param name="Req"></param>
                /// <returns></returns>
                Task<IDataResult> GetBookingInquiry(ReqBookingInquiry Req);

                /// <summary>
                /// 확정된 예약 조회
                /// </summary>
                /// <param name="reservationId"></param>
                /// <returns></returns>
                Task<IDataResult> GetConfirmBookingInquiry(string reservationId, string daemonId);
        

                Task<IDataResult> Test();
        */

        /// <summary>
        /// 예약요청 생성 테스트
        /// </summary>
        /// <param name="supplierCode"></param>
        /// <returns></returns>
        Task<OAPIDataResponse<ReservationReponse>> PostTestBookingRequest(string supplierCode);

        /// <summary>
        /// 예약요청 조회
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<OAPIDataResponse<List<ReservationReponse>>> GetBookingRequest(ReservationListRequest request, string supplierCode);

        /// <summary>
        /// 예약확정
        /// </summary>
        /// <param name="request"></param>  
        /// <returns></returns>
        Task<IDataResult> PostBookingConfirm(ReservationRequest request, string supplierCode);

        /// <summary>
        /// 예약취소요청 조회
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<OAPIDataResponse<List<ReservationReponse>>> GetBookingCancellations(ReservationListRequest request, string supplierCode);

        /// <summary>
        /// 예약취소
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IDataResult> PostBookingCancel(cancelRequest request, string supplierCode);


    }
}

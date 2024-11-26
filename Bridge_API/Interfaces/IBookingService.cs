using AGL.Api.ApplicationCore.Interfaces;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIRequest;

namespace AGL.Api.Bridge_API.Interfaces
{
    /// <summary>
    /// OPEN API
    /// </summary>
    public interface IBookingService
    {
        /// <summary>
        /// 예약요청
        /// </summary>
        /// <param name="Req"></param>
        /// <returns></returns>
        Task<IDataResult> POSTBookingRequest(ReqBookingRequest Req);

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

        /// <summary>
        /// 예약 취소
        /// </summary>
        /// <param name="Req"></param>
        /// <returns></returns>
        Task<IDataResult> PostBookingCancel(ReservationInboundRequest Req);

        /// <summary>
        /// 예약확정
        /// </summary>
        /// <returns></returns>
        Task<IDataResult> PostBookingConfirm(ReservationRequest request, string supplierCode);



        Task<IDataResult> Test();
    }
}

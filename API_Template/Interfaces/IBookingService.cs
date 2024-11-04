using AGL.Api.API_Template.Models.OAPI;
using AGL.Api.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Mvc;
using static AGL.Api.API_Template.Models.OAPI.OAPI;
using static AGL.Api.API_Template.Models.OAPI.OAPIResponse;

namespace AGL.Api.API_Template.Interfaces
{
    /// <summary>
    /// OPEN API
    /// </summary>
    public interface IBookingService
    {
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
        Task<IDataResult> GetConfirmBookingInquiry(string reservationId);

        /// <summary>
        /// 예약 취소
        /// </summary>
        /// <param name="Req"></param>
        /// <returns></returns>
        Task<IDataResult> GetBookingCancel(OAPIReservationRequest Req);
        /// <summary>
        /// 예약확정
        /// </summary>
        /// <returns></returns>
        Task<IDataResult> PostBookingConfirm(OAPIReservationRequest request, string supplierCode);



        Task<IDataResult> Test();
    }
}

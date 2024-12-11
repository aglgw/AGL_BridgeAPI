using AGL.Api.ApplicationCore.Interfaces;
using static AGL.Api.Bridge_API.Models.OAPI.Inbound;
using static AGL.Api.Bridge_API.Models.OAPI.OAPI;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIRequest;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIResponse;

namespace AGL.Api.Bridge_API.Interfaces
{
    public interface IInboundService
    {
        /// <summary>
        /// 서비스 예약요청
        /// </summary>
        /// <param name="Req"></param>
        /// <returns></returns>
        Task<IDataResult> POSTInboundBookingRequest(ReqBookingRequest Req);

        /// <summary>
        /// 서비스 예약취소
        /// </summary>
        /// <param name="Req"></param>
        /// <returns></returns>
        Task<IDataResult> PostInboundBookingCancel(ReservationInboundRequest Req);

        /// <summary>
        /// 내부연동 골프장 조회
        /// </summary>
        /// <param name="inboundCode"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<OAPIDataResponse<List<GolfClubInfoWithInboundCode>>> GetInboundGolfClub(string? inboundCode, string token);

        /// <summary>
        /// 내부연동 티타임 기간조회
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IDataResult> GetInboundTeeTime(InboundTeeTimeRequest request);


    }
}

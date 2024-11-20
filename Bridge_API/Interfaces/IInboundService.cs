using AGL.Api.ApplicationCore.Interfaces;
using static AGL.Api.Bridge_API.Models.OAPI.Inbound;
using static AGL.Api.Bridge_API.Models.OAPI.OAPI;

namespace AGL.Api.Bridge_API.Interfaces
{
    public interface IInboundService
    {

        /// <summary>
        /// 내부연동 골프장 조회
        /// </summary>
        /// <param name="golfclubCode"></param>
        /// <returns></returns>
        Task<IDataResult> GetInboundGolfClub(string? golfclubCode);

        /// <summary>
        /// 내부연동 티타임 기간조회
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IDataResult> GetInboundTeeTime(InboundTeeTimeRequest request);


    }
}

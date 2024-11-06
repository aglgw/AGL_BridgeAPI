using AGL.Api.ApplicationCore.Interfaces;
using static AGL.Api.API_Template.Models.OAPI.Inbound;
using static AGL.Api.API_Template.Models.OAPI.OAPI;

namespace AGL.Api.API_Template.Interfaces
{
    public interface IInboundService
    {
        /// <summary>
        /// 내부연동 티타임 기간조회
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IDataResult> GetInboundTeeTime(InboundTeeTimeRequest request);

    }
}

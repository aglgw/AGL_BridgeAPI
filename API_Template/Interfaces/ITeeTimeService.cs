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
    public interface ITeeTimeService
    {
        /// <summary>
        /// 티타임 등록
        /// </summary>
        /// <returns></returns>
        Task<IDataResult> PostTeeTime(TeeTimeRequest request, string supplierCode);
        /// <summary>
        /// 티타임 수정
        /// </summary>
        /// <returns></returns>
        Task<IDataResult> PutTeeTime(TeeTimeRequest request, string supplierCode);
        /// <summary>
        /// 티타임 조회
        /// </summary>
        /// <returns></returns>
        Task<IDataResult> GetTeeTime(TeeTimeGetRequest request, string supplierCode);
        /// <summary>
        /// 티타임 상태수정
        /// </summary>
        /// <returns></returns>
        Task<IDataResult> PutTeeTimeAvailability(TeeTimetAvailabilityRequest request, string supplierCode);
    }
}

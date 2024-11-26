using AGL.Api.ApplicationCore.Interfaces;
using static AGL.Api.Bridge_API.Models.OAPI.OAPI;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIResponse;


namespace AGL.Api.Bridge_API.Interfaces
{
    public interface IGolfClubService
    {
        /// <summary>
        /// 골프장 등록
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IDataResult> PostGolfClub(GolfClubInfo request, string supplierCode);

        /// <summary>
        /// 골프장 수정
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<IDataResult> PutGolfClub(GolfClubInfo request, string supplierCode);

        /// <summary>
        /// 골프장 조회
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<OAPIDataResponse<List<GolfClubInfo>>> GetGolfClub(string supplierCode, string? GolfclubCode);
    }
}

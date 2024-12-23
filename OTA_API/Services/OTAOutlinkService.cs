using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using AGL.Api.OTA_API.Interfaces;
using AGL.API.Infrastructure.Data;
using static AGL.Api.OTA_API.Models.OAPI.OTA;
using static AGL.Api.OTA_API.Models.OAPI.OTARequest;
using static AGL.Api.OTA_API.Models.OAPI.OTAResponse;

namespace AGL.Api.OTA_API.Services
{
    public class OTAOutlinkService : BaseService, IOTAOutlinkService
    {
        private readonly OTADbContext _context;
        private readonly ICommonService _commonService;

        public OTAOutlinkService(OTADbContext context, ICommonService commonService)
        {
            _context = context;
            _commonService = commonService;
        }

        public async Task<OTADataResponseBase<List<Outlink>>> GetOutlinkData(OTARequestBase request)
        {


            try
            {
                //Utils.UtilLogs.LogRegHour(request.CompanyCode, request.CompanyCode, "GetOutlinkData", "아웃링크 데이터 처리 완료");

                return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "ProcessTracking successfully", null);
            }
            catch (Exception ex)
            {
                Utils.UtilLogs.LogRegHour(request.CompanyCode, request.CompanyCode, "GetOutlinkData", $"트래킹 저장 실패 {ex.Message}", true);
                return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
            }

        }


        public async Task<OTADataResponseBase<List<Outlink>>> GetUpdatedOutlink(OTARequestBase request)
        {


            try
            {
                Utils.UtilLogs.LogRegHour(request.CompanyCode, request.CompanyCode, "GetUpdatedOutlink", "트래킹 처리 완료");

                return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "ProcessTracking successfully", null);
            }
            catch (Exception ex)
            {
                Utils.UtilLogs.LogRegHour(request.CompanyCode, request.CompanyCode, "GetOutlinkData", $"트래킹 저장 실패 {ex.Message}", true);
                return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
            }

        }

    }
}

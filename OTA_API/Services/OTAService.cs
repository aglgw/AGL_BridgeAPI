using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using AGL.Api.Infrastructure.Data;
using AGL.Api.OTA_API.Interfaces;
using AGL.API.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static AGL.Api.OTA_API.Models.OAPI.OTARequest;
using static AGL.Api.OTA_API.Utils.Util;

namespace AGL.Api.OTA_API.Services
{
    public class OTAService : BaseService, IOTAService
    {
        private readonly OTADbContext _context;
        private readonly ICommonService _commonService;

        public OTAService(OTADbContext context, ICommonService commonService)
        {
            _context = context;
            _commonService = commonService;
        }

        public async Task<IOTAResult> CreateTracking(OTARequestBase request)
        {
            return await ProcessTracking(request, "POST");
        }

        public async Task<IOTAResult> UpdateTracking(OTARequestBase request)
        {
            return await ProcessTracking(request, "PUT");
        }

        private async Task<IOTAResult> ProcessTracking(OTARequestBase request, string mode)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {




                        Utils.UtilLogs.LogRegHour(request.CompanyCode, request.CompanyCode, "ProcessTracking", "트래킹 처리 완료");

                        return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "ProcessTracking successfully", null);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Utils.UtilLogs.LogRegHour(request.CompanyCode, request.CompanyCode, "ProcessTracking", $"트래킹 저장 실패 {ex.Message}", true);
                        return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
                    }
                }
            });
        }

        public async Task<IOTAResult> DeleteTracking(OTARequestBase request)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {



                        Utils.UtilLogs.LogRegHour(request.CompanyCode, request.CompanyCode, "ProcessTracking", "트래킹 처리 완료");

                        return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "ProcessTracking successfully", null);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Utils.UtilLogs.LogRegHour(request.CompanyCode, request.CompanyCode, "ProcessTracking", $"트래킹 저장 실패 {ex.Message}", true);
                        return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
                    }
                }
            });
        }


        public async Task<IOTAResult> GetTrackingList(OTARequestBase request)
        {

            try
            {



                Utils.UtilLogs.LogRegHour(request.CompanyCode, request.CompanyCode, "ProcessTracking", "트래킹 처리 완료");

                return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "ProcessTracking successfully", null);
            }
            catch (Exception ex)
            {
                Utils.UtilLogs.LogRegHour(request.CompanyCode, request.CompanyCode, "ProcessTracking", $"트래킹 저장 실패 {ex.Message}", true);
                return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
            }

        }
    }
}

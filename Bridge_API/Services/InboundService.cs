using AGL.Api.Bridge_API.Interfaces;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using AGL.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static AGL.Api.Bridge_API.Models.OAPI.Inbound;
using static AGL.Api.Bridge_API.Models.OAPI.OAPI;

namespace AGL.Api.Bridge_API.Services
{
    public class InboundService : IInboundService
    {
        public const string InboundToken = "9EC6wbUI6y"; // 인바운드용 header token
        private readonly OAPI_DbContext _context;
        private IConfiguration _configuration { get; }
        private readonly ICommonService _commonService;

        public InboundService(OAPI_DbContext context, IConfiguration configuration, ICommonService commonService)
        {
            _context = context;
            _configuration = configuration;
            _commonService = commonService;
        }

        public async Task<IDataResult> GetInboundTeeTime(InboundTeeTimeRequest request)
        {
            var startDate = request.startDate;
            var endDate = request.endDate;
            var golfClubCode = request.golfclubCode;
            var token = request.token;

            if (string.IsNullOrEmpty(token) || token != InboundToken)
            {
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "token is invalid", null);
            }

            try
            {
                // 공급자 코드에 따른 가격 정책 가져오기
                var pricePolicies = await _context.TeetimePricePolicies
                    .Where(pp => pp.TeeTimeMappings.Any(tm => tm.TeeTime.GolfClub.GolfClubCode == golfClubCode))
                    .ToDictionaryAsync(pp => pp.PricePolicyId);

                // 주어진 골프장 코드와 날짜 범위에 해당하는 티타임 매핑 가져오기
                var teeTimeMappings = await _context.TeeTimeMappings
                    .Include(tm => tm.TeeTime).ThenInclude(t => t.GolfClub)
                    .Include(tm => tm.TeeTime).ThenInclude(t => t.GolfClubCourse)
                    .Include(tm => tm.TimeSlot)
                    .Include(tm => tm.DateSlot)
                    .Where(tm => tm.TeeTime.GolfClub.GolfClubCode == request.golfclubCode && string.Compare(tm.DateSlot.PlayDate, startDate) >= 0 && string.Compare(tm.DateSlot.PlayDate, endDate) <= 0)
                    .OrderBy(tm => tm.DateSlot.PlayDate)
                    .ThenBy(tm => tm.TimeSlot.StartTime)
                    .ToListAsync();

                // 응답 데이터 준비
                var responseData = new Dictionary<string, List<InboundTeeTimeResponse>>
                {
                    ["resultData"] = new List<InboundTeeTimeResponse>()
                };

                foreach ( var tm in teeTimeMappings )
                {
                    var pricePolicy = pricePolicies.ContainsKey(tm.PricePolicyId) ? pricePolicies[tm.PricePolicyId] : null;

                    var InboundData = new InboundTeeTimeResponse
                    {
                        PlayDate = tm.PlayDate,
                        CourseCode = tm.TeeTime.GolfClubCourse.CourseCode,
                        PlayTime = tm.StartTime,
                        MinMember = tm.TeeTime.MinMembers,
                        sumAmt_3 = pricePolicy?.UnitPrice_3,
                        sumAmt_4 = pricePolicy?.UnitPrice_4,
                    };
                    responseData["resultData"].Add(InboundData);
                }

                return await _commonService.CreateResponse(true, ResultCode.SUCCESS, "TeeTime List successfully", responseData);
            }
            catch (Exception ex)
            {
                return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
            }
        }


    }
}

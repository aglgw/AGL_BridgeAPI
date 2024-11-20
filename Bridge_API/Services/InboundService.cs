using AGL.Api.Bridge_API.Interfaces;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using AGL.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static AGL.Api.Bridge_API.Models.OAPI.Inbound;
using static AGL.Api.Bridge_API.Models.OAPI.OAPI;
using AGL.Api.Domain.Entities.OAPI;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIResponse;

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

        public async Task<OAPIDataResponse<List<InboundTeeTimeResponse>>> GetInboundTeeTime(InboundTeeTimeRequest request)
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

                // 날짜 범위에 해당하는 DateSlot의 ID 목록 가져오기
                var dateSlotIds = await _context.DateSlots
                    .Where(ds => string.Compare(ds.PlayDate, startDate) >= 0 && string.Compare(ds.PlayDate, endDate) <= 0)
                    .Select(ds => ds.DateSlotId)
                    .ToListAsync();

                // 골프장 코드와 공급자 코드에 해당하는 골프장 가져오기
                var golfClub = await _context.GolfClubs
                    .Where(gc => gc.GolfClubCode == golfClubCode)
                    .Select(gc => new { gc.GolfClubId, gc.SupplierId })
                    .FirstOrDefaultAsync();

                if (golfClub == null)
                {
                    return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Invalid GolfClubCode", null);
                }

                // 주어진 골프장 코드와 날짜 범위에 해당하는 티타임 매핑 가져오기
                var teeTimeMappings = await _context.TeeTimeMappings
                    .Where(tm => tm.TeeTime.GolfClubId == golfClub.GolfClubId && dateSlotIds.Contains(tm.DateSlotId))
                    .OrderBy(tm => tm.DateSlot.PlayDate)
                    .ThenBy(tm => tm.TimeSlot.StartTime)
                    .Select(tm => new
                    {
                        tm.PricePolicyId,
                        PlayDate = tm.DateSlot.PlayDate,
                        CourseCode = tm.TeeTime.GolfClubCourse.CourseCode,
                        PlayTime = tm.TimeSlot.StartTime,
                        MinMember = tm.TeeTime.MinMembers,
                    })
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
                        CourseCode = tm.CourseCode,
                        PlayTime = tm.PlayTime,
                        MinMember = tm.MinMember,
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

        public async Task<OAPIDataResponse<List<GolfClubInfo>>> GetInboundGolfClub(string golfClubCode)
        {
            try
            {
                // 모든 관련 데이터를 미리 조회 골프장,이미지,환불정책,코스,홀
                var existingGolfclubQuery = _context.GolfClubs
                    .Include(g => g.GolfClubImages)
                    .Include(g => g.RefundPolicies)
                    .Include(g => g.Courses)
                    .Include(g => g.Holes)
                    .Where(g => true); // 검색 조건 없는 where 추가

                // 골프장 코드가 있을 경우 해당 코드들에 대한 조건 추가
                if (golfClubCode != null)
                {
                    existingGolfclubQuery = existingGolfclubQuery.Where(g => g.GolfClubCode == golfClubCode);
                }
                // 조건에 맞는 TeeTimePriceMappings 목록을 조회
                var existingGolfclubs = await existingGolfclubQuery.ToListAsync();

                // 유효성 검사 - 조회된 골프장이 없을 경우
                if (existingGolfclubs == null || !existingGolfclubs.Any())
                {
                    Utils.UtilLogs.LogRegHour("inbound", "inbound", "GolfClub", "골프장 검색 코드 없음");
                    return await _commonService.CreateResponse<object>(false, ResultCode.NOT_FOUND, "GolfClubs Not Found", null);
                }

                var golfClubDtos = existingGolfclubs.Select(golfClub => new GolfClubInfo
                {
                    golfClubCode = golfClub.GolfClubCode,
                    golfClubName = golfClub.GolfClubName,
                    countryCode = golfClub.CountryCode,
                    currency = golfClub.Currency,
                    description = golfClub.Description,
                    address = golfClub.Address,
                    latitude = golfClub.Latitude?.ToString(),
                    longitude = golfClub.Longitude?.ToString(),
                    phone = golfClub.Phone,
                    fax = golfClub.Fax,
                    email = golfClub.Email,
                    homepage = golfClub.Homepage,
                    totalHoleCount = golfClub.TotalHoleCount,
                    totalCourseCount = golfClub.Courses.Count,
                    isGuestInfoRequired = golfClub.isGuestInfoRequired,
                    image = golfClub.GolfClubImages.Select(img => new Images
                    {
                        id = img.Idx,
                        url = img.Url,
                        title = img.Title,
                        description = img.ImageDescription
                    }).ToList(),
                    refundPolicy = golfClub.RefundPolicies.Select(rp => new RefundPolicy
                    {
                        refundDate = rp.RefundDate,
                        refundFee = rp.RefundFee,
                        refundUnit = rp.RefundUnit,
                    }).ToList(),
                    course = golfClub.Courses.Select(c => new Course
                    {
                        courseCode = c.CourseCode,
                        courseName = c.CourseName,
                        courseHoleCount = c.CourseHoleCount,
                        startHole = c.StartHole,
                    }).ToList(),
                    holeInfo = golfClub.Holes.Select(h => new HoleInfo
                    {
                        holeNumber = h.HoleNumber,
                        holeName = h.HoleName,
                        par = h.Par,
                        distanceUnit = h.DistanceUnit,
                        distance = h.Distance
                    }).ToList()
                }).ToList();

                Utils.UtilLogs.LogRegHour("inbound", "inbound", "GolfClub", $"골프장 검색 성공");
                return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "GolfClub List successfully", golfClubDtos);
            }
            catch (Exception ex)
            {
                Utils.UtilLogs.LogRegHour("inbound", "inbound", "GolfClub", $"골프장 검색 실패 {ex.Message}", true);
                return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
            }
        }

    }
}

﻿using AGL.Api.Bridge_API.Interfaces;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using AGL.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static AGL.Api.Bridge_API.Models.OAPI.Inbound;
using static AGL.Api.Bridge_API.Models.OAPI.OAPI;
using AGL.Api.Domain.Entities.OAPI;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIResponse;
using RTools_NTS.Util;
using System.Diagnostics;

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
            var inboundCode = request.inboundCode;
            var token = request.token;

            if (string.IsNullOrEmpty(token) || token != InboundToken)
            {
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "token is invalid", null);
            }

            try
            {
                // 골프장 코드와 공급자 코드에 해당하는 골프장 가져오기
                var golfClub = await _context.GolfClubs
                    .Where(gc => gc.InboundCode == inboundCode)
                    .Select(gc => new { gc.GolfClubId, gc.SupplierId })
                    .FirstOrDefaultAsync();

                if (golfClub == null)
                {
                    return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Invalid GolfClub", null);
                }

                // request.startDate와 request.endDate를 DateTime으로 변환
                if (!DateTime.TryParseExact(request.startDate, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var startDate) ||
                    !DateTime.TryParseExact(request.endDate, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var endDate))
                {
                    return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Invalid date format. Expected yyyyMMdd.", null);
                }

                var teeTimeMappingsWithPolicies = await _context.TeeTimeMappings
                    .Where(tm => tm.TeeTime.GolfClub.InboundCode == inboundCode &&
                                 tm.DateSlot.StartDate >= startDate &&
                                 tm.DateSlot.StartDate <= endDate)
                    .Select(tm => new
                    {
                        tm.DateSlot.PlayDate,
                        tm.TimeSlot.StartTime,
                        tm.TeeTime.MinMembers,
                        tm.TeeTime.GolfClubCourse.CourseCode,
                        PricePolicy = tm.TeetimePricePolicy == null ? null : new
                        {
                            tm.TeetimePricePolicy.UnitPrice_3,
                            tm.TeetimePricePolicy.UnitPrice_4
                        }
                    })
                    .OrderBy(x => x.PlayDate)
                    .ThenBy(x => x.StartTime)
                    .ToListAsync();

                // 응답 데이터 준비
                var responseData = teeTimeMappingsWithPolicies.Select(tm => new InboundTeeTimeResponse
                {
                    PlayDate = tm.PlayDate,
                    CourseCode = tm.CourseCode,
                    PlayTime = tm.StartTime,
                    MinMember = tm.MinMembers,
                    sumAmt_3 = tm.PricePolicy?.UnitPrice_3,
                    sumAmt_4 = tm.PricePolicy?.UnitPrice_4
                }).ToList();

                return await _commonService.CreateResponse(true, ResultCode.SUCCESS, "TeeTime List successfully", responseData);
            }
            catch (Exception ex)
            {
                Utils.UtilLogs.LogRegHour("inbound", "inbound", "GolfClub", $"티타임 검색 실패 {ex.Message}", true);
                return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
            }
        }

        public async Task<OAPIDataResponse<List<GolfClubInfoWithInboundCode>>> GetInboundGolfClub(string inboundCode, string token)
        {
            if (string.IsNullOrEmpty(token) || token != InboundToken)
            {
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "token is invalid", null);
            }

            try
            {
                // 모든 관련 데이터를 미리 조회 골프장,이미지,환불정책,코스,홀
                var golfClubDtos = await _context.GolfClubs
                     .Where(g => inboundCode == null || g.InboundCode == inboundCode)
                     .Select(g => new GolfClubInfoWithInboundCode
                     {
                         InboundCode = g.InboundCode,
                         golfClubName = g.GolfClubName,
                         countryCode = g.CountryCode,
                         currency = g.Currency,
                         description = g.Description,
                         address = g.Address,
                         latitude = g.Latitude,
                         longitude = g.Longitude,
                         phone = g.Phone,
                         fax = g.Fax,
                         email = g.Email,
                         homepage = g.Homepage,
                         totalHoleCount = g.TotalHoleCount,
                         totalCourseCount = g.Courses.Count,
                         isGuestInfoRequired = g.isGuestInfoRequired,
                         image = g.GolfClubImages.Select(img => new Images
                         {
                             id = img.Idx,
                             url = img.Url,
                             title = img.Title,
                             description = img.ImageDescription
                         }).ToList(),
                         refundPolicy = g.RefundPolicies.Select(rp => new RefundPolicy
                         {
                             refundDate = rp.RefundDate,
                             refundFee = rp.RefundFee,
                             refundUnit = rp.RefundUnit,
                         }).ToList(),
                         course = g.Courses.Select(c => new Course
                         {
                             courseCode = c.CourseCode,
                             courseName = c.CourseName,
                             courseHoleCount = c.CourseHoleCount,
                             startHole = c.StartHole,
                         }).ToList(),
                         holeInfo = g.Holes.Select(h => new HoleInfo
                         {
                             holeNumber = h.HoleNumber,
                             holeName = h.HoleName,
                             par = h.Par,
                             distanceUnit = h.DistanceUnit,
                             distance = h.Distance
                         }).ToList()
                     }).ToListAsync();

                // 유효성 검사 - 조회된 골프장이 없을 경우
                if ( !golfClubDtos.Any())
                {
                    Utils.UtilLogs.LogRegHour("inbound", "inbound", "GolfClub", "골프장 검색 코드 없음");
                    return await _commonService.CreateResponse<List<GolfClubInfoWithInboundCode>>(false, ResultCode.NOT_FOUND, "GolfClubs Not Found", null);
                }

                return await _commonService.CreateResponse(true, ResultCode.SUCCESS, "GolfClub List successfully", golfClubDtos);
            }
            catch (Exception ex)
            {
                Utils.UtilLogs.LogRegHour("inbound", "inbound", "GolfClub", $"골프장 검색 실패 {ex.Message}", true);
                return await _commonService.CreateResponse<List<GolfClubInfoWithInboundCode>>(false, ResultCode.SERVER_ERROR, ex.Message, null);
            }
        }

    }
}

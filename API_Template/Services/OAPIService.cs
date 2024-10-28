using AGL.Api.API_Template.Interfaces;
using AGL.Api.API_Template.Models.OAPI;
using AGL.Api.API_Template.Utils;
using AGL.Api.ApplicationCore.Extensions;
using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using AGL.Api.Domain.Entities.OAPI;
using AGL.Api.Infrastructure.Data;
using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static AGL.Api.API_Template.Models.OAPI.OAPIResponse;

namespace AGL.Api.API_Template.Services
{
    public class OAPIService : BaseService, IOAPIService
    {
        private readonly OAPI_DbContext _context;
        private IConfiguration _configuration { get; }

        public OAPIService(OAPI_DbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        public async Task<IDataResult> PostTeeTime(OAPITeeTimeRequest request, string supplierCode)
        {
            return await ProcessTeeTime(request, supplierCode, request.golfclubCode);
        }

        public async Task<IDataResult> UpdateTeeTime(OAPITeeTimeRequest request, string supplierCode)
        {
            return await ProcessTeeTime(request, supplierCode, request.golfclubCode);
        }

        public Task<IDataResult> GetTeeTime(OAPITeeTimeGetRequest request, string supplierCode)
        {
            throw new NotImplementedException();
        }

        public Task<IDataResult> PostReservatioConfirm(OAPIReservationRequest request)
        {
            throw new NotImplementedException();
        }


        public Task<IDataResult> PostTeeTimeAvailability(OAPITeeTimetAvailabilityRequest request)
        {
            throw new NotImplementedException();
        }

        private async Task<IDataResult> ProcessTeeTime(OAPITeeTimeRequest request, string supplierCode, string golfclubCode)
        {
            OAPIResponseBase response = new OAPIResponseBase();

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 공급사 코드로 공급사 ID 조회
                    var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.SupplierCode == supplierCode);

                    int supplierId = supplier.SupplierId;

                    var existingGolfclub = await _context.GolfClubs.FirstOrDefaultAsync(g => g.SupplierId == supplierId && g.GolfClubCode == golfclubCode);

                    if (existingGolfclub == null)
                    {
                        // 골프장이 유효하지 않을때 처리
                        response.IsSuccess = false;
                        response.RstCd = ExtensionMethods.GetDescription(ResultCode.INVALID_INPUT);
                        response.RstMsg = $"{ExtensionMethods.GetDescription(ResultCode.INVALID_INPUT)}(StatusCode:{ResultCode.INVALID_INPUT}) golfclubCode";
                        response.StatusCode = (int)ResultCode.INVALID_INPUT;
                    }

                    // 1. 기존 요금 정책, 날짜 슬롯, 시간 슬롯, 티타임 정보를 모두 조회
                    var existingPricePolicies = await _context.PricePolicies.ToListAsync();
                    var dateSlots = await _context.DateSlots.ToListAsync();
                    var timeSlots = await _context.TimeSlots.ToListAsync();
                    var golfClubCourses = await _context.GolfClubs
                        .Where(gc => gc.GolfClubCode == golfclubCode && gc.SupplierId == supplierId)
                        .Include(gc => gc.Courses)
                        .SelectMany(gc => gc.Courses)
                        .ToListAsync();
                    var teeTimes = await _context.TeeTimes.Where(tt => tt.SupplierId == supplierId).ToListAsync();

                    // 추가할 티타임 매핑 목록 생성 (일괄 삽입을 위해)
                    var teeTimeMappings = new List<OAPI_TeeTimeMapping>();
                    var pricePolicyMappings = new List<OAPI_TeetimePriceMapping>();

                    // 1. 코스 코드 수 만큼 티타임 추가
                    foreach (var teeTimeInfo in request.TeeTimeInfo)
                    {
                        foreach (var courseCode in teeTimeInfo.CourseCode)
                        {
                            var golfClubCourse = golfClubCourses.FirstOrDefault(gc => gc.CourseCode == courseCode);
                            if (golfClubCourse == null)
                            {
                                continue;
                            }

                            var teeTime = teeTimes.FirstOrDefault(tt => tt.GolfClubCourseId == golfClubCourse.GolfClubCourseId && tt.MinMembers == teeTimeInfo.MinMembers && tt.MaxMembers == teeTimeInfo.MaxMembers);
                            if (teeTime == null)
                            {
                                // 새로운 티타임 추가
                                var newTeeTime = new OAPI_TeeTime
                                {
                                    GolfClubCourseId = golfClubCourse.GolfClubCourseId,
                                    SupplierId = supplierId,
                                    MinMembers = teeTimeInfo.MinMembers,
                                    MaxMembers = teeTimeInfo.MaxMembers,
                                    CreatedDate = DateTime.UtcNow
                                };
                                _context.TeeTimes.Add(newTeeTime);
                                await _context.SaveChangesAsync();
                                teeTime = newTeeTime;
                                teeTimes.Add(newTeeTime);
                            }
                        }
                    }

                    // 2. 요금 정책 추가
                    foreach (var teeTimeInfo in request.TeeTimeInfo)
                    {
                        foreach (var price in teeTimeInfo.Price)
                        {
                            var existingPolicy = existingPricePolicies.FirstOrDefault(p =>
                                p.PlayerCount == price.PlayerCount &&
                                p.GreenFee == price.GreenFee &&
                                p.CartFee == price.CartFee &&
                                p.CaddyFee == price.CaddyFee &&
                                p.Tax == price.Tax &&
                                p.AdditionalTax == price.AdditionalTax &&
                                p.UnitPrice == price.UnitPrice);

                            int pricePolicyId;

                            if (existingPolicy != null)
                            {
                                // 중복된 요금 정책이 있는 경우 기존 키값 사용
                                pricePolicyId = existingPolicy.PricePolicyId;
                            }
                            else
                            {
                                // 중복된 요금 정책이 없는 경우 새로 추가 후 키값 가져옴
                                var newPricePolicy = new OAPI_PricePolicy
                                {
                                    PlayerCount = price.PlayerCount,
                                    GreenFee = price.GreenFee,
                                    CartFee = price.CartFee,
                                    CaddyFee = price.CaddyFee,
                                    Tax = price.Tax,
                                    AdditionalTax = price.AdditionalTax,
                                    UnitPrice = price.UnitPrice,
                                    CreatedDate = DateTime.UtcNow
                                };

                                _context.PricePolicies.Add(newPricePolicy);
                                await _context.SaveChangesAsync();
                                pricePolicyId = newPricePolicy.PricePolicyId;

                                // 새로 추가된 요금 정책을 기존 리스트에도 추가
                                existingPricePolicies.Add(newPricePolicy);
                            }

                            // 3. 티타임 매핑 정보 추가
                            IEnumerable<string> applicableDates = request.dateApplyType == 1
                                ? Enumerable.Range(0, (DateTime.ParseExact(request.endPlayDate, "yyyyMMdd", null) - DateTime.ParseExact(request.startPlayDate, "yyyyMMdd", null)).Days + 1)
                                    .Select(offset => DateTime.ParseExact(request.startPlayDate, "yyyyMMdd", null).AddDays(offset).ToString("yyyyMMdd"))
                                : request.effectiveDate;

                            foreach (var date in applicableDates)
                            {
                                var dateSlot = dateSlots.FirstOrDefault(ds => ds.PlayDate == date);
                                if (dateSlot == null)
                                {
                                    continue;
                                }

                                // 요일 및 예외일 처리
                                if (request.week.Any() && !request.week.Contains((int)DateTime.ParseExact(dateSlot.PlayDate, "yyyyMMdd", null).DayOfWeek + 1))
                                {
                                    continue; // 요일에 해당하지 않으면 건너뜀
                                }

                                if (request.exceptionDate.Contains(dateSlot.PlayDate))
                                {
                                    continue; // 예외일이면 건너뜀
                                }

                                foreach (var time in teeTimeInfo.Time)
                                {
                                    var timeSlot = timeSlots.FirstOrDefault(ts => ts.StartTime == time.StartTime);
                                    if (timeSlot == null)
                                    {
                                        continue;
                                    }

                                    for (int i = 0; i < teeTimeInfo.CourseCode.Count; i++)
                                    {
                                        var courseCode = teeTimeInfo.CourseCode[i];
                                        var teeTimeCode = time.TeeTimeCode.Count > i ? time.TeeTimeCode[i] : null;
                                        if (teeTimeCode == null)
                                        {
                                            continue;
                                        }

                                        var golfClubCourse = golfClubCourses.FirstOrDefault(gc => gc.CourseCode == courseCode);
                                        if (golfClubCourse == null)
                                        {
                                            continue;
                                        }

                                        var teeTime = teeTimes.FirstOrDefault(tt => tt.GolfClubCourseId == golfClubCourse.GolfClubCourseId && tt.MinMembers == teeTimeInfo.MinMembers && tt.MaxMembers == teeTimeInfo.MaxMembers);
                                        if (teeTime == null)
                                        {
                                            continue;
                                        }

                                        var newTeeTimeMapping = new OAPI_TeeTimeMapping
                                        {
                                            TeetimeId = teeTime.TeetimeId,
                                            DateSlotId = dateSlot.DateSlotId,
                                            TimeSlotId = timeSlot.TimeSlotId,
                                            SupplierTeetimeCode = teeTimeCode,
                                            IsAvailable = true,
                                            IsDisable = false,
                                            IsDeleted = false,
                                            CreatedDate = DateTime.UtcNow
                                        };

                                        teeTimeMappings.Add(newTeeTimeMapping);

                                        // 4. 요금 정책과 티타임 매핑 추가
                                        var newPricePolicyMapping = new OAPI_TeetimePriceMapping
                                        {
                                            TeeTimeMappingId = newTeeTimeMapping.TeeTimeMappingId,
                                            PricePolicyId = pricePolicyId
                                        };

                                        pricePolicyMappings.Add(newPricePolicyMapping);
                                    }
                                }

                            }
                        }
                    }

                    // 5. 티타임 매핑 및 요금 정책 매핑 일괄 삽입
                    if (teeTimeMappings.Any())
                    {
                        await _context.TeeTimeMappings.AddRangeAsync(teeTimeMappings);
                        await _context.SaveChangesAsync();
                    }

                    if (pricePolicyMappings.Any())
                    {
                        await _context.TeetimePriceMappings.AddRangeAsync(pricePolicyMappings);
                        await _context.SaveChangesAsync();
                    }

                    // 트랜잭션 커밋
                    await transaction.CommitAsync();

                    response.IsSuccess = true;
                    response.RstCd = ExtensionMethods.GetDescription(ResultCode.SUCCESS);
                    response.RstMsg = $"{ExtensionMethods.GetDescription(ResultCode.SUCCESS)}(StatusCode:{ResultCode.SUCCESS})";
                    response.StatusCode = (int)ResultCode.SUCCESS;
                }
                catch (Exception ex)
                {
                    // 오류 발생 시 트랜잭션 롤백
                    await transaction.RollbackAsync();

                    response.IsSuccess = false;
                    response.RstCd = ExtensionMethods.GetDescription(ResultCode.SERVER_ERROR);
                    response.RstMsg = $"{ExtensionMethods.GetDescription(ResultCode.SERVER_ERROR)}(StatusCode:{ResultCode.SERVER_ERROR}) ";
                    response.StatusCode = (int)ResultCode.SERVER_ERROR;
                    return response;
                    //throw new DomainException(ResultCode.SERVER_ERROR, $"Unauthorized(StatusCode:{ResultCode.SERVER_ERROR}) Missing golfclubCode Code");
                }


            }
            return response;
        }
    }
}

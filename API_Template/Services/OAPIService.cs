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
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using static AGL.Api.API_Template.Models.OAPI.OAPI;
using static AGL.Api.API_Template.Models.OAPI.OAPIResponse;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        public async Task<IDataResult> GetTeeTime(OAPITeeTimeGetRequest request, string supplierCode)
        {
            if (string.IsNullOrEmpty(request.startDate) || string.IsNullOrEmpty(request.endDate))
            {
                return CreateResponse(false, ResultCode.INVALID_INPUT, "startDate or ) is invalid", null);
            }

            try
            {
                var startDateParsed = request.startDate;
                var endDateParsed = request.endDate;

                var teeTimeList = await _context.TeeTimes
                        .Include(t => t.TeeTimeMappings)
                            .ThenInclude(tm => tm.TeetimePriceMappings)
                        .Include(t => t.TeeTimeMappings)
                            .ThenInclude(tm => tm.TeetimeRefundMappings)
                        .Include(t => t.GolfClub)
                        .Where(t => t.GolfClub.Supplier.SupplierCode == supplierCode && t.TeeTimeMappings.Any(tm => string.Compare(tm.DateSlot.PlayDate, startDateParsed) >= 0 && string.Compare(tm.DateSlot.PlayDate, endDateParsed) <= 0))
                        .ToListAsync();

                var teeTimeData = teeTimeList.GroupBy(t => t.GolfClub.GolfClubId)
                    .ToDictionary(g => g.Key.ToString(), g => g.Select(t => new TeeTimeInfo
                    {
                        CourseCode = t.GolfClub.Courses.Select(c => c.CourseCode).ToList(),
                        MinMembers = t.MinMembers,
                        MaxMembers = t.MaxMembers,
                        IncludeCart = t.IncludeCart,
                        IncludeCaddie = t.IncludeCaddie,
                        ReservationType = t.ReservationType,
                        Time = t.TeeTimeMappings.Select(tm => new TimeInfo
                        {
                            StartTime = tm.TimeSlot.StartTime,
                            TeeTimeCode = new List<string> { tm.SupplierTeetimeCode }
                        }).ToList(),
                        Price = t.TeeTimeMappings.SelectMany(tm => tm.TeetimePriceMappings).Select(tp => new PriceInfo
                        {
                            PlayerCount = tp.PricePolicy.PlayerCount,
                            GreenFee = tp.PricePolicy.GreenFee,
                            CartFee = tp.PricePolicy.CartFee,
                            CaddyFee = tp.PricePolicy.CaddyFee,
                            Tax = tp.PricePolicy.Tax,
                            AdditionalTax = tp.PricePolicy.AdditionalTax,
                            UnitPrice = tp.PricePolicy.UnitPrice
                        }).ToList(),
                        RefundPolicy = t.TeeTimeMappings.SelectMany(tm => tm.TeetimeRefundMappings).Select(tr => new RefundPolicy
                        {
                            RefundDate = tr.TeetimeRefundPolicy.RefundDate,
                            RefundFee = tr.TeetimeRefundPolicy.RefundFee,
                            RefundUnit = tr.TeetimeRefundPolicy.RefundUnit
                        }).ToList(),
                    }).ToList());

                return CreateResponse(true, ResultCode.SUCCESS, "Reservation confirmed successfully", teeTimeData);
            }
            catch (Exception ex)
            {
                return CreateResponse(false, ResultCode.SERVER_ERROR, ex.Message, null);
            }

        }

        public async Task<IDataResult> PostReservatioConfirm(OAPIReservationRequest request, string supplierCode)
        {
            var reservationId = request.reservationId;

            // 입력 값 검증 - reservationId와 supplierCode가 비어 있는지 확인
            if (string.IsNullOrEmpty(reservationId) || string.IsNullOrEmpty(supplierCode))
            {
                return CreateResponse(false, ResultCode.INVALID_INPUT, "reservationId or supplierCode is invalid", null);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 공급자 정보 조회 - supplierCode에 해당하는 공급자를 데이터베이스에서 검색
                    var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.SupplierCode == supplierCode);
                    if (supplier == null)
                    {
                        return CreateResponse(false, ResultCode.INVALID_INPUT, "Supplier not found", null);
                    }
                    int supplierId = supplier.SupplierId;

                    // 예약관리 DB에서 예약 조회 - 예약 번호, 상태, 공급자 ID를 기준으로 예약 검색
                    var reservation = await _context.ReservationManagements.FirstOrDefaultAsync(r => r.ReservationId == reservationId && r.ReservationStatus == 1 && r.SupplierId == supplierId);

                    if (reservation != null)
                    {
                        reservation.ReservationStatus = 2; // 1 예약요청 2 예약확정 3 예약취소
                        reservation.UpdatedDate = DateTime.Now;

                        _context.SaveChanges();
                    }
                    else
                    {
                        return CreateResponse(false, ResultCode.NOT_FOUND, "Reservation not found", null);
                    }

                    // 트랜잭션 커밋
                    await transaction.CommitAsync();

                    return CreateResponse(true, ResultCode.SUCCESS, "Reservation confirmed successfully", null);
                }
                catch (Exception ex)
                {
                    // 오류 발생 시 트랜잭션 롤백
                    await transaction.RollbackAsync();

                    return CreateResponse(false, ResultCode.SERVER_ERROR, ex.Message, null);
                }
            }
        }

        public async Task<IDataResult> PutTeeTimeAvailability(OAPITeeTimetAvailabilityRequest request, string supplierCode)
        {
            var golfClub = await _context.GolfClubs.FirstOrDefaultAsync(g => g.Supplier.SupplierCode == supplierCode && g.GolfClubCode == request.golfclubCode);

            if (golfClub == null)
            {
                return CreateResponse(false, ResultCode.INVALID_INPUT, "GolfClub not found", null);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 요청에서 제공된 티타임 코드 목록을 가져옴
                    var teeTimeCodes = request.time != null ? request.time.SelectMany(t => t.TeeTimeCode).ToList() : [];

                    // TeeTimeMappings 테이블에서 조건에 맞는 항목을 조회 (연관된 TeeTime, GolfClubCourse, DateSlot을 포함)
                    var existingTeeTimeMappingsQuery = _context.TeeTimeMappings
                        .Include(tm => tm.TeeTime)
                            .ThenInclude(t => t.GolfClubCourse)
                        .Include(tm => tm.DateSlot)
                        .Where(tm => tm.TeeTime.GolfClubCourse.GolfClubId == golfClub.GolfClubId &&
                                    request.courseCode.Contains(tm.SupplierTeetimeCode) &&
                                    tm.DateSlot.PlayDate == request.playDate);

                    // 티타임 코드 목록이 있을 경우 해당 코드들에 대한 조건 추가
                    if (teeTimeCodes.Count != 0)
                    {
                        existingTeeTimeMappingsQuery = existingTeeTimeMappingsQuery.Where(tm => teeTimeCodes.Contains(tm.SupplierTeetimeCode));
                    }

                    // 조건에 맞는 TeeTimePriceMappings 목록을 조회
                    var existingTeeTimeMappings = await existingTeeTimeMappingsQuery.ToListAsync();

                    // 조회된 티타임 가격 매핑에 대해 가용성 및 수정 날짜 업데이트를 벌크 업데이트로 수행
                    existingTeeTimeMappings.ForEach(teeTimeMapping =>
                    {
                        teeTimeMapping.IsAvailable = request.available;
                        teeTimeMapping.UpdatedDate = DateTime.UtcNow;
                    });

                    // 벌크 업데이트 수행
                    await _context.BulkUpdateAsync(existingTeeTimeMappings);

                    await transaction.CommitAsync();

                    return CreateResponse(true, ResultCode.SUCCESS, "Reservation confirmed successfully", null);
                }
                catch (Exception ex)
                {
                    // 오류 발생 시 트랜잭션 롤백
                    await transaction.RollbackAsync();

                    return CreateResponse(false, ResultCode.SERVER_ERROR, ex.Message, null);
                }
            }
        }

        private async Task<IDataResult> ProcessTeeTime(OAPITeeTimeRequest request, string supplierCode, string golfclubCode)
        {

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
                        return CreateResponse(false, ResultCode.INVALID_INPUT, "golfclubCode is invalid", null);
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
                            if (golfClubCourse == null) //골프장 코스 없을시
                            {
                                return CreateResponse(false, ResultCode.INVALID_INPUT, "golfClubCourse is invalid", null);
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
                                    // 날짜 DB에 없을시 처리 ( 날짜슬롯에 날짜 추가 )
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
                                        // 시간 DB에 없을시 처리 ( 시간슬롯에 시간 추가 )
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

                    return CreateResponse(true, ResultCode.SUCCESS, "Reservation confirmed successfully", null);
                }
                catch (Exception ex)
                {
                    // 오류 발생 시 트랜잭션 롤백
                    await transaction.RollbackAsync();

                    return CreateResponse(false, ResultCode.SERVER_ERROR, ex.Message, null);
                }


            }

        }


        private OAPIResponseBase CreateResponse(bool isSuccess, ResultCode resultCode, string message, Dictionary<string, List<TeeTimeInfo>>? data)
        {
            var response = new OAPITeeTimeGetResponse
            {
                IsSuccess = isSuccess,
                RstCd = ExtensionMethods.GetDescription(resultCode),
                RstMsg = $"{ExtensionMethods.GetDescription(resultCode)} (StatusCode: {(int)resultCode}) {message}",
                StatusCode = (int)resultCode
            };

            if (data != null)
            {
                response.Data = data;
            }

            return response;
        }

    }
}

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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using static AGL.Api.API_Template.Models.OAPI.OAPI;
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
                            //.ThenInclude(tm => tm.TeetimePriceMappings)
                        .Include(t => t.TeeTimeMappings)
                            //.ThenInclude(tm => tm.TeetimeRefundMappings)
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
                        //Price = t.TeeTimeMappings.SelectMany(tm => tm.TeetimePriceMappings).Select(tp => new PriceInfo
                        //{
                        //    PlayerCount = tp.PricePolicy.PlayerCount,
                        //    GreenFee = tp.PricePolicy.GreenFee,
                        //    CartFee = tp.PricePolicy.CartFee,
                        //    CaddyFee = tp.PricePolicy.CaddyFee,
                        //    Tax = tp.PricePolicy.Tax,
                        //    AdditionalTax = tp.PricePolicy.AdditionalTax,
                        //    UnitPrice = tp.PricePolicy.UnitPrice
                        //}).ToList(),
                        //RefundPolicy = t.TeeTimeMappings.SelectMany(tm => tm.TeetimeRefundMappings).Select(tr => new RefundPolicy
                        //{
                        //    RefundDate = tr.TeetimeRefundPolicy.RefundDate,
                        //    RefundFee = tr.TeetimeRefundPolicy.RefundFee,
                        //    RefundUnit = tr.TeetimeRefundPolicy.RefundUnit
                        //}).ToList(),
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

                    // 골프장 코드로 골프장 정보 조회
                    var existingGolfclub = await _context.GolfClubs.FirstOrDefaultAsync(g => g.SupplierId == supplierId && g.GolfClubCode == golfclubCode);
                    if (existingGolfclub == null)
                    {
                        return CreateResponse(false, ResultCode.INVALID_INPUT, "golfclubCode is invalid", null); // 골프장이 유효하지 않을때 처리
                    }

                    // 1. applicableDates 생성
                    IEnumerable<string> applicableDates;

                    // 날짜적용방법에 따라 조건 생성 ( 1: 기간 , 2: 특정날짜 )
                    if (request.dateApplyType == 1) // dateApplyType이 1인 경우: startDate부터 endDate까지의 날짜 생성
                    {
                        if (!DateTime.TryParseExact(request.startPlayDate, "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime startDate))
                        {
                            var startYear = int.Parse(request.startPlayDate.Substring(0, 4));
                            var startMonth = int.Parse(request.startPlayDate.Substring(5, 2));
                            startDate = new DateTime(startYear, startMonth, 1);
                        }

                        if (!DateTime.TryParseExact(request.endPlayDate, "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime endDate))
                        {
                            var endYear = int.Parse(request.endPlayDate.Substring(0, 4));
                            var endMonth = int.Parse(request.endPlayDate.Substring(5, 2));
                            int lastDay = DateTime.DaysInMonth(endYear, endMonth);
                            endDate = new DateTime(endYear, endMonth, lastDay);
                        }

                        applicableDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                                                    .Select(offset => startDate.AddDays(offset).ToString("yyyyMMdd"));
                    }
                    else // dateApplyType이 1이 아닌 경우: effectiveDate 리스트의 날짜들 사용
                    {
                        applicableDates = request.effectiveDate
                                                .Where(date => !string.IsNullOrWhiteSpace(date) &&DateTime.TryParseExact(date, "yyyy-MM-dd", null, DateTimeStyles.None, out _))
                                                .Select(date => DateTime.ParseExact(date, "yyyy-MM-dd", null).ToString("yyyyMMdd"))
                                                .ToList();
                    }

                    // 1. 기존 데이터 조회 (요금 정책, 환불 정책, 날짜 슬롯, 시간 슬롯, 코스정보, 티타임 정보 등)
                    var existingTeeTimePricePolicies = await _context.TeetimePricePolicies.ToListAsync();
                    var existingTeeTimeRefundPolicies = await _context.TeetimeRefundPolicies.ToListAsync();
                    var dateSlots = await _context.DateSlots.Where(ds => applicableDates.Contains(ds.PlayDate)).ToListAsync();
                    var timeSlots = await _context.TimeSlots.ToListAsync();
                    var golfClubCourses = await _context.GolfClubs
                        .Where(gc => gc.GolfClubCode == golfclubCode && gc.SupplierId == supplierId)
                        .Include(gc => gc.Courses)
                        .SelectMany(gc => gc.Courses)
                        .ToListAsync();
                    var teeTimes = await _context.TeeTimes.Where(tt => tt.SupplierId == supplierId).ToListAsync();
                    var existingTeeTimeMappings = await _context.TeeTimeMappings
                        .Where(ttm => ttm.TeeTime.SupplierId == supplierId &&
                                      ttm.TeeTime.GolfClubId == existingGolfclub.GolfClubId &&
                                      applicableDates.Contains(ttm.DateSlot.PlayDate))
                        .ToListAsync();

                    // 골프장 코스, 날짜 슬롯, 시간 슬롯, 기존 매핑을 딕셔너리로 변환하여 조회 성능 향상
                    var golfClubCourseMap = golfClubCourses.ToDictionary(gc => gc.CourseCode, gc => gc.GolfClubCourseId); // CourseCode와 GolfClubCourseId의 매핑 딕셔너리 생성
                    var dateSlotMap = dateSlots.ToDictionary(ds => ds.PlayDate, ds => ds.DateSlotId); // PlayDate와 DateSlotId의 매핑 딕셔너리 생성
                    var timeSlotMap = timeSlots.ToDictionary(ts => ts.StartTime, ts => ts.TimeSlotId); // StartTime과 TimeSlotId의 매핑 딕셔너리 생성
                    var existingTeeTimeMappingsMap = existingTeeTimeMappings.ToDictionary(ttm => (ttm.TeetimeId, ttm.DateSlotId, ttm.TimeSlotId));

                    // 기존 매핑된 티타임의 중복 체크를 위한 ConcurrentDictionary 생성
                    var existingTeeTimeMappingsSet = new ConcurrentDictionary<(int TeetimeId, int DateSlotId, int TimeSlotId), bool>(existingTeeTimeMappings.Select(ttm => new KeyValuePair<(int, int, int), bool>((ttm.TeetimeId, ttm.DateSlotId, ttm.TimeSlotId), true)));

                    // 코스 코드 수 만큼 티타임 추가
                    foreach (var teeTimeInfo in request.TeeTimeInfo)
                    {
                        foreach (var courseCode in teeTimeInfo.CourseCode)
                        {
                            // 골프장 코스 조회
                            var golfClubCourse = golfClubCourses.FirstOrDefault(gc => gc.CourseCode == courseCode);
                            if (golfClubCourse == null) //골프장 코스 없을시
                            {
                                return CreateResponse(false, ResultCode.INVALID_INPUT, "golfClubCourse is invalid", null);
                            }

                            // 기존 티타임 조회 또는 신규 티타임 추가
                            var teeTime = teeTimes.FirstOrDefault(tt => tt.GolfClubCourseId == golfClubCourse.GolfClubCourseId && tt.MinMembers == teeTimeInfo.MinMembers && tt.MaxMembers == teeTimeInfo.MaxMembers);
                            if (teeTime == null)
                            {
                                // 새로운 티타임 추가
                                var newTeeTime = new OAPI_TeeTime
                                {
                                    GolfClubCourseId = golfClubCourse.GolfClubCourseId,
                                    SupplierId = supplierId,
                                    GolfClubId = existingGolfclub.GolfClubId,
                                    MinMembers = teeTimeInfo.MinMembers,
                                    MaxMembers = teeTimeInfo.MaxMembers,
                                    IncludeCart = teeTimeInfo.IncludeCart,
                                    IncludeCaddie = teeTimeInfo.IncludeCaddie,
                                    ReservationType = teeTimeInfo.ReservationType,
                                    CreatedDate = DateTime.UtcNow
                                };
                                _context.TeeTimes.Add(newTeeTime);
                                await _context.SaveChangesAsync();
                                teeTime = newTeeTime;
                                teeTimes.Add(newTeeTime);
                            }
                        }

                        // 검색용 딕셔너리 현재 위치에서 만들어야 검색한 티타임 + 추가된 티타임 으로 만들수 있음
                        Dictionary<(int GolfClubCourseId, int MinMembers, int MaxMembers), OAPI_TeeTime> teeTimesDictionary = teeTimes.ToDictionary(t => (t.GolfClubCourseId, t.MinMembers, t.MaxMembers));

                        // 요금 정책 중복 확인 후 추가
                        var existingTeeTimePricePolicy = existingTeeTimePricePolicies.FirstOrDefault(p =>
                            Enumerable.Range(1, 6).All(count =>
                                p.GetType().GetProperty($"GreenFee_{count}")?.GetValue(p) as decimal? == teeTimeInfo.Price.FirstOrDefault(price => price.PlayerCount == count)?.GreenFee &&
                                p.GetType().GetProperty($"CartFee_{count}")?.GetValue(p) as decimal? == teeTimeInfo.Price.FirstOrDefault(price => price.PlayerCount == count)?.CartFee &&
                                p.GetType().GetProperty($"CaddyFee_{count}")?.GetValue(p) as decimal? == teeTimeInfo.Price.FirstOrDefault(price => price.PlayerCount == count)?.CaddyFee &&
                                p.GetType().GetProperty($"Tax_{count}")?.GetValue(p) as decimal? == teeTimeInfo.Price.FirstOrDefault(price => price.PlayerCount == count)?.Tax &&
                                p.GetType().GetProperty($"AdditionalTax_{count}")?.GetValue(p) as decimal? == teeTimeInfo.Price.FirstOrDefault(price => price.PlayerCount == count)?.AdditionalTax &&
                                p.GetType().GetProperty($"UnitPrice_{count}")?.GetValue(p) as decimal? == teeTimeInfo.Price.FirstOrDefault(price => price.PlayerCount == count)?.UnitPrice
                            ));

                        int pricePolicyId;
                        if (existingTeeTimePricePolicy != null)
                        {
                            pricePolicyId = existingTeeTimePricePolicy.PricePolicyId;
                        }
                        else
                        {
                            var newTeeTimePricePolicy = new OAPI_TeetimePricePolicy();
                            foreach (var price in teeTimeInfo.Price)
                            {
                                var count = price.PlayerCount;
                                newTeeTimePricePolicy.GetType().GetProperty($"GreenFee_{count}")?.SetValue(newTeeTimePricePolicy, price.GreenFee);
                                newTeeTimePricePolicy.GetType().GetProperty($"CartFee_{count}")?.SetValue(newTeeTimePricePolicy, price.CartFee);
                                newTeeTimePricePolicy.GetType().GetProperty($"CaddyFee_{count}")?.SetValue(newTeeTimePricePolicy, price.CaddyFee);
                                newTeeTimePricePolicy.GetType().GetProperty($"Tax_{count}")?.SetValue(newTeeTimePricePolicy, price.Tax);
                                newTeeTimePricePolicy.GetType().GetProperty($"AdditionalTax_{count}")?.SetValue(newTeeTimePricePolicy, price.AdditionalTax);
                                newTeeTimePricePolicy.GetType().GetProperty($"UnitPrice_{count}")?.SetValue(newTeeTimePricePolicy, price.UnitPrice);
                            }
                            newTeeTimePricePolicy.CreatedDate = DateTime.UtcNow;

                            _context.TeetimePricePolicies.Add(newTeeTimePricePolicy);
                            await _context.SaveChangesAsync();
                            pricePolicyId = newTeeTimePricePolicy.PricePolicyId;

                            // 새로운 정책 추가 후 리스트에 포함
                            existingTeeTimePricePolicies.Add(newTeeTimePricePolicy);
                        }

                        // 환불 정책 중복 확인 후 추가
                        var existingRefundPolicy = existingTeeTimeRefundPolicies.FirstOrDefault(rp =>
                            Enumerable.Range(1, teeTimeInfo.RefundPolicy.Count).All(i =>
                                ((rp.GetType().GetProperty($"RefundDate_{i}")?.GetValue(rp) as string) == null && teeTimeInfo.RefundPolicy[i - 1].RefundDate == null) ||
                                ((rp.GetType().GetProperty($"RefundDate_{i}")?.GetValue(rp) as string) == teeTimeInfo.RefundPolicy[i - 1].RefundDate.ToString("D2")) &&
                                //((rp.GetType().GetProperty($"RefundHour_{i}")?.GetValue(rp) as string) == null && teeTimeInfo.RefundPolicy[i - 1].RefundHour == null) ||
                                //((rp.GetType().GetProperty($"RefundHour_{i}")?.GetValue(rp) as string) == teeTimeInfo.RefundPolicy[i - 1].RefundHour?.ToString("D4")) &&
                                (rp.GetType().GetProperty($"RefundFee_{i}")?.GetValue(rp) as decimal?) == null && teeTimeInfo.RefundPolicy[i - 1].RefundFee == null ||
                                ((rp.GetType().GetProperty($"RefundFee_{i}")?.GetValue(rp) as decimal?) == teeTimeInfo.RefundPolicy[i - 1].RefundFee) &&
                                ((rp.GetType().GetProperty($"RefundUnit_{i}")?.GetValue(rp) as byte?) == null && teeTimeInfo.RefundPolicy[i - 1].RefundUnit == null) ||
                                ((rp.GetType().GetProperty($"RefundUnit_{i}")?.GetValue(rp) as byte?) == teeTimeInfo.RefundPolicy[i - 1].RefundUnit)
                            ));

                        int refundPolicyId;
                        if (existingRefundPolicy != null)
                        {
                            refundPolicyId = existingRefundPolicy.RefundPolicyId;
                        }
                        else
                        {
                            var newRefundPolicy = new OAPI_TeetimeRefundPolicy();
                            for (int i = 0; i < teeTimeInfo.RefundPolicy.Count; i++)
                            {
                                var refund = teeTimeInfo.RefundPolicy[i];
                                newRefundPolicy.GetType().GetProperty($"RefundDate_{i + 1}")?.SetValue(newRefundPolicy, refund.RefundDate.ToString("D2"));
                                //newRefundPolicy.GetType().GetProperty($"RefundHour_{i + 1}")?.SetValue(newRefundPolicy, "0000");
                                newRefundPolicy.GetType().GetProperty($"RefundFee_{i + 1}")?.SetValue(newRefundPolicy, refund.RefundFee);
                                newRefundPolicy.GetType().GetProperty($"RefundUnit_{i + 1}")?.SetValue(newRefundPolicy, (byte?)refund.RefundUnit);
                            }
                            newRefundPolicy.CreatedDate = DateTime.UtcNow;

                            _context.TeetimeRefundPolicies.Add(newRefundPolicy);
                            await _context.SaveChangesAsync();
                            refundPolicyId = newRefundPolicy.RefundPolicyId;

                            // 새로운 정책 추가 후 리스트에 포함
                            existingTeeTimeRefundPolicies.Add(newRefundPolicy);
                        }

                        // 병렬로 티타임 매핑 생성
                        var teeTimeMappings = new ConcurrentBag<OAPI_TeeTimeMapping>();
                        Parallel.ForEach(Partitioner.Create(applicableDates), date => // 최상위 루프에서만 병렬화 적용
                        {
                            foreach (var time in teeTimeInfo.Time)
                            {
                                foreach (var (course, code) in teeTimeInfo.CourseCode.Zip(time.TeeTimeCode))
                                {
                                    if (!golfClubCourseMap.TryGetValue(course, out var golfClubCourseId) ||
                                        !teeTimesDictionary.TryGetValue((golfClubCourseId, teeTimeInfo.MinMembers, teeTimeInfo.MaxMembers), out var teeTimeDictionary) ||
                                        !dateSlotMap.TryGetValue(date, out var dateSlotId) ||
                                        !timeSlotMap.TryGetValue(time.StartTime, out var timeSlotId))
                                        continue;

                                    var key = (teeTimeDictionary.TeetimeId, dateSlotId, timeSlotId);

                                    if (existingTeeTimeMappingsMap.TryGetValue(key, out var existingTeeTimeMapping))
                                    {
                                        // 기존 항목이 있을 경우 조건에 따라 업데이트
                                        if (existingTeeTimeMapping.PricePolicyId != pricePolicyId ||
                                            existingTeeTimeMapping.RefundPolicyId != refundPolicyId ||
                                            existingTeeTimeMapping.SupplierTeetimeCode != code)
                                        {
                                            existingTeeTimeMapping.PricePolicyId = pricePolicyId;
                                            existingTeeTimeMapping.RefundPolicyId = refundPolicyId;
                                            existingTeeTimeMapping.SupplierTeetimeCode = code;
                                            existingTeeTimeMapping.UpdatedDate = DateTime.UtcNow;
                                            teeTimeMappings.Add(existingTeeTimeMapping);
                                        }
                                    }
                                    else
                                    {
                                        // 새로운 항목 추가
                                        existingTeeTimeMappingsSet.TryAdd(key, true);
                                        teeTimeMappings.Add(new OAPI_TeeTimeMapping
                                        {
                                            TeetimeId = teeTimeDictionary.TeetimeId,
                                            DateSlotId = dateSlotId,
                                            TimeSlotId = timeSlotId,
                                            PricePolicyId = pricePolicyId,
                                            RefundPolicyId = refundPolicyId,
                                            SupplierTeetimeCode = code,
                                            IsAvailable = true,
                                            IsDisable = true,
                                            IsDeleted = false,
                                            CreatedDate = DateTime.UtcNow
                                        });
                                    }
                                }
                            }
                        });

                        var teeTimeMappingsList = teeTimeMappings.ToList();
                        if (teeTimeMappingsList.Any())
                        {
                            await _context.BulkInsertOrUpdateAsync(teeTimeMappingsList);
                        }
                    }

                    // 트랜잭션 커밋
                    await transaction.CommitAsync();

                    return CreateResponse(true, ResultCode.SUCCESS, "ProcessTeeTime successfully", null);
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

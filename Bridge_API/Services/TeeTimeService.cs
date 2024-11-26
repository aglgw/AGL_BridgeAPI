using AGL.Api.Bridge_API.Interfaces;
using AGL.Api.ApplicationCore.Extensions;
using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using AGL.Api.Domain.Entities.OAPI;
using AGL.Api.Infrastructure.Data;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Globalization;
using static AGL.Api.Bridge_API.Models.OAPI.OAPI;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIRequest;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIResponse;

namespace AGL.Api.Bridge_API.Services
{
    public class TeeTimeService : BaseService, ITeeTimeService
    {
        private readonly OAPI_DbContext _context;
        private IConfiguration _configuration { get; }
        private readonly ICommonService _commonService;
        private readonly RequestQueue _queue;

        public TeeTimeService(OAPI_DbContext context, IConfiguration configuration, ICommonService commonService, RequestQueue queue)
        {
            _context = context;
            _configuration = configuration;
            _commonService = commonService;
            _queue = queue;
        }

        public async Task<IDataResult> PostTeeTime(TeeTimeRequest request, string supplierCode)
        {
            return await ValidateTeeTime(request, supplierCode, request.golfClubCode, "POST");
        }

        public async Task<IDataResult> PutTeeTime(TeeTimeRequest request, string supplierCode)
        {
            return await ValidateTeeTime(request, supplierCode, request.golfClubCode, "PUT");
        }

        public async Task<TeeTimeResponse> GetTeeTime(TeeTimeGetRequest request, string supplierCode)
        {
            if (string.IsNullOrEmpty(request.startDate) && string.IsNullOrEmpty(request.endDate))
            {
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "startDate or EndDate is invalid", null);
            }
//var stopwatch = Stopwatch.StartNew();
            try
            {
                var startDateParsed = request.startDate;
                var endDateParsed = request.endDate;

                // 공급자 코드에 따른 가격 정책 가져오기
                var pricePolicies = await _context.TeetimePricePolicies
                    .Where(pp => pp.TeeTimeMappings.Any(tm => tm.TeeTime.GolfClub.Supplier.SupplierCode == supplierCode && tm.TeeTime.GolfClub.GolfClubCode == request.golfClubCode))
                    .ToDictionaryAsync(pp => pp.PricePolicyId);

                // 공급자 코드에 따른 환불 정책 가져오기
                var refundPolicies = await _context.TeetimeRefundPolicies
                    .Where(rp => rp.TeeTimeMappings.Any(tm => tm.TeeTime.GolfClub.Supplier.SupplierCode == supplierCode && tm.TeeTime.GolfClub.GolfClubCode == request.golfClubCode))
                    .ToDictionaryAsync(rp => rp.RefundPolicyId);

                // 날짜 범위에 해당하는 DateSlot의 ID 목록 가져오기
                var dateSlotIds = await _context.DateSlots
                    .Where(ds => string.Compare(ds.PlayDate, startDateParsed) >= 0 && string.Compare(ds.PlayDate, endDateParsed) <= 0)
                    .Select(ds => ds.DateSlotId)
                    .ToListAsync();

                // 골프장 코드와 공급자 코드에 해당하는 골프장 가져오기
                var golfClub = await _context.GolfClubs
                    .Where(gc => gc.GolfClubCode == request.golfClubCode)
                    .Select(gc => new { gc.GolfClubId, gc.SupplierId })
                    .FirstOrDefaultAsync();

                if (golfClub == null)
                {
                    return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Invalid GolfClubCode", null);
                }
//Debug.WriteLine($"[Step 1] Execution Time: {stopwatch.ElapsedMilliseconds} ms");
                // 주어진 골프장 코드와 날짜 범위에 해당하는 티타임 매핑 가져오기 (필요한 필드만 선택적으로 가져오기)
                var teeTimeMappings = await _context.TeeTimeMappings
                    .Where(tm => tm.TeeTime.GolfClubId == golfClub.GolfClubId
                    && dateSlotIds.Contains(tm.DateSlotId)
                    && tm.TeeTime.SupplierId == golfClub.SupplierId
                    && tm.IsAvailable == true)
                    .Select(tm => new
                    {
                        TeeTime = new
                        {
                            tm.TeeTime.MinMembers,
                            tm.TeeTime.MaxMembers,
                            tm.TeeTime.IncludeCart,
                            tm.TeeTime.IncludeCaddie,
                            tm.TeeTime.ReservationType,
                            tm.TeeTime.GolfClubCourse.CourseCode
                        },
                        tm.TimeSlot.StartTime,
                        tm.SupplierTeetimeCode,
                        tm.DateSlot.PlayDate,
                        tm.PricePolicyId,
                        tm.RefundPolicyId
                    })
                    .ToListAsync();

//Debug.WriteLine($"[Step 2] Execution Time: {stopwatch.ElapsedMilliseconds} ms");
                // 특정 속성들(최소/최대 인원, 가격 정책, 환불 정책)으로 티타임 그룹화
                var groupedTeeTimes = teeTimeMappings
                    .GroupBy(tm => new { tm.TeeTime.MinMembers, tm.TeeTime.MaxMembers, tm.PricePolicyId, tm.RefundPolicyId })
                    .Select(g => new
                    {
                        TeeTime = g.First().TeeTime,
                        PlayDates = g.Select(tm => DateTime.ParseExact(tm.PlayDate, "yyyyMMdd", null).ToString("yyyy-MM-dd")).Distinct().OrderBy(date => date).ToList(),
                        CourseCodes = g.Select(tm => tm.TeeTime.CourseCode).Distinct().ToList(),
                        Times = g.Select(tm => new { tm.StartTime, tm.SupplierTeetimeCode }).OrderBy(t => t.StartTime).Where(t => t.SupplierTeetimeCode != null || t.StartTime != null).ToList(),
                        PricePolicyId = g.Key.PricePolicyId,
                        RefundPolicyId = g.Key.RefundPolicyId
                    })
                    .ToList();

                // 응답 데이터 준비
                var responseData = new TeeTimeData
                {
                    teeTimeInfo = new List<TeeTimeInfo>()
                };
                //Debug.WriteLine($"[Step 3] Execution Time: {stopwatch.ElapsedMilliseconds} ms");
                // 그룹화된 티타임을 순회하며 TeeTimeInfo 객체 생성
                foreach (var groupedTeeTime in groupedTeeTimes)
                {
                    var teeTime = groupedTeeTime.TeeTime;
                    var pricePolicy = pricePolicies.ContainsKey(groupedTeeTime.PricePolicyId) ? pricePolicies[groupedTeeTime.PricePolicyId] : null;
                    var refundPolicy = refundPolicies.ContainsKey(groupedTeeTime.RefundPolicyId) ? refundPolicies[groupedTeeTime.RefundPolicyId] : null;
                    // 상세 정보를 포함한 TeeTimeInfo 객체 생성
                    var teeTimeInfo = new TeeTimeInfo
                    {
                        playDate = groupedTeeTime.PlayDates.ToList(),
                        courseCode = groupedTeeTime.CourseCodes,
                        minMembers = teeTime.MinMembers,
                        maxMembers = teeTime.MaxMembers,
                        includeCart = teeTime.IncludeCart,
                        includeCaddie = teeTime.IncludeCaddie,
                        reservationType = teeTime.ReservationType,
                        time = groupedTeeTime.Times.GroupBy(t => t.StartTime).Select(g => new TimeInfo
                        {
                            startTime = g.Key,
                            teeTimeCode = g.Any(t => t.SupplierTeetimeCode != null) ? g.Where(t => t.SupplierTeetimeCode != null).Select(t => t.SupplierTeetimeCode).Distinct().ToList() : null
                        }).ToList(),
                        price = pricePolicy != null ?
                            Enumerable.Range(1, 5).Select(playerCount =>
                            {
                                var greenFee = (decimal?)pricePolicy.GetType().GetProperty($"GreenFee_{playerCount}")?.GetValue(pricePolicy);
                                var cartFee = (decimal?)pricePolicy.GetType().GetProperty($"CartFee_{playerCount}")?.GetValue(pricePolicy);
                                var caddyFee = (decimal?)pricePolicy.GetType().GetProperty($"CaddyFee_{playerCount}")?.GetValue(pricePolicy);
                                var tax = (decimal?)pricePolicy.GetType().GetProperty($"Tax_{playerCount}")?.GetValue(pricePolicy);
                                var additionalTax = (decimal?)pricePolicy.GetType().GetProperty($"AdditionalTax_{playerCount}")?.GetValue(pricePolicy);
                                var unitPrice = (decimal?)pricePolicy.GetType().GetProperty($"UnitPrice_{playerCount}")?.GetValue(pricePolicy);

                                if (new[] { greenFee, cartFee, caddyFee, tax, additionalTax, unitPrice }.All(v => v == null))
                                {
                                    return null;
                                }

                                return new PriceInfo
                                {
                                    playerCount = playerCount,
                                    greenFee = greenFee ?? 0,
                                    cartFee = cartFee ?? 0,
                                    caddyFee = caddyFee ?? 0,
                                    tax = tax ?? 0,
                                    additionalTax = additionalTax ?? 0,
                                    unitPrice = unitPrice ?? 0
                                };
                            }).Where(p => p != null).Cast<PriceInfo>().ToList() : new List<PriceInfo>(),
                        refundPolicy = refundPolicy != null ?
                            Enumerable.Range(1, 5).Select(refundCount =>
                            {
                                var refundDate = refundPolicy.GetType().GetProperty($"RefundDate_{refundCount}")?.GetValue(refundPolicy) as int?;
                                var refundFee = refundPolicy.GetType().GetProperty($"RefundFee_{refundCount}")?.GetValue(refundPolicy) as decimal?;
                                var refundUnit = refundPolicy.GetType().GetProperty($"RefundUnit_{refundCount}")?.GetValue(refundPolicy) as byte?;

                                if (new[] { refundDate, refundFee, refundUnit }.All(v => v == null))
                                {
                                    return null;
                                }

                                return new RefundPolicy
                                {
                                    refundDate = refundDate ?? 0,
                                    refundFee = refundFee ?? 0,
                                    refundUnit = refundUnit ?? 0
                                };
                            }).Where(rp => rp != null).Cast<RefundPolicy>().ToList() : new List<RefundPolicy>()
                    };
//Debug.WriteLine($"[Step 4] Execution Time: {stopwatch.ElapsedMilliseconds} ms");
                    responseData.teeTimeInfo.Add(teeTimeInfo);
                }
                Utils.UtilLogs.LogRegHour(supplierCode, request.golfClubCode, $"TeeTime", $"티타임 정보 검색 성공");
                return await _commonService.CreateResponse(true, ResultCode.SUCCESS, "TeeTime List successfully", responseData);
            }
            catch (Exception ex)
            {
                Utils.UtilLogs.LogRegHour(supplierCode, request.golfClubCode, $"TeeTime", $"티타임 정보 검색 실패 {ex.Message}", true);
                return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
            }
        }

        public async Task<IDataResult> PutTeeTimeAvailability(TeeTimeAvailabilityRequest request, string supplierCode)
        {
            var golfClub = await _context.GolfClubs.FirstOrDefaultAsync(g => g.Supplier.SupplierCode == supplierCode && g.GolfClubCode == request.golfClubCode);

            if (golfClub == null)
            {
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "GolfClub not found", null);
            }

            string dateFormat = "yyyy-MM-dd";

            // PlayDate 유효성 검사
            if (!DateTime.TryParseExact(request.playDate, dateFormat, null, System.Globalization.DateTimeStyles.None, out DateTime startDate))
            {
                Utils.UtilLogs.LogRegHour(supplierCode, request.golfClubCode, "TeeTime", "시작일 없음");
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "PlayDate is not in the correct format. Expected format is yyyy-MM-dd", null);
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 요청에서 제공된 티타임 코드 목록을 가져옴
                        var startTimes = request.time != null ? request.time.Select(t => t.startTime).ToList() : new List<string>();
                        var teeTimeCodes = request.time != null ? request.time.SelectMany(t => t.teeTimeCode).ToList() : [];
                        var playDate = request.playDate.Replace("-", "");

                        // TeeTimeMappings 테이블에서 조건에 맞는 항목을 조회 (연관된 TeeTime, GolfClubCourse, DateSlot을 포함)
                        var existingTeeTimeMappingsQuery = _context.TeeTimeMappings
                            .Include(tm => tm.TeeTime) // TeeTime 엔터티 포함
                                .ThenInclude(t => t.GolfClubCourse) // GolfClubCourse 엔터티 포함 (연관관계)
                            .Include(tm => tm.DateSlot) // DateSlot 엔터티 포함 (연관관계)
                            .Include(tm => tm.TimeSlot) // TimeSlot 엔터티 포함 (연관관계)
                            .Where(tm => tm.TeeTime.GolfClubCourse.GolfClub.GolfClubCode == request.golfClubCode && // 요청된 골프장 코드와 일치 확인
                                         request.courseCode.Contains(tm.TeeTime.GolfClubCourse.CourseCode) && // 요청된 코스 코드가 TeeTime의 GolfClubCourse와 일치하는지 확인
                                         tm.DateSlot.PlayDate == playDate && // 요청된 플레이 날짜와 일치하는 DateSlot 확인
                                         startTimes.Contains(tm.TimeSlot.StartTime.ToString()));  // 요청된 시작 시간과 일치하는지 확인

                        // 티타임 코드 목록이 있을 경우 해당 코드들에 대한 조건 추가
                        if (teeTimeCodes.Count != 0)
                        {
                            existingTeeTimeMappingsQuery = existingTeeTimeMappingsQuery.Where(tm => teeTimeCodes.Contains(tm.SupplierTeetimeCode));
                        }

                        // 조건에 맞는 TeeTimePriceMappings 목록을 조회
                        var existingTeeTimeMappings = await existingTeeTimeMappingsQuery.ToListAsync();

                        if (!existingTeeTimeMappings.Any() || existingTeeTimeMappings == null)
                        {
                            Utils.UtilLogs.LogRegHour(supplierCode, request.golfClubCode, $"TeeTime", $"티타임 없음으로 상태 변경 실패", true);
                            return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "TeeTime not found", null);
                        }

                        // 조회된 티타임 가격 매핑에 대해 가용성 및 수정 날짜 업데이트를 벌크 업데이트로 수행
                        existingTeeTimeMappings.ForEach(teeTimeMapping =>
                        {
                            teeTimeMapping.IsAvailable = request.available;
                            teeTimeMapping.UpdatedDate = DateTime.UtcNow;
                        });

                        // 벌크 업데이트 수행
                        await _context.BulkUpdateAsync(existingTeeTimeMappings);

                        await transaction.CommitAsync();

                        Utils.UtilLogs.LogRegHour(supplierCode, request.golfClubCode, $"TeeTime", $"티타임 상태 변경 성공");
                        return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "TeeTime Availability successfully", null);
                    }
                    catch (Exception ex)
                    {
                        // 오류 발생 시 트랜잭션 롤백
                        await transaction.RollbackAsync();
                        Utils.UtilLogs.LogRegHour(supplierCode, request.golfClubCode, $"TeeTime", $"티타임 상태 변경 실패 {ex.Message}", true);
                        return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
                    }
                }
            });
        }

        private async Task<IDataResult> ValidateTeeTime(TeeTimeRequest request, string supplierCode, string golfClubCode, string mode)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.GolfClubs)
                    .ThenInclude(gc => gc.Courses)
                .FirstOrDefaultAsync(s => s.SupplierCode == supplierCode);

            // 골프장 코드 유효성
            if (string.IsNullOrEmpty(golfClubCode) || !supplier.GolfClubs.Any(gc => gc.GolfClubCode == golfClubCode))
            {
                Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "TeeTime", "골프장 코드 없음",true);
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "golfclubCode not found", null);
            }

            // 골프장 코스 유효성
            var golfClub = supplier.GolfClubs.FirstOrDefault(gc => gc.GolfClubCode == golfClubCode);
            if (golfClub.Courses == null || !golfClub.Courses.Any())
            {
                Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "TeeTime", "골프장 코스 없음", true);
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Golf club or courses not found", null);
            }

            // 코스 코드 유효성 
            foreach (var teeTimeInfo in request.teeTimeInfo)
            {
                foreach (var courseCode in teeTimeInfo.courseCode)
                {
                    // 골프장 코스 조회
                    var golfClubCourse = golfClub.Courses.FirstOrDefault(gc => gc.CourseCode == courseCode);
                    if (golfClubCourse == null) //골프장 코스 없을시
                    {
                        Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "TeeTime", "코스 코드 없음", true);
                        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "golfClubCourse is invalid", null);
                    }
                }
            }

            if (request.dateApplyType == 1) // 날짜적용방법이 1번 일때 시작일과 종료일이 있어야 함
            {
                string dateFormat = "yyyy-MM-dd";

                // StartPlayDate 유효성 검사
                if (!DateTime.TryParseExact(request.startPlayDate, dateFormat, null, System.Globalization.DateTimeStyles.None, out DateTime startDate))
                {
                    Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "TeeTime", "시작일 없음");
                    return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "StartPlayDate is not in the correct format. Expected format is yyyy-MM-dd", null);
                }

                // EndPlayDate 유효성 검사
                 if (!DateTime.TryParseExact(request.endPlayDate, dateFormat, null, System.Globalization.DateTimeStyles.None, out DateTime endDate))
                {
                    Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "TeeTime", "종료일 없음");
                    return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "EndPlayDate is not in the correct format. Expected format is yyyy-MM-dd", null);
                }

                // StartPlayDate가 EndPlayDate보다 빠른 날짜인지 확인
                if (startDate > endDate)
                {
                    Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "TeeTime", "시작일이 종료일보다 빠른 날짜임");
                    return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "StartPlayDate cannot be later than EndPlayDate", null);
                }
            }
            else if (request.dateApplyType == 2) // 날짜적용방법이 2번 일때 EffectiveDate이 있어야 함
            {
                if (request.effectiveDate == null || request.effectiveDate.Any()) // Assuming EffectiveDate is StartPlayDate
                {
                    Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "TeeTime", "적용일 없음");
                    return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "EffectiveDate not found", null);
                }
            }

            // TeeTimeBackgroundRequest 객체 생성 및 SupplierCode 설정
            TeeTimeBackgroundRequest teeTimeBackgroundRequest = new TeeTimeBackgroundRequest
            {
                supplierCode = supplierCode
            };

            // TeeTimeRequest의 모든 속성을 TeeTimeBackgroundRequest로 복사
            foreach (var property in typeof(TeeTimeRequest).GetProperties())
            {
                property.SetValue(teeTimeBackgroundRequest, property.GetValue(request));
            }

            if (mode == "PUT") // 업데이트 일때 큐방식
            {
                Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "TeeTime", "queue 로 진행");
                _queue.Enqueue(teeTimeBackgroundRequest);
                return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "ProcessTeeTime successfully", null);
            }
            else 
            {
                return await ProcessTeeTime(request, supplierCode, request.golfClubCode);
            }

        }

        public async Task<IDataResult> ProcessTeeTime(TeeTimeRequest request, string supplierCode, string golfClubCode)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
//var stopwatch = Stopwatch.StartNew();

            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 공급사 코드로 공급사 ID 조회
                        var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.SupplierCode == supplierCode);
                        int supplierId = supplier.SupplierId;

                        // 골프장 코드로 골프장 정보 조회
                        var existingGolfclub = await _context.GolfClubs.FirstOrDefaultAsync(g => g.Supplier != null && g.SupplierId == supplierId && g.GolfClubCode == golfClubCode);
                        if (existingGolfclub == null)
                        {
                            Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "TeeTime", "골프장 검색 코드 없음");
                            return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "golfclubCode is invalid", null); // 골프장이 유효하지 않을때 처리
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

                            // 요청한 기간 에서 요일, 특정일, 예외일 처리
                            applicableDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                                .Select(offset => startDate.AddDays(offset))
                                .Where(date => (request.week.Contains((int)date.DayOfWeek) || request.effectiveDate.Contains(date.ToString("yyyy-MM-dd"))) && !request.exceptionDate.Contains(date.ToString("yyyy-MM-dd")))
                                .Select(date => date.ToString("yyyyMMdd"))
                                .ToList();
                            Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "TeeTime", "날짜적용방법 1로 검색");
                        }
                        else // dateApplyType이 1이 아닌 경우: effectiveDate 리스트의 날짜들 사용
                        {
                            applicableDates = request.effectiveDate
                                .Where(date => !string.IsNullOrWhiteSpace(date) && DateTime.TryParseExact(date, "yyyy-MM-dd", null, DateTimeStyles.None, out _))
                                .Select(date => DateTime.ParseExact(date, "yyyy-MM-dd", null).ToString("yyyyMMdd"))
                                .ToList();
                            Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "TeeTime", "날짜적용방법 2로 검색");
                        }

                        // 1. 기존 데이터 조회 (요금 정책, 환불 정책, 날짜 슬롯, 시간 슬롯, 코스정보, 티타임 정보 등)
                        var existingTeeTimePricePolicies = await _context.TeetimePricePolicies.ToListAsync();
                        var existingTeeTimeRefundPolicies = await _context.TeetimeRefundPolicies.ToListAsync();
                        var dateSlots = await _context.DateSlots.Where(ds => applicableDates.Contains(ds.PlayDate)).ToListAsync();
                        var dateSlotIds = dateSlots.Select(ds => ds.DateSlotId).ToHashSet();
                        var timeSlots = await _context.TimeSlots.ToListAsync();
                        var golfClubCourses = await _context.GolfClubs
                            .Where(gc => gc.GolfClubCode == golfClubCode && gc.SupplierId == supplierId)
                            .Include(gc => gc.Courses)
                            .SelectMany(gc => gc.Courses)
                            .Select(gc => new
                            {
                                gc.CourseCode,
                                gc.GolfClubCourseId,
                            })
                            .ToListAsync();
                        var teeTimes = await _context.TeeTimes.Where(tt => tt.SupplierId == supplierId).ToListAsync();
//Debug.WriteLine($"[Step 1] Execution Time: {stopwatch.ElapsedMilliseconds} ms");
                        int batchSize = 10000; // 한 번에 처리할 ID 개수
                        List<OAPI_TeeTimeMapping> existingTeeTimeMappings = new List<OAPI_TeeTimeMapping>();

                        // dateSlotIds를 배치 크기로 나눔
                        var batchedDateSlotIds = dateSlotIds.Select((value, index) => new { value, index })
                                                             .GroupBy(x => x.index / batchSize)
                                                             .Select(g => g.Select(x => x.value).ToList());

                        foreach (var batch in batchedDateSlotIds)
                        {
                            var batchMappings = await _context.TeeTimeMappings
                                .AsNoTracking()
                                .Where(ttm => ttm.TeeTime.SupplierId == supplierId &&
                                              ttm.TeeTime.GolfClubId == existingGolfclub.GolfClubId &&
                                              batch.Contains(ttm.DateSlotId))
                                .ToListAsync();

                            existingTeeTimeMappings.AddRange(batchMappings);
                        }
//Debug.WriteLine($"[Step 2] Execution Time: {stopwatch.ElapsedMilliseconds} ms");
                        // 골프장 코스, 날짜 슬롯, 시간 슬롯, 기존 매핑을 딕셔너리로 변환하여 조회 성능 향상
                        var golfClubCourseMap = golfClubCourses.ToDictionary(gc => gc.CourseCode, gc => gc.GolfClubCourseId); // CourseCode와 GolfClubCourseId의 매핑 딕셔너리 생성
                        var dateSlotMap = dateSlots.ToDictionary(ds => ds.PlayDate, ds => ds.DateSlotId); // PlayDate와 DateSlotId의 매핑 딕셔너리 생성
                        var timeSlotMap = timeSlots.ToDictionary(ts => ts.StartTime, ts => ts.TimeSlotId); // StartTime과 TimeSlotId의 매핑 딕셔너리 생성
                        var existingTeeTimeMappingsMap = existingTeeTimeMappings.ToDictionary(ttm => (ttm.TeetimeId, ttm.DateSlotId, ttm.TimeSlotId));

                        // 기존 매핑된 티타임의 중복 체크를 위한 ConcurrentDictionary 생성
                        var existingTeeTimeMappingsSet = new ConcurrentDictionary<(int TeetimeId, int DateSlotId, int TimeSlotId), bool>(existingTeeTimeMappings.Select(ttm => new KeyValuePair<(int, int, int), bool>((ttm.TeetimeId, ttm.DateSlotId, ttm.TimeSlotId), true)));

                        // 코스 코드 수 만큼 티타임 추가
                        foreach (var teeTimeInfo in request.teeTimeInfo)
                        {
                            foreach (var courseCode in teeTimeInfo.courseCode)
                            {
                                // 골프장 코스 조회
                                var golfClubCourse = golfClubCourses.FirstOrDefault(gc => gc.CourseCode == courseCode);
                                //if (golfClubCourse == null) //골프장 코스 없을시
                                //{
                                //    Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "TeeTime", "코스 코드 없음",true);
                                //    return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "golfClubCourse is invalid", null);
                                //}

                                // 기존 티타임 조회 또는 신규 티타임 추가
                                var teeTime = teeTimes.FirstOrDefault(tt => tt.GolfClubCourseId == golfClubCourse.GolfClubCourseId && tt.MinMembers == teeTimeInfo.minMembers && tt.MaxMembers == teeTimeInfo.maxMembers);
                                if (teeTime == null)
                                {
                                    // 새로운 티타임 추가
                                    var newTeeTime = new OAPI_TeeTime
                                    {
                                        GolfClubCourseId = golfClubCourse.GolfClubCourseId,
                                        SupplierId = supplierId,
                                        GolfClubId = existingGolfclub.GolfClubId,
                                        MinMembers = teeTimeInfo.minMembers,
                                        MaxMembers = teeTimeInfo.maxMembers,
                                        IncludeCart = teeTimeInfo.includeCart,
                                        IncludeCaddie = teeTimeInfo.includeCaddie,
                                        ReservationType = teeTimeInfo.reservationType,
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
                                    p.GetType().GetProperty($"GreenFee_{count}")?.GetValue(p) as decimal? == teeTimeInfo.price.FirstOrDefault(price => price.playerCount == count)?.greenFee &&
                                    p.GetType().GetProperty($"CartFee_{count}")?.GetValue(p) as decimal? == teeTimeInfo.price.FirstOrDefault(price => price.playerCount == count)?.cartFee &&
                                    p.GetType().GetProperty($"CaddyFee_{count}")?.GetValue(p) as decimal? == teeTimeInfo.price.FirstOrDefault(price => price.playerCount == count)?.caddyFee &&
                                    p.GetType().GetProperty($"Tax_{count}")?.GetValue(p) as decimal? == teeTimeInfo.price.FirstOrDefault(price => price.playerCount == count)?.tax &&
                                    p.GetType().GetProperty($"AdditionalTax_{count}")?.GetValue(p) as decimal? == teeTimeInfo.price.FirstOrDefault(price => price.playerCount == count)?.additionalTax &&
                                    p.GetType().GetProperty($"UnitPrice_{count}")?.GetValue(p) as decimal? == teeTimeInfo.price.FirstOrDefault(price => price.playerCount == count)?.unitPrice
                                ));

                            int pricePolicyId;
                            if (existingTeeTimePricePolicy != null)
                            {
                                pricePolicyId = existingTeeTimePricePolicy.PricePolicyId;
                                Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "TeeTime", "요금 정책 있음");
                            }
                            else
                            {
                                var newTeeTimePricePolicy = new OAPI_TeetimePricePolicy();
                                foreach (var price in teeTimeInfo.price)
                                {
                                    var count = price.playerCount;
                                    newTeeTimePricePolicy.GetType().GetProperty($"GreenFee_{count}")?.SetValue(newTeeTimePricePolicy, price.greenFee);
                                    newTeeTimePricePolicy.GetType().GetProperty($"CartFee_{count}")?.SetValue(newTeeTimePricePolicy, price.cartFee);
                                    newTeeTimePricePolicy.GetType().GetProperty($"CaddyFee_{count}")?.SetValue(newTeeTimePricePolicy, price.caddyFee);
                                    newTeeTimePricePolicy.GetType().GetProperty($"Tax_{count}")?.SetValue(newTeeTimePricePolicy, price.tax);
                                    newTeeTimePricePolicy.GetType().GetProperty($"AdditionalTax_{count}")?.SetValue(newTeeTimePricePolicy, price.additionalTax);
                                    newTeeTimePricePolicy.GetType().GetProperty($"UnitPrice_{count}")?.SetValue(newTeeTimePricePolicy, price.unitPrice);
                                }
                                newTeeTimePricePolicy.CreatedDate = DateTime.UtcNow;

                                _context.TeetimePricePolicies.Add(newTeeTimePricePolicy);
                                await _context.SaveChangesAsync();
                                pricePolicyId = newTeeTimePricePolicy.PricePolicyId;

                                // 새로운 정책 추가 후 리스트에 포함
                                existingTeeTimePricePolicies.Add(newTeeTimePricePolicy);
                                Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "TeeTime", "요금 정책 없어서 추가");
                            }

                            // 환불 정책 중복 확인 후 추가
                            var sortedRefundPolicies = teeTimeInfo.refundPolicy.OrderByDescending(rp => rp.refundDate).ToList();
                            var existingRefundPolicy = existingTeeTimeRefundPolicies.FirstOrDefault(rp =>
                                sortedRefundPolicies.Count <= 5 && Enumerable.Range(1, sortedRefundPolicies.Count).All(i =>
                                    (rp.GetType().GetProperty($"RefundDate_{i}")?.GetValue(rp) as int?) == sortedRefundPolicies[i - 1].refundDate &&
                                    (rp.GetType().GetProperty($"RefundFee_{i}")?.GetValue(rp) as decimal?) == sortedRefundPolicies[i - 1].refundFee &&
                                    (rp.GetType().GetProperty($"RefundUnit_{i}")?.GetValue(rp) as byte?) == sortedRefundPolicies[i - 1].refundUnit
                                )
                            );

                            int refundPolicyId;
                            if (existingRefundPolicy != null)
                            {
                                refundPolicyId = existingRefundPolicy.RefundPolicyId;
                                Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "TeeTime", "환불 정책 있음");
                            }
                            else
                            {
                                var newRefundPolicy = new OAPI_TeetimeRefundPolicy();
                                var sortedRefundPolicy = teeTimeInfo.refundPolicy.OrderByDescending(r => r.refundDate).ToList();
                                for (int i = 0; i < teeTimeInfo.refundPolicy.Count; i++)
                                {
                                    var refund = teeTimeInfo.refundPolicy[i];
                                    newRefundPolicy.GetType().GetProperty($"RefundDate_{i + 1}")?.SetValue(newRefundPolicy, refund.refundDate);
                                    //newRefundPolicy.GetType().GetProperty($"RefundHour_{i + 1}")?.SetValue(newRefundPolicy, "0000");
                                    newRefundPolicy.GetType().GetProperty($"RefundFee_{i + 1}")?.SetValue(newRefundPolicy, refund.refundFee);
                                    newRefundPolicy.GetType().GetProperty($"RefundUnit_{i + 1}")?.SetValue(newRefundPolicy, (byte?)refund.refundUnit);
                                }
                                newRefundPolicy.CreatedDate = DateTime.UtcNow;

                                _context.TeetimeRefundPolicies.Add(newRefundPolicy);
                                await _context.SaveChangesAsync();
                                refundPolicyId = newRefundPolicy.RefundPolicyId;

                                // 새로운 정책 추가 후 리스트에 포함
                                existingTeeTimeRefundPolicies.Add(newRefundPolicy);
                                Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "TeeTime", "환불 정책 없어서 추가");
                            }

                            // 병렬로 티타임 매핑 생성
                            var teeTimeMappings = new ConcurrentBag<OAPI_TeeTimeMapping>();
//Debug.WriteLine($"[Step 3] Execution Time: {stopwatch.ElapsedMilliseconds} ms");
                            var partitioner = Partitioner.Create(applicableDates.ToList(), true);
                            Parallel.ForEach(partitioner, date =>
                            {
                                if (!dateSlotMap.TryGetValue(date, out var dateSlotId))
                                    return;

                                foreach (var time in teeTimeInfo.time)
                                {
                                    if (!timeSlotMap.TryGetValue(time.startTime, out var timeSlotId))
                                        continue;

                                    var minMembers = teeTimeInfo.minMembers;
                                    var maxMembers = teeTimeInfo.maxMembers;

                                    foreach (var course in teeTimeInfo.courseCode)
                                    {
                                        if (!golfClubCourseMap.TryGetValue(course, out var golfClubCourseId))
                                            continue;

                                        // time.teeTimeCode가 null이 아니고, 현재 인덱스가 teeTimeCode 리스트의 범위 내라면 값을 사용합니다.
                                        string code = null;
                                        int index = teeTimeInfo.courseCode.IndexOf(course);
                                        if (time.teeTimeCode != null && index < time.teeTimeCode.Count)
                                        {
                                            code = time.teeTimeCode[index];
                                        }

                                        var key = (golfClubCourseId, minMembers, maxMembers);
                                        if (!teeTimesDictionary.TryGetValue(key, out var teeTimeDictionary))
                                            continue;

                                        var teetimeId = teeTimeDictionary.TeetimeId;
                                        var mappingKey = (teetimeId, dateSlotId, timeSlotId);

                                        if (existingTeeTimeMappingsMap.TryGetValue(mappingKey, out var existingTeeTimeMapping))
                                        {
                                            // 기존 항목이 있을 경우 조건에 따라 업데이트
                                            bool needsUpdate = existingTeeTimeMapping.TeetimeId != teetimeId ||
                                                               existingTeeTimeMapping.PricePolicyId != pricePolicyId ||
                                                               existingTeeTimeMapping.RefundPolicyId != refundPolicyId ||
                                                               !string.Equals(existingTeeTimeMapping.SupplierTeetimeCode, code);

                                            if (needsUpdate)
                                            {
                                                existingTeeTimeMapping.TeetimeId = teetimeId;
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
                                            if (existingTeeTimeMappingsSet.TryAdd(mappingKey, true))
                                            {
                                                teeTimeMappings.Add(new OAPI_TeeTimeMapping
                                                {
                                                    TeetimeId = teetimeId,
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
                                }
                            });

                            var teeTimeMappingsList = teeTimeMappings.ToList();
                            if (teeTimeMappingsList.Any())
                            {
                                Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "TeeTime", "티타임 저장 시작");
                                await _context.BulkInsertOrUpdateAsync(teeTimeMappingsList);
                            }
                        }

                        await transaction.CommitAsync();
                        Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "TeeTime", "티타임 처리 완료");
//Debug.WriteLine($"[Step 4] Execution Time: {stopwatch.ElapsedMilliseconds} ms");
//stopwatch.Stop();
                        return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "ProcessTeeTime successfully", null);
                    }
                    catch (Exception ex)
                    {
                        // 오류 발생 시 트랜잭션 롤백
                        await transaction.RollbackAsync();
                        Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, $"TeeTime", $"티타임 처리 실패 {ex.Message}",true);
                        return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
                    }
                }
            });
        }

    }
}

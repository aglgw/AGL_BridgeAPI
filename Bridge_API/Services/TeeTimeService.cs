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
using System.Diagnostics;
using AGL.Api.ApplicationCore.Utilities;
using StackExchange.Redis;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.IdentityModel.Tokens;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualBasic;
using AGL.Api.ApplicationCore.Helpers;

namespace AGL.Api.Bridge_API.Services
{
    public class TeeTimeService : BaseService, ITeeTimeService
    {
        private readonly OAPI_DbContext _context;
        private IConfiguration _configuration { get; }
        private readonly ICommonService _commonService;
        private readonly RequestQueue _queue;
        private readonly IRedisService _redisService;

        public TeeTimeService(OAPI_DbContext context, IConfiguration configuration, ICommonService commonService, RequestQueue queue, IRedisService redisService)
        {
            _context = context;
            _configuration = configuration;
            _commonService = commonService;
            _queue = queue;
            _redisService = redisService;
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
                Utils.UtilLogs.LogRegHour(supplierCode, request.golfClubCode, "TeeTimeList", "시작일 or 종료일 없음");
                return await _commonService.CreateResponse<TeeTimeData>(false, ResultCode.INVALID_INPUT, "startDate or EndDate is invalid", null);
            }
            var startDateParsed = request.startDate;
            var endDateParsed = request.endDate;

            string[] formats = { "yyyy-MM-dd", "yyyyMMdd" };

            // 시작일과 종료일의 차이가 3개월을 초과하는지 확인
            var startDate = DateTime.ParseExact(startDateParsed, formats, null, System.Globalization.DateTimeStyles.None);
            var endDate = DateTime.ParseExact(endDateParsed, formats, null, System.Globalization.DateTimeStyles.None);

            if ((endDate - startDate).TotalDays > 90)
            {
                Utils.UtilLogs.LogRegHour(supplierCode, request.golfClubCode, "TeeTimeList", "3달 이상 검색함");
                return await _commonService.CreateResponse<TeeTimeData>(false, ResultCode.INVALID_INPUT, "Date range cannot exceed 3 months", null);
            }
            //var stopwatch = Stopwatch.StartNew();
            //Debug.WriteLine($"[Step 0] Execution Time: {stopwatch.ElapsedMilliseconds} ms");
            try
            {
                // 공급자 코드에 따른 가격 정책 가져오기
                var pricePolicies = await _context.TeetimePricePolicies.ToDictionaryAsync(pp => pp.PricePolicyId);

                // 공급자 코드에 따른 환불 정책 가져오기
                var refundPolicies = await _context.TeetimeRefundPolicies.ToDictionaryAsync(rp => rp.RefundPolicyId);

                // 골프장 코드와 공급자 코드에 해당하는 골프장 가져오기
                var golfClub = await _context.GolfClubs
                    .Where(gc => gc.GolfClubCode == request.golfClubCode)
                    .Select(gc => new { gc.GolfClubId, gc.SupplierId })
                    .FirstOrDefaultAsync();

                if (golfClub == null)
                {
                    Utils.UtilLogs.LogRegHour(supplierCode, request.golfClubCode, "TeeTimeList", "골프장 검색 코드 없음");
                    return await _commonService.CreateResponse<TeeTimeData>(false, ResultCode.INVALID_INPUT, "Invalid GolfClubCode", null);
                }
                //Debug.WriteLine($"[Step 1] Execution Time: {stopwatch.ElapsedMilliseconds} ms");
                // 주어진 골프장 코드와 날짜 범위에 해당하는 티타임 매핑 가져오기 (필요한 필드만 선택적으로 가져오기)
                var teeTimeMappings = await _context.TeeTimeMappings
                    .Where(tm => tm.TeeTime.GolfClubId == golfClub.GolfClubId
                    && tm.DateSlot.StartDate >= startDate && tm.DateSlot.StartDate <= endDate
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
                        tm.DateSlot.StartDate,
                        tm.PricePolicyId,
                        RefundPolicyId = tm.RefundPolicyId ?? 0
                    })
                    .ToListAsync();

                if (!teeTimeMappings.Any())
                {
                    Utils.UtilLogs.LogRegHour(supplierCode, request.golfClubCode, "TeeTimeList", "티타임 검색 내용 없음");
                    return await _commonService.CreateResponse<TeeTimeData>(false, ResultCode.NOT_FOUND, "The requested tee times are not available.", null);
                }

                //Debug.WriteLine($"[Step 2] Execution Time: {stopwatch.ElapsedMilliseconds} ms");
                // 특정 속성들(최소/최대 인원, 가격 정책, 환불 정책)으로 티타임 그룹화
                var groupedTeeTimes = teeTimeMappings
                    .GroupBy(tm => new { tm.TeeTime.MinMembers, tm.TeeTime.MaxMembers, tm.PricePolicyId, tm.RefundPolicyId })
                    .Select(g => new
                    {
                        TeeTime = g.First().TeeTime,
                        PlayDates = g.Select(tm => tm.StartDate.ToString("yyyy-MM-dd")).Distinct().OrderBy(date => date).ToList(), //
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
                responseData.teeTimeInfo = groupedTeeTimes.Select(groupedTeeTime =>
                {
                    var teeTime = groupedTeeTime.TeeTime;
                    var pricePolicy = pricePolicies.TryGetValue(groupedTeeTime.PricePolicyId, out var policy) ? policy : null;
                    var refundPolicy = refundPolicies.TryGetValue(groupedTeeTime.RefundPolicyId, out var refund) ? refund : null;

                    return new TeeTimeInfo
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
                            teeTimeCode = g.Any(t => t.SupplierTeetimeCode != null) ? g.Where(t => t.SupplierTeetimeCode != null)?.Select(t => t.SupplierTeetimeCode).Distinct().ToList() : null
                        }).ToList(),
                        price = pricePolicy != null ?
                            pricePolicy.PriceDetails.Select(pd => new PriceInfo
                            {
                                playerCount = pd.PlayerCount,
                                greenFee = pd.GreenFee ?? 0,
                                cartFee = pd.CartFee ?? 0,
                                caddyFee = pd.CaddyFee ?? 0,
                                tax = pd.Tax ?? 0,
                                additionalTax = pd.AdditionalTax ?? 0,
                                unitPrice = pd.UnitPrice ?? 0
                            }).ToList() : new List<PriceInfo>(),
                        refundPolicy = refundPolicy != null ?
                            refundPolicy.RefundDetails.Select((rd, index) => new RefundPolicy
                            {
                                refundDate = rd.RefundDate ?? 0,
                                refundFee = rd.RefundFee ?? 0,
                                refundUnit = rd.RefundUnit ?? 0
                            }).ToList() : new List<RefundPolicy>()
                    };

                }).ToList();
//Debug.WriteLine($"[Step 4] Execution Time: {stopwatch.ElapsedMilliseconds} ms");
                Utils.UtilLogs.LogRegHour(supplierCode, request.golfClubCode, $"TeeTimeList", $"티타임 정보 검색 성공");
                return await _commonService.CreateResponse(true, ResultCode.SUCCESS, "TeeTime List successfully", responseData);
            }
            catch (Exception ex)
            {
                Utils.UtilLogs.LogRegHour(supplierCode, request.golfClubCode, $"TeeTimeList", $"티타임 정보 검색 실패 {ex.Message}", true);
                return await _commonService.CreateResponse<TeeTimeData>(false, ResultCode.SERVER_ERROR, ex.Message, null);
            }
        }

        public async Task<IDataResult> PutTeeTimeAvailability(TeeTimeAvailabilityRequest request, string supplierCode)
        {
            var golfClub = await _context.GolfClubs.FirstOrDefaultAsync(g => g.Supplier.SupplierCode == supplierCode && g.GolfClubCode == request.golfClubCode);

            if (golfClub == null)
            {
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "GolfClub not found", null);
            }

            if(request.time.Any()) // time 유효성
            {
                foreach (var time in request.time)
                {
                    time.startTime = int.Parse(time.startTime).ToString("D4"); // startTime 4자리로 변환
                    var startTime = time.startTime;
                    // 4자리 숫자인지 확인
                    if (string.IsNullOrEmpty(startTime) || startTime.Length != 4 || !int.TryParse(startTime, out _))
                    {
                        Utils.UtilLogs.LogRegHour(supplierCode, request.golfClubCode, "TeeTime", "시작시간 형식 틀림");
                        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "The startTime value is invalid. It must be a 4-digit string in HHmm format.", null);
                    }

                    // 시간 값 추출
                    var hour = int.Parse(startTime.Substring(0, 2)); // 앞 2자리: 시(hour)
                    var minute = int.Parse(startTime.Substring(2, 2)); // 뒤 2자리: 분(minute)

                    // 유효한 시간 범위 확인
                    if (hour < 0 || hour > 23 || minute < 0 || minute > 59)
                    {
                        Utils.UtilLogs.LogRegHour(supplierCode, request.golfClubCode, "TeeTime", "시작 시간이 유효한 범위 벗어남");
                        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "The startTime value is out of valid range. It must represent a valid time between 0000 and 2359.", null);
                    }
                }
            }


            //var RedisStrKey = $"PTTA:" + ComputeSha256.ComputeSha256RequestHash(request);

            //try
            //{
            //    if (await _redisService.KeyExistsAsync(RedisStrKey)) // Redis 키 조회 (비동기)
            //    {
            //        Utils.UtilLogs.LogRegHour(supplierCode, request.golfClubCode, "TeeTime", $"티타임 상태 변경 중복");
            //        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Duplicate request", null);
            //    }
            //    else
            //    {
            //        await _redisService.SetValueAsync(RedisStrKey, "", TimeSpan.FromMinutes(1)); // 비동기로 Redis 키 설정
            //    }
            //}
            //catch (RedisException ex)
            //{
            //    Utils.UtilLogs.LogRegDay(supplierCode, request.golfClubCode, "TeeTime", $"티타임 상태 변경 Redis 실패 {ex.Message}", true);
            //    return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
            //}

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var courseCodes = (request.courseCode as List<string>) ?? ObjectToListConverter.ConvertToListOfStrings(request.courseCode, "courseCode");

                        // 요청에서 제공된 티타임 코드 목록을 가져옴
                        // TeeTimeMappings 테이블에서 조건에 맞는 항목을 조회 (연관된 TeeTime, GolfClubCourse, DateSlot을 포함)
                        var existingTeeTimeMappingsQuery = _context.TeeTimeMappings
                            .Include(tm => tm.TeeTime) // TeeTime 엔터티 포함
                                .ThenInclude(t => t.GolfClubCourse) // GolfClubCourse 엔터티 포함 (연관관계)
                            .Include(tm => tm.DateSlot) // DateSlot 엔터티 포함 (연관관계)
                            .Include(tm => tm.TimeSlot) // TimeSlot 엔터티 포함 (연관관계)
                            .Where(tm => tm.TeeTime.GolfClubCourse.GolfClub.GolfClubCode == request.golfClubCode); // 요청된 골프장 코드와 일치 확인

                        if (courseCodes.Count() > 0)
                        {
                            existingTeeTimeMappingsQuery = existingTeeTimeMappingsQuery.Where(tm => courseCodes.Contains(tm.TeeTime.GolfClubCourse.CourseCode));  // 요청된 코스 코드가 TeeTime의 GolfClubCourse와 일치하는지 확인
                        }

                        if(!string.IsNullOrEmpty(request.playDate))
                        {
                            string dateFormat = "yyyy-MM-dd";

                            // PlayDate 유효성 검사
                            if (!DateTime.TryParseExact(request.playDate, dateFormat, null, System.Globalization.DateTimeStyles.None, out DateTime startDate))
                            {
                                Utils.UtilLogs.LogRegHour(supplierCode, request.golfClubCode, "TeeTime", "시작일 없음");
                                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "PlayDate is not in the correct format. Expected format is yyyy-MM-dd", null);
                            }

                            var playDate = DateTime.ParseExact(request.playDate, "yyyy-MM-dd", null);
                            if (string.IsNullOrEmpty(request.playDate))
                            {
                                existingTeeTimeMappingsQuery = existingTeeTimeMappingsQuery.Where(tm => tm.DateSlot.StartDate == playDate); // 요청된 플레이 날짜와 일치하는 DateSlot 확인
                            }
                        }

                        var startTimes = request.time != null ? request.time.Select(t => t.startTime).ToList() : new List<string>();
                        if (startTimes.Any())
                        {
                            existingTeeTimeMappingsQuery = existingTeeTimeMappingsQuery.Where(tm => startTimes.Contains(tm.TimeSlot.StartTime.ToString())); // 요청된 시작 시간과 일치하는지 확인
                        }

                        var teeTimeCodes = request.time != null ? request.time.SelectMany(t => t.teeTimeCode).ToList() : new List<string>();
                        // 티타임 코드 목록이 있을 경우 해당 코드들에 대한 조건 추가
                        if (teeTimeCodes.Count != 0)
                        {
                            existingTeeTimeMappingsQuery = existingTeeTimeMappingsQuery = existingTeeTimeMappingsQuery.Where(tm => teeTimeCodes.Contains(tm.SupplierTeetimeCode));
                        }

                        // ExecuteUpdateAsync를 사용하여 직접 업데이트 수행
                        await existingTeeTimeMappingsQuery.ExecuteUpdateAsync(s => s
                            .SetProperty(tm => tm.IsAvailable, request.available)
                            .SetProperty(tm => tm.UpdatedDate, DateTime.UtcNow));

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
            //var RedisStrKey = $"PTT:"+ComputeSha256.ComputeSha256RequestHash(request);

            //try
            //{
            //    if (await _redisService.KeyExistsAsync(RedisStrKey)) // Redis 키 조회 (비동기)
            //    {
            //        Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "TeeTime", $"티타임 중복");
            //        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Duplicate request", null);
            //    }
            //    else
            //    {
            //        await _redisService.SetValueAsync(RedisStrKey, "", TimeSpan.FromMinutes(1)); // 비동기로 Redis 키 설정
            //    }
            //}
            //catch (RedisException ex)
            //{
            //    Utils.UtilLogs.LogRegDay(supplierCode, golfClubCode, "TeeTime", $"티타임 Redis 실패 {ex.Message}", true);
            //    return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
            //}

            try
            {
                Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessTeeTime", "유효성 처리 시작 ", true);

                // GolfClub 미등록 인 경우 처리
                var hasGolfClubs = await _context.Suppliers.AnyAsync(s => s.SupplierCode == supplierCode && s.GolfClubs.Any());
                if (!hasGolfClubs) 
                {
                    Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", "골프장 등록 안됨", true);
                    return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Please register the golf club before use", null);
                }

                var supplier = await _context.Suppliers
                .Include(s => s.GolfClubs)
                    .ThenInclude(gc => gc.Courses)
                .FirstOrDefaultAsync(s => s.SupplierCode == supplierCode);

                // 골프장 코드 유효성
                if (string.IsNullOrEmpty(golfClubCode) || !supplier.GolfClubs.Any(gc => gc.GolfClubCode == golfClubCode))
                {
                    Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", "골프장 코드 없음", true);
                    return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Golfclub Code not found", null);
                }

                // 골프장 코스 유효성
                var golfClub = supplier.GolfClubs.FirstOrDefault(gc => gc.GolfClubCode == golfClubCode);
                if (golfClub.Courses == null || !golfClub.Courses.Any())
                {
                    Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", "골프장 코스 없음", true);
                    return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Golfclub Curses not found", null);
                }

                // 티타임 유효성
                if (request.teeTimeInfo == null || !request.teeTimeInfo.Any())
                {
                    Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", "teeTimeInfo 없음", true);
                    return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "TeeTimeInfo cannot be null or empty", null);
                }

                // 코스 코드 유효성 
                foreach (var teeTime in request.teeTimeInfo)
                {
                    // 필수 필드 확인
                    if (teeTime.courseCode == null || !teeTime.courseCode.Any())
                    {
                        Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", "코스 코드 없음", true);
                        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "CourseCode is required in TeeTimeInfo", null);
                    }

                    // 최소 최대 인원이 0이하 인 경우
                    if (teeTime.minMembers <= 0 || teeTime.maxMembers <= 0)
                    {
                        Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", "minMembers 또는 maxMembers 유효하지 않음", true);
                        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "MinMembers and MaxMembers must be greater than 0", null);
                    }

                    // 최대인원보다 최소인원이 큰 경우
                    if (teeTime.maxMembers < teeTime.minMembers)
                    {
                        Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", "maxMembers가 minMembers보다 작음", true);
                        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "MaxMembers cannot be less than MinMembers", null);
                    }

                    // time 유효성
                    if (teeTime.time == null || !teeTime.time.Any())
                    {
                        Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", "시간 정보 없음", true);
                        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Time information is required in TeeTimeInfo", null);
                    }

                    // price 유효성
                    if (teeTime.price == null || !teeTime.price.Any())
                    {
                        Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", "가격 정보 없음", true);
                        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Price information is required in TeeTimeInfo", null);
                    }

                    foreach (var price in teeTime.price)
                    {
                        // playerCount 0이하 인 경우
                        if (price.playerCount <= 0)
                        {
                            Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", "playerCount 유효하지 않음", true);
                            return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "PlayerCount must be greater than 0", null);
                        }

                        // unitPrice 0이하 인 경우
                        if (price.unitPrice <= 0)
                        {
                            Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", "unitPrice 유효하지 않음", true);
                            return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "UnitPrice must be greater than 0", null);
                        }
                    }

                    // playerCount 가 중복된 경우
                    var playerCounts = teeTime.price.Select(p => p.playerCount).ToList();
                    if (playerCounts.Count != playerCounts.Distinct().Count())
                    {
                        Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", "playerCount 중복됨", true);
                        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Duplicate playerCount found in price list", null);
                    }

                    // 최소,최대 인원 대비 price의 인원수 별 금액이 맞는지 확인
                    var validPlayerCounts = Enumerable.Range((int)teeTime.minMembers, (int)teeTime.maxMembers - (int)teeTime.minMembers + 1);
                    var providedPlayerCounts = teeTime.price.Select(p => p.playerCount);
                    var missingPlayerCounts = validPlayerCounts.Where(count => !providedPlayerCounts.Contains(count)).ToList();

                    if (missingPlayerCounts.Any())
                    {
                        Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", $"playerCount 일치하지 않음 {string.Join(", ", missingPlayerCounts)}", true);
                        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, $"playerCount is invalid. Missing counts: {string.Join(", ", missingPlayerCounts)}", null);
                    }

                    foreach (var courseCode in teeTime.courseCode)
                    {
                        // 골프장 코스 조회
                        var golfClubCourse = golfClub.Courses.FirstOrDefault(gc => gc.CourseCode == courseCode);
                        if (golfClubCourse == null) //골프장 코스 없을시
                        {
                            Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", "골프장에 코스 코드 없음", true);
                            return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "The golf course does not have a course code", null);
                        }
                    }

                    foreach (var time in teeTime.time)
                    {
                        time.startTime = int.Parse(time.startTime).ToString("D4"); // startTime 4자리로 변환
                        var startTime = time.startTime;
                        // 4자리 숫자인지 확인
                        if (string.IsNullOrEmpty(startTime) || startTime.Length != 4 || !int.TryParse(startTime, out _))
                        {
                            Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", "시작시간 형식 틀림");
                            return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "The startTime value is invalid. It must be a 4-digit string in HHmm format.", null);
                        }

                        // 시간 값 추출
                        var hour = int.Parse(startTime.Substring(0, 2)); // 앞 2자리: 시(hour)
                        var minute = int.Parse(startTime.Substring(2, 2)); // 뒤 2자리: 분(minute)

                        // 유효한 시간 범위 확인
                        if (hour < 0 || hour > 23 || minute < 0 || minute > 59)
                        {
                            Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", "시작 시간이 유효한 범위 벗어남");
                            return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "The startTime value is out of valid range. It must represent a valid time between 0000 and 2359.", null);
                        }
                    }
                }

                // dateApplyType 1,2 유효성
                if (request.dateApplyType != 1 && request.dateApplyType != 2)
                {
                    Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", "dateApplyType 유효하지 않음", true);
                    return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Invalid dateApplyType. Allowed values are 1 or 2", null);
                }

                string dateFormat = "yyyy-MM-dd";

                if (request.dateApplyType == 1) // 날짜적용방법이 1번 일때 시작일과 종료일이 있어야 함
                {
                    // StartPlayDate와 EndPlayDate 필수 확인
                    if (string.IsNullOrEmpty(request.startPlayDate) || string.IsNullOrEmpty(request.endPlayDate))
                    {
                        Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", "시작일 또는 종료일 없음");
                        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Both StartPlayDate and EndPlayDate are required", null);
                    }

                    // StartPlayDate 유효성 검사
                    if (!DateTime.TryParseExact(request.startPlayDate, dateFormat, null, System.Globalization.DateTimeStyles.None, out DateTime startDate))
                    {
                        Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", "시작일 없음");
                        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "StartPlayDate is not in the correct format. Expected format is yyyy-MM-dd", null);
                    }

                    // EndPlayDate 유효성 검사
                    if (!DateTime.TryParseExact(request.endPlayDate, dateFormat, null, System.Globalization.DateTimeStyles.None, out DateTime endDate))
                    {
                        Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", "종료일 없음");
                        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "EndPlayDate is not in the correct format. Expected format is yyyy-MM-dd", null);
                    }

                    // StartPlayDate가 EndPlayDate보다 빠른 날짜인지 확인
                    if (startDate > endDate)
                    {
                        Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", "시작일이 종료일보다 빠른 날짜임");
                        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "StartPlayDate cannot be later than EndPlayDate", null);
                    }

                    // week 유효성 검사 (0 ~ 6 사이에 최소 1개의 값이 있어야 함)
                    if (request.week == null || !request.week.Any() || request.week.Any(w => w < 0 || w > 6))
                    {
                        Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", "week 배열 유효하지 않음");
                        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Week array must contain at least one value between 0 and 6", null);
                    }

                    // week 중복체크
                    if (request.week.Distinct().Count() != request.week.Count())
                    {
                        Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", "week 배열에 중복 값 있음", true);
                        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Week array contains duplicate values", null);
                    }

                    // exceptionDate 체크 후 yyyy-MM-dd 포멧 체크
                    if (request.exceptionDate != null && request.exceptionDate.Any())
                    {
                        foreach (var exceptionDate in request.exceptionDate)
                        {
                            if (!DateTime.TryParseExact(exceptionDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                            {
                                Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", "예외일 없음");
                                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "exceptionDate is not in the correct format. Expected format is yyyy-MM-dd", null);
                            }
                        }
                    }
                }
                else if (request.dateApplyType == 2) // 날짜적용방법이 2번 일때 EffectiveDate이 있어야 함
                {
                    if (request.effectiveDate == null || !request.effectiveDate.Any()) // Assuming EffectiveDate is StartPlayDate
                    {
                        Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", "적용일 없음");
                        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "effectiveDate not found", null);
                    }
                    else
                    {
                        if(request.effectiveDate.Any())
                        {
                            // effectiveDate 날짜 유효성 검사
                            foreach (var effectiveDate in request.effectiveDate)
                            {
                                if (!DateTime.TryParseExact(effectiveDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                                {
                                    Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", "적용일 없음");
                                    return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "effectiveDate is not in the correct format. Expected format is yyyy-MM-dd", null);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessTeeTime", $"유효성 처리 실패 {ex.Message}", true);
                return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
            }

            //if(mode == "POST")
            //{
            //    return await ProcessTeeTime(request, supplierCode, request.golfClubCode);
            //}
            //else
            //{
                Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ValidateTeeTime", "queue 로 진행");
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

                _queue.Enqueue(teeTimeBackgroundRequest);
            //}

            return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "ProcessTeeTime successfully", null);

        }


        public async Task<IDataResult> ProcessTeeTime(TeeTimeRequest request, string supplierCode, string golfClubCode)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            //var stopwatch = Stopwatch.StartNew();

            var response = await strategy.ExecuteAsync(async () =>
            {
                int? pricePolicyId = null;
                int? refundPolicyId = null;
                //try
                //{
                // 공급사 코드로 공급사 ID 조회
                var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.SupplierCode == supplierCode);
                int supplierId = supplier.SupplierId;

                // 골프장 코드로 골프장 정보 조회
                var existingGolfclub = await _context.GolfClubs.FirstOrDefaultAsync(g => g.Supplier != null && g.SupplierId == supplierId && g.GolfClubCode == golfClubCode);
                if (existingGolfclub == null)
                {
                    Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessTeeTime", "골프장 검색 코드 없음");
                    return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "golfclubCode is invalid", null); // 골프장이 유효하지 않을때 처리
                }

                // 1. applicableDates 생성
                IEnumerable<DateTime> applicableDates;

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
                        .Where(date => (request.week.Contains((int)date.DayOfWeek) || request.effectiveDate.Contains(date.ToString("yyyy-MM-dd"))));
                         // DateTime 리스트 유지

                    if(request.exceptionDate != null && request.exceptionDate.Any())
                    {
                        applicableDates = applicableDates.Where(date => !request.exceptionDate.Contains(date.ToString("yyyy-MM-dd")));
                    }

                    applicableDates.ToList();

                    Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessTeeTime", "날짜적용방법 1로 검색");
                }
                else // dateApplyType이 1이 아닌 경우: effectiveDate 리스트의 날짜들 사용
                {
                    applicableDates = request.effectiveDate
                        .Where(date => !string.IsNullOrWhiteSpace(date) && DateTime.TryParseExact(date, "yyyy-MM-dd", null, DateTimeStyles.None, out _))
                        //.Select(date => DateTime.ParseExact(date, "yyyy-MM-dd", null).ToString("yyyyMMdd"))
                        .Select(date => DateTime.ParseExact(date, "yyyy-MM-dd", null))
                        .ToList();
                    Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessTeeTime", "날짜적용방법 2로 검색");
                }

                // 1. 기존 데이터 조회 (요금 정책, 환불 정책, 날짜 슬롯, 시간 슬롯, 코스정보, 티타임 정보 등)
                var existingTeeTimePricePolicies = await _context.TeetimePricePolicies.Where(pp => pp.TeeTimeMappings.Any(tm => tm.TeeTime.SupplierId == supplier.SupplierId)).ToListAsync();
                var existingTeeTimeRefundPolicies = await _context.TeetimeRefundPolicies.Where(pp => pp.TeeTimeMappings.Any(tm => tm.TeeTime.SupplierId == supplier.SupplierId)).ToListAsync();
                var dateSlots = await _context.DateSlots.Where(ds => applicableDates.Contains(ds.StartDate)).ToListAsync();
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
                int batchSize = Math.Min(dateSlotIds.Count, 5000); // 기본적으로 5000 또는 전체 데이터 크기 중 작은 값 사용
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
                var dateSlotMap = dateSlots.ToDictionary(ds => ds.StartDate, ds => ds.DateSlotId); // PlayDate와 DateSlotId의 매핑 딕셔너리 생성
                var timeSlotMap = timeSlots.ToDictionary(ts => ts.StartTime, ts => ts.TimeSlotId); // StartTime과 TimeSlotId의 매핑 딕셔너리 생성

                // 기존 매핑을 ConcurrentDictionary로 생성하여 중복 체크 및 업데이트 가능하게 설정
                var existingTeeTimeMappingsDict = new ConcurrentDictionary<(int TeetimeId, int DateSlotId, int TimeSlotId), OAPI_TeeTimeMapping>(
                    existingTeeTimeMappings.Select(ttm => new KeyValuePair<(int, int, int), OAPI_TeeTimeMapping>((ttm.TeetimeId, ttm.DateSlotId, ttm.TimeSlotId), ttm))
                );

                // 코스 코드 수 만큼 티타임 추가
                foreach (var teeTimeInfo in request.teeTimeInfo)
                {
                    foreach (var courseCode in teeTimeInfo.courseCode)
                    {
                        using (var transaction = await _context.Database.BeginTransactionAsync())
                        {
                            try
                            {
                                // 골프장 코스 조회
                                var golfClubCourse = golfClubCourses.FirstOrDefault(gc => gc.CourseCode == courseCode);
                                if (golfClubCourse == null) //골프장 코스 없을시
                                {
                                    Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessTeeTime", "코스 코드 없음", true);
                                    return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "golfClubCourse is invalid", null);
                                }

                                // 기존 티타임 조회 또는 신규 티타임 추가
                                var teeTime = teeTimes
                                .Where(tt => tt.GolfClubCourseId == golfClubCourse.GolfClubCourseId 
                                && tt.GolfClubId == existingGolfclub.GolfClubId
                                && tt.MinMembers == teeTimeInfo.minMembers 
                                && tt.MaxMembers == teeTimeInfo.maxMembers
                                && tt.IncludeCart == teeTimeInfo.includeCart
                                && tt.IncludeCaddie == teeTimeInfo.includeCaddie
                                && tt.ReservationType == teeTimeInfo.reservationType)
                                .FirstOrDefault();
                                if (teeTime == null)
                                {
                                    // 새로운 티타임 추가
                                    var newTeeTime = new OAPI_TeeTime
                                    {
                                        GolfClubCourseId = golfClubCourse.GolfClubCourseId,
                                        SupplierId = supplierId,
                                        GolfClubId = existingGolfclub.GolfClubId,
                                        MinMembers = (int)teeTimeInfo.minMembers,
                                        MaxMembers = (int)teeTimeInfo.maxMembers,
                                        IncludeCart = teeTimeInfo.includeCart,
                                        IncludeCaddie = teeTimeInfo.includeCaddie,
                                        ReservationType = (int)teeTimeInfo.reservationType,
                                        CreatedDate = DateTime.UtcNow
                                    };
                                    _context.TeeTimes.Add(newTeeTime);
                                    await _context.SaveChangesAsync();
                                    teeTime = newTeeTime;
                                    teeTimes.Add(newTeeTime);
                                }
                                await transaction.CommitAsync();
                            }
                            catch (Exception ex)
                            {
                                await transaction.RollbackAsync();
                                Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, $"ProcessTeeTime", $"티타임 처리 실패 {ex.Message}", true);
                                return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
                            }
                        }

                        // 검색용 딕셔너리 현재 위치에서 만들어야 검색한 티타임 + 추가된 티타임 으로 만들수 있음
                        Dictionary<(int GolfClubCourseId, int MinMembers, int MaxMembers), OAPI_TeeTime> teeTimesDictionary = teeTimes.ToDictionary(t => (t.GolfClubCourseId, t.MinMembers, t.MaxMembers));

                        // 요금 정책 처리 (트랜잭션 사용)
                        using (var transaction = await _context.Database.BeginTransactionAsync())
                        {
                            try
                            {
                                var existingTeeTimePricePolicy = existingTeeTimePricePolicies.FirstOrDefault(p =>
                                    request.teeTimeInfo.First().price.All(price =>
                                    {
                                        var priceDetail = p.PriceDetails.FirstOrDefault(pd => pd.PlayerCount == price.playerCount);
                                        return priceDetail != null &&
                                                priceDetail.GreenFee == price.greenFee &&
                                                priceDetail.CartFee == price.cartFee &&
                                                priceDetail.CaddyFee == price.caddyFee &&
                                                priceDetail.Tax == price.tax &&
                                                priceDetail.AdditionalTax == price.additionalTax &&
                                                priceDetail.UnitPrice == price.unitPrice;
                                    })
                                );

                                if (existingTeeTimePricePolicy != null)
                                {
                                    pricePolicyId = existingTeeTimePricePolicy.PricePolicyId;
                                    Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessTeeTime", "요금 정책 있음");
                                }
                                else
                                {
                                    // 새 정책 추가
                                    var priceDetails = request.teeTimeInfo.First().price.Select(price => new PriceDetail
                                    {
                                        PlayerCount = price.playerCount ?? 0,
                                        GreenFee = price.greenFee,
                                        CartFee = price.cartFee,
                                        CaddyFee = price.caddyFee,
                                        Tax = price.tax,
                                        AdditionalTax = price.additionalTax,
                                        UnitPrice = price.unitPrice
                                    }).ToList();

                                    var newTeeTimePricePolicy = new OAPI_TeetimePricePolicy
                                    {
                                        CreatedDate = DateTime.UtcNow,
                                        PriceDetails = priceDetails
                                    };

                                    _context.TeetimePricePolicies.Add(newTeeTimePricePolicy);
                                    await _context.SaveChangesAsync();
                                    pricePolicyId = newTeeTimePricePolicy.PricePolicyId;

                                    existingTeeTimePricePolicies.Add(newTeeTimePricePolicy);
                                    Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessTeeTime", "요금 정책 없어서 추가");
                                }

                                await transaction.CommitAsync();
                            }
                            catch (Exception ex)
                            {
                                await transaction.RollbackAsync();
                                Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessTeeTime", $"티타임 처리 실패 {ex.Message}", true);
                                return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
                            }
                        }

                        using (var transaction = await _context.Database.BeginTransactionAsync())
                        {
                            try
                            {
                                var sortedRefundPolicies = teeTimeInfo.refundPolicy.OrderByDescending(rp => rp.refundDate).ToList();
                                var existingRefundPolicy = existingTeeTimeRefundPolicies.FirstOrDefault(rp =>
                                    rp.RefundDetails.Count == sortedRefundPolicies.Count &&
                                    sortedRefundPolicies.All(refund =>
                                    {
                                        var refundDetail = rp.RefundDetails.FirstOrDefault(rd => rd.RefundDate == refund.refundDate);
                                        return refundDetail != null &&
                                                refundDetail.RefundFee == refund.refundFee &&
                                                refundDetail.RefundUnit == refund.refundUnit;
                                    })
                                );

                                if (existingRefundPolicy != null)
                                {
                                    refundPolicyId = existingRefundPolicy.RefundPolicyId;
                                    Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessTeeTime", "환불 정책 있음");
                                }
                                else
                                {
                                    if (teeTimeInfo.refundPolicy != null && teeTimeInfo.refundPolicy.Any())
                                    {
                                        var refundDetails = teeTimeInfo.refundPolicy.Select(refund => new RefundDetail
                                        {
                                            RefundDate = refund.refundDate,
                                            RefundHour = refund.refundHour,
                                            RefundFee = refund.refundFee,
                                            RefundUnit = refund.refundUnit
                                        }).ToList();

                                        var newRefundPolicy = new OAPI_TeetimeRefundPolicy
                                        {
                                            CreatedDate = DateTime.UtcNow,
                                            RefundDetails = refundDetails,
                                        };

                                        _context.TeetimeRefundPolicies.Add(newRefundPolicy);
                                        await _context.SaveChangesAsync();
                                        refundPolicyId = newRefundPolicy.RefundPolicyId;

                                        // 새로운 정책 추가 후 리스트에 포함
                                        existingTeeTimeRefundPolicies.Add(newRefundPolicy);
                                        Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessTeeTime", "환불 정책 없어서 추가");
                                    }
                                }
                                await transaction.CommitAsync();
                            }
                            catch (Exception ex)
                            {
                                Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessTeeTime", $"티타임 처리 실패 {ex.Message}", true);
                                return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
                            }
                        };


                        using (var transaction = await _context.Database.BeginTransactionAsync())
                        {
                            try
                            {

                                // 병렬로 티타임 매핑 생성
                                var teeTimeMappings = new ConcurrentBag<OAPI_TeeTimeMapping>();
                                //Debug.WriteLine($"[Step 3] Execution Time: {stopwatch.ElapsedMilliseconds} ms");

                                // 최적화된 파티션 크기 설정
                                int partitionSize = Math.Max(1, applicableDates.Count() / Environment.ProcessorCount);

                                // Partitioner.Create를 사용하여 고정 크기 파티션 생성
                                var partitioner = Partitioner.Create(applicableDates.ToList(), EnumerablePartitionerOptions.NoBuffering);

                                // 병렬 작업을 통해 티타임 매핑 생성
                                Parallel.ForEach(partitioner, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, date =>
                                {
                                    if (!dateSlotMap.TryGetValue(date, out var dateSlotId))
                                        return;

                                    foreach (var time in teeTimeInfo.time)
                                    {
                                        if (!timeSlotMap.TryGetValue(time.startTime, out var timeSlotId))
                                            continue;

                                        var minMembers = (int)teeTimeInfo.minMembers;
                                        var maxMembers = (int)teeTimeInfo.maxMembers;

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

                                            // 기존 매핑이 있는지 확인 후 업데이트 또는 추가
                                            existingTeeTimeMappingsDict.AddOrUpdate(mappingKey,
                                                // 새로운 항목 추가
                                                key =>
                                                {
                                                    var newMapping = new OAPI_TeeTimeMapping
                                                    {
                                                        TeetimeId = teetimeId,
                                                        DateSlotId = dateSlotId,
                                                        TimeSlotId = timeSlotId,
                                                        PricePolicyId = (int)pricePolicyId,
                                                        RefundPolicyId = refundPolicyId,
                                                        SupplierTeetimeCode = code,
                                                        IsAvailable = true,
                                                        IsDisable = true,
                                                        IsDeleted = false,
                                                        CreatedDate = DateTime.UtcNow
                                                    };

                                                    teeTimeMappings.Add(newMapping);  // 새로 추가된 항목을 리스트에 포함
                                                    return newMapping;
                                                },
                                                // 기존 항목 업데이트
                                                (key, existingTeeTimeMapping) =>
                                                {
                                                    bool needsUpdate = existingTeeTimeMapping.TeetimeId != teetimeId ||
                                                                        existingTeeTimeMapping.PricePolicyId != pricePolicyId ||
                                                                        existingTeeTimeMapping.RefundPolicyId != refundPolicyId ||
                                                                        !string.Equals(existingTeeTimeMapping.SupplierTeetimeCode, code);

                                                    if (needsUpdate)
                                                    {
                                                        existingTeeTimeMapping.TeetimeId = teetimeId;
                                                        existingTeeTimeMapping.PricePolicyId = (int)pricePolicyId;
                                                        existingTeeTimeMapping.RefundPolicyId = refundPolicyId;
                                                        existingTeeTimeMapping.SupplierTeetimeCode = code;
                                                        existingTeeTimeMapping.UpdatedDate = DateTime.UtcNow;

                                                        teeTimeMappings.Add(existingTeeTimeMapping); // 업데이트된 항목을 리스트에 포함
                                                    }

                                                    return existingTeeTimeMapping;
                                                });
                                        }
                                    }
                                });
 
                                var teeTimeMappingsList = teeTimeMappings.ToList();
                                if (teeTimeMappingsList.Any())
                                {
                                    Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessTeeTime", "티타임 저장 시작");
                                    await _context.BulkInsertOrUpdateAsync(teeTimeMappingsList);
                                }
                                await transaction.CommitAsync();
                                Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessTeeTime", "티타임 처리 완료");
                                //Debug.WriteLine($"[Step 4] Execution Time: {stopwatch.ElapsedMilliseconds} ms");
                                //stopwatch.Stop();
                            }
                            catch (Exception ex)
                            {
                                Utils.UtilLogs.LogRegHour(supplierCode, golfClubCode, "ProcessTeeTime", $"티타임 처리 실패 {ex.Message}", true);
                                return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
                            }

                        }
                    }
                }
                return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "ProcessTeeTime successfully", null);
            });
            return response;
        } // end ProcessTeeTime
    }
}

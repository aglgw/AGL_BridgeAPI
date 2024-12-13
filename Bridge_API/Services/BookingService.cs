using AGL.Api.Bridge_API.Interfaces;
using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using AGL.Api.Domain.Entities.OAPI;
using AGL.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using static AGL.Api.Bridge_API.Models.OAPI.OAPI;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIResponse;
using System.Globalization;
using AGL.Api.ApplicationCore.Helpers;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIRequest;
using AGL.Api.ApplicationCore.Utilities;
using StackExchange.Redis;
using Azure.Core;
using Azure;
using System.Diagnostics;

namespace AGL.Api.Bridge_API.Services
{
    public class BookingService : BaseService,IBookingService
    {
        private readonly OAPI_DbContext _context;
        private IConfiguration _configuration { get; }
        private readonly ICommonService _commonService;
        private readonly IRedisService _redisService;

        public BookingService(OAPI_DbContext context,
            ICommonService commonService,
            IConfiguration configuration,
            IRedisService redisService)
        {
            _context = context;
            _configuration = configuration;
            _commonService = commonService;
            _redisService = redisService;
        }

        /// <summary>
        /// 예약 요청 조회
        /// </summary>
        /// <param name="request"></param>
        /// <param name="supplierCode"></param>
        /// <returns></returns>
        public async Task<OAPIDataResponse<List<ReservationReponse>>> GetBookingRequest(ReservationListRequest request, string supplierCode)
        {
            return await GetBookingList(request, supplierCode, StatusCode.REQUEST);
        }

        /// <summary>
        /// 취소 요청 조회
        /// </summary>
        /// <param name="request"></param>
        /// <param name="supplierCode"></param>
        /// <returns></returns>
        public async Task<OAPIDataResponse<List<ReservationReponse>>> GetBookingCancellations(ReservationListRequest request, string supplierCode)
        {
            return await GetBookingList(request, supplierCode, StatusCode.CANCELLATIONREQUEST);
        }

        private async Task<OAPIDataResponse<List<ReservationReponse>>> GetBookingList(ReservationListRequest request, string supplierCode, StatusCode statusCode)
        {
            try
            {
                var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.SupplierCode == supplierCode);
                if (supplier == null)
                {
                    Utils.UtilLogs.LogRegHour(supplierCode, "BookingList", "BookingList", "공급사 검색 안됨");
                    return await _commonService.CreateResponse<List<ReservationReponse>>(false, ResultCode.INVALID_INPUT, "Supplier not found", null);
                }

                var existingReservationManagementsQuery = _context.ReservationManagements.Include(r => r.Guests).Where(r => r.SupplierId == supplier.SupplierId);

                if(!string.IsNullOrEmpty(request.reservationDate))
                {
                    string reservationDate = string.Empty;
                    if (DateTime.TryParseExact(
                        request.reservationDate,
                        new[] { "yyyy-MM-dd", "yyyyMMdd" }, // 허용되는 형식 배열
                        null,
                        System.Globalization.DateTimeStyles.None,
                        out DateTime parsedDate))
                    {
                        reservationDate = parsedDate.ToString("yyyyMMdd");
                    }
                    else
                    {
                        return await _commonService.CreateResponse<List<ReservationReponse>>(false, ResultCode.INVALID_INPUT, "Invalid date format. Expected format: yyyy-MM-dd.", null);
                    }
                    existingReservationManagementsQuery = existingReservationManagementsQuery.Where(r => r.ReservationDate == reservationDate);
                }

                if(!string.IsNullOrEmpty(request.reservationId))
                {
                    existingReservationManagementsQuery = existingReservationManagementsQuery.Where(r => r.ReservationId == request.reservationId);
                }

                if(request.status != null)
                {
                    existingReservationManagementsQuery = existingReservationManagementsQuery.Where(r => r.ReservationStatus == request.status);
                }
                else
                {
                    existingReservationManagementsQuery = existingReservationManagementsQuery.Where(r => r.ReservationStatus == (byte)statusCode);
                }

                var existingReservationManagements = await existingReservationManagementsQuery.ToListAsync();

                var reservationDtos = existingReservationManagements.Select(reservation => new ReservationReponse
                {
                    reservationId = reservation.ReservationId,
                    golfClubCode = reservation.GolfClubCode,
                    courseCode = reservation.CourseCode,
                    reservationDate = reservation.ReservationDate,
                    reservationStartTime = reservation.ReservationStartTime,
                    reservationMembers = reservation.ReservationMembers,
                    currency = reservation.Currency,
                    totalPrice = reservation.TotalPrice,
                    holderName = reservation.HolderName,
                    reservationPhone = reservation.ReservationPhone,
                    reservationEmail = reservation.ReservationEmail,
                    reservationCountry = reservation.ReservationCountry,
                    guestInfo = reservation.Guests?.Select(guest => new GuestInfo
                    {
                        guestName = guest.GuestName,
                        guestCountry = guest.GuestCountry,
                        guestGender = guest.GuestGender,
                        guestPhone = guest.GuestPhone,
                    }).ToList() ?? new List<GuestInfo>(),
                }).ToList();
                Utils.UtilLogs.LogRegHour(supplierCode, "BookingList", $"BookingList", $"티타임 정보 검색 성공");
                return await _commonService.CreateResponse(true, ResultCode.SUCCESS, "Booking List successfully", reservationDtos);
            }
            catch (Exception ex)
            {
                Utils.UtilLogs.LogRegHour(supplierCode, "", "Booking", "예약요청 저장 실패", true);
                return await _commonService.CreateResponse<List<ReservationReponse>>(false, ResultCode.SERVER_ERROR, ex.Message, null);
            }
        }


        /// <summary>
        /// 예약 확정
        /// </summary>
        /// <param name="request"></param>
        /// <param name="supplierCode"></param>
        /// <returns></returns>
        public async Task<IDataResult> PostBookingConfirm(ReservationRequest request, string supplierCode)
        {
            var reservationId = request.reservationId;

            var RedisStrKey = $"PBCF:{supplierCode}:{reservationId}";

            try
            {
                if (await _redisService.KeyExistsAsync(RedisStrKey)) // Redis 키 조회 (비동기)
                {
                    Utils.UtilLogs.LogRegHour(supplierCode, "Confirm", "Confirm", $"예약확정 중복");
                    return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Duplicate request", null);
                }
                else
                {
                    await _redisService.SetValueAsync(RedisStrKey, "", TimeSpan.FromMinutes(1)); // 비동기로 Redis 키 설정
                }
            }
            catch (RedisException ex)
            {
                Utils.UtilLogs.LogRegDay(supplierCode, "Confirm", "Confirm", $"예약확정 Redis 실패 {ex.Message}", true);
                return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
            }

            // 입력 값 검증 - reservationId와 supplierCode가 비어 있는지 확인
            if (string.IsNullOrEmpty(reservationId) || string.IsNullOrEmpty(supplierCode))
            {
                Utils.UtilLogs.LogRegHour(supplierCode, "Confirm", "Confirm", $"예약번호 또는 공급사 코드 없음 {reservationId} : {supplierCode}");
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "reservationId or supplierCode is invalid", null);
            }
            // 공급자 정보 조회 - supplierCode에 해당하는 공급자를 데이터베이스에서 검색
            var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.SupplierCode == supplierCode);
            if (supplier == null)
            {
                Utils.UtilLogs.LogRegHour(supplierCode, "Confirm", "Confirm", "공급사 검색 안됨");
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Supplier not found", null);
            }
            int supplierId = supplier.SupplierId;

            // 예약관리 DB에서 예약 조회 - 예약 번호, 상태, 공급자 ID를 기준으로 예약 검색
            var reservation = await _context.ReservationManagements.FirstOrDefaultAsync(r => r.ReservationId == reservationId && r.ReservationStatus == 1 && r.SupplierId == supplierId);
            if (reservation == null)
            {
                Utils.UtilLogs.LogRegHour(supplierCode, "Confirm", "Confirm", "예약관리에서 검색 안됨");
                return await _commonService.CreateResponse<object>(false, ResultCode.NOT_FOUND, "Reservation not found", null);
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        reservation.ReservationStatus = (byte)StatusCode.CONFIRMATION; // 1 예약요청 2 예약확정 3 예약취소
                        reservation.UpdatedDate = DateTime.UtcNow;

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        Utils.UtilLogs.LogRegHour(supplierCode, "Confirm", "Confirm", "예약관리에 예약확정 변경 완료");

                        return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "Reservation confirmed successfully", null);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Utils.UtilLogs.LogRegDay(supplierCode, "Confirm", "Confirm", $"예약확정 실패 {ex.Message}", true);
                        return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
                    }
                }
            });
        }

        /// <summary>
        /// 예약 취소
        /// </summary>
        /// <param name="request"></param>
        /// <param name="supplierCode"></param>
        /// <returns></returns>
        public async Task<IDataResult> PostBookingCancel(cancelRequest request, string supplierCode)
        {
            var reservationId = request.reservationId;

            var RedisStrKey = $"PBC:{supplierCode}:{reservationId}";

            try
            {
                if (await _redisService.KeyExistsAsync(RedisStrKey)) // Redis 키 조회 (비동기)
                {
                    Utils.UtilLogs.LogRegHour(supplierCode, "Cancel", "Cancel", $"예약취소 중복");
                    return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Duplicate request", null);
                }
                else
                {
                    await _redisService.SetValueAsync(RedisStrKey, "", TimeSpan.FromMinutes(1)); // 비동기로 Redis 키 설정
                }
            }
            catch (RedisException ex)
            {
                Utils.UtilLogs.LogRegDay(supplierCode, "Cancel", "Cancel", $"예약취소 Redis 실패 {ex.Message}", true);
                return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
            }

            // 입력 값 검증 - reservationId와 supplierCode가 비어 있는지 확인
            if (string.IsNullOrEmpty(reservationId) || string.IsNullOrEmpty(supplierCode))
            {
                Utils.UtilLogs.LogRegHour(supplierCode, "Cancel", "Cancel", $"예약번호 또는 공급사 코드 없음 {reservationId} : {supplierCode}");
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "reservationId or supplierCode is invalid", null);
            }
            // 공급자 정보 조회 - supplierCode에 해당하는 공급자를 데이터베이스에서 검색
            var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.SupplierCode == supplierCode);
            if (supplier == null)
            {
                Utils.UtilLogs.LogRegHour(supplierCode, "Cancel", "Cancel", "공급사 검색 안됨");
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Supplier not found", null);
            }
            int supplierId = supplier.SupplierId;

            // 예약관리 DB에서 예약 조회 - 예약 번호, 상태, 공급자 ID를 기준으로 예약 검색
            var reservation = await _context.ReservationManagements.FirstOrDefaultAsync(r => r.ReservationId == reservationId && r.ReservationStatus == 4 && r.SupplierId == supplierId);
            if (reservation == null)
            {
                Utils.UtilLogs.LogRegHour(supplierCode, "Cancel", "Cancel", "예약관리에서 검색 안됨");
                return await _commonService.CreateResponse<object>(false, ResultCode.NOT_FOUND, "Reservation not found", null);
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        reservation.ReservationStatus = (byte)StatusCode.CANCELLATION;
                        reservation.UpdatedDate = DateTime.UtcNow;
                        reservation.cancelDate = request?.cancelDate;
                        reservation.cancelPenaltyAmount = request?.cancelPenaltyAmount;
                        reservation.cancelCurrency = request?.currency;

                        await _context.SaveChangesAsync();

                        // 예약 취소 시 티타임 재고 정리
                        var teeTimeMapping = await _context.TeeTimeMappings
                            .Where(ttm => ttm.TeeTime.SupplierId == supplier.SupplierId
                            && ttm.TeeTime.GolfClub.GolfClubCode == reservation.GolfClubCode
                            && ttm.TeeTime.GolfClubCourse.CourseCode == reservation.CourseCode
                            && ttm.DateSlot.PlayDate == reservation.ReservationDate
                            && ttm.TimeSlot.StartTime == reservation.ReservationStartTime)
                            .FirstOrDefaultAsync();

                        if (teeTimeMapping != null)
                        {
                            teeTimeMapping.IsAvailable = false;
                            teeTimeMapping.UpdatedDate = DateTime.UtcNow;

                            await _context.SaveChangesAsync();
                        }

                        await transaction.CommitAsync();
                        Utils.UtilLogs.LogRegHour(supplierCode, "Cancel", "Cancel", "예약관리에 예약취소 완료");

                        return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "Reservation canceled successfully", null);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Utils.UtilLogs.LogRegDay(supplierCode, "Cancel", "Cancel", $"예약취소 실패 {ex.Message}", true);
                        return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
                    }
                }
            });
        }

        /*
                /// <summary>
                /// 예약 조회
                /// </summary>
                /// <param name="Req"></param>
                /// <returns></returns>
                public async Task<IDataResult> GetBookingInquiry(ReqBookingInquiry Req)
                {
                    var inboundCode = Req.inboundCode;
                    if (string.IsNullOrWhiteSpace(inboundCode))
                    {
                        Utils.UtilLogs.LogRegHour(inboundCode, inboundCode, "Booking", "inboundCode 키 없음");
                        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "daemonId or supplierCode not found", null);
                    }

                    var supplier = await _context.Suppliers.Include(s => s.Authentication).FirstOrDefaultAsync(s => s.Authentication.Deleted == false && s.GolfClubs.Any(g => g.InboundCode == inboundCode));

                    if (supplier == null)
                    {
                        Utils.UtilLogs.LogRegHour(inboundCode, inboundCode, $"Booking", $"예약 요청 공급사 누락");
                        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Supplier not found", null);
                    }

                    var splitCode = inboundCode.Split("_");

                    var golfClubCode = splitCode.Last();
                    var supplierCode = splitCode.First();

                    string dateFormat = "yyyy-MM-dd";

                    // StartPlayDate 유효성 검사
                    if (!DateTime.TryParseExact(Req.startDate, dateFormat, null, System.Globalization.DateTimeStyles.None, out DateTime startDate))
                    {
                        Utils.UtilLogs.LogRegHour(inboundCode, inboundCode, "Booking", "시작일 없음");
                        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "startDate is not in the correct format. Expected format is yyyy-MM-dd", null);
                    }

                    // EndPlayDate 유효성 검사
                    if (!DateTime.TryParseExact(Req.endDate, dateFormat, null, System.Globalization.DateTimeStyles.None, out DateTime endDate))
                    {
                        Utils.UtilLogs.LogRegHour(inboundCode, inboundCode, "Booking", "종료일 없음");
                        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "endDate is not in the correct format. Expected format is yyyy-MM-dd", null);
                    }

                    // 인증 정보 설정
                    var authentication = supplier.Authentication;
                    string clientCode = authentication.AglCode;
                    string token = GenerateSHA256Hash(authentication.TokenAgl);

                    // 엔드포인트 설정
                    string EndpointUrl = $"{supplier.EndPoint.TrimEnd('/')}/reservation/list";

                    // HTTP 요청 설정
                    var header = new Dictionary<string, string>
                    {
                        { "X-Client-Code", clientCode },
                        { "Authorization", $"Bearer {token}" },
                    };

                    var query = new Dictionary<string, string>
                    {
                        { "golfClubCode", golfClubCode },
                        { "startDate", Req.startDate },
                        { "endDate", Req.endDate },
                        { "reservationId", Req.reservationId },
                        //{ "status", Req.status },
                    };

                    var response = new OAPIReservationConfirmListResponse();
                    string strReaponse = string.Empty;

                    try
                    {
                        await RestfulClient.GETAPIHeaderQuery<OAPIReservationConfirmListResponse>(EndpointUrl, header, query, (status, reasonPhrase, apiResponse) =>
                        {
                            if (status == System.Net.HttpStatusCode.OK)
                            {
                                string jsonString = System.Text.Json.JsonSerializer.Serialize(apiResponse);
                                response = apiResponse;

                            }
                            else
                            {
                                strReaponse = reasonPhrase;
                                Utils.UtilLogs.LogRegHour(inboundCode, inboundCode, $"Booking", $"확정 목록 조회 실패 statusCode : {status} : {strReaponse} ", true);
                            }
                        });

                        if (response != null)
                        {
                            if (response.data.Any())
                            {
                                Utils.UtilLogs.LogRegHour(inboundCode, inboundCode, $"Booking", $"확정 목록 조회 성공");
                                return await _commonService.CreateResponse<object>(true, ResultCode.NOT_FOUND, "Confirmation Reservation was fail", response.data);
                            }
                            else
                            {
                                Utils.UtilLogs.LogRegHour(inboundCode, inboundCode, $"Booking", $"확정 목록 내용 없음");
                                return await _commonService.CreateResponse<object>(true, ResultCode.NOT_FOUND, "Failed to retrieve confirmation reservation details.", response.data);
                            }
                        }
                        else
                        {
                            Utils.UtilLogs.LogRegDay(inboundCode, inboundCode, "Booking", "확정 목록 정보 없음", true);
                            return await _commonService.CreateResponse<object>(true, ResultCode.NOT_FOUND, "Confirmation Reservation was fail", null);
                        }

                    }
                    catch (Exception ex)
                    {
                        Utils.UtilLogs.LogRegHour(inboundCode, inboundCode, "Booking", "예약요청 저장 실패", true);
                        return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
                    }

                }

                /// <summary>
                /// 확정된 예약 조회
                /// </summary>
                /// <param name="reservationId"></param>
                /// <returns></returns>
                public async Task<IDataResult> GetConfirmBookingInquiry(string reservationId, string inboundCode)
                {

                    if (string.IsNullOrWhiteSpace(inboundCode))
                    {
                        Utils.UtilLogs.LogRegHour(inboundCode, inboundCode, "Booking", "inboundCode 키 없음");
                        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "daemonId or supplierCode not found", null);
                    }

                    var supplier = await _context.Suppliers.Include(s => s.Authentication).FirstOrDefaultAsync(s => s.Authentication.Deleted == false && s.GolfClubs.Any(g => g.InboundCode == inboundCode));

                    if (supplier == null)
                    {
                        Utils.UtilLogs.LogRegHour(inboundCode, inboundCode, $"Booking", $"예약 요청 공급사 누락");
                        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Supplier not found", null);
                    }

                    // 인증 정보 설정
                    var authentication = supplier.Authentication;
                    string clientCode = authentication.AglCode;
                    string token = GenerateSHA256Hash(authentication.TokenAgl);

                    // 엔드포인트 설정
                    string EndpointUrl = $"{supplier.EndPoint.TrimEnd('/')}/reservation";

                    // HTTP 요청 설정
                    var header = new Dictionary<string, string>
                    {
                        { "X-Client-Code", clientCode },
                        { "Authorization", $"Bearer {token}" },
                    };

                    var query = new Dictionary<string, string>
                    {
                        { "reservationId", reservationId },
                    };

                    var response = new OAPIReservationConfirmListResponse();
                    string strReaponse = string.Empty;

                    try
                    {
                        await RestfulClient.GETAPIHeaderQuery<OAPIReservationConfirmListResponse>(EndpointUrl, header, query, (status, reasonPhrase, apiResponse) =>
                        {
                            if (status == System.Net.HttpStatusCode.OK)
                            {
                                string jsonString = System.Text.Json.JsonSerializer.Serialize(apiResponse);
                                response = apiResponse;

                            }
                            else
                            {
                                strReaponse = reasonPhrase;
                                Utils.UtilLogs.LogRegHour(inboundCode, inboundCode, $"Booking", $"확정 목록 조회 실패 statusCode : {status} : {strReaponse} ", true);
                            }
                        });

                        if (response != null)
                        {
                            if (response.data.Any())
                            {
                                Utils.UtilLogs.LogRegHour(inboundCode, inboundCode, $"Booking", $"확정 목록 조회 성공");
                                return await _commonService.CreateResponse<object>(true, ResultCode.NOT_FOUND, "Confirmation Reservation was fail", response.data);
                            }
                            else
                            {
                                Utils.UtilLogs.LogRegHour(inboundCode, inboundCode, $"Booking", $"확정 목록 내용 없음");
                                return await _commonService.CreateResponse<object>(true, ResultCode.NOT_FOUND, "Failed to retrieve confirmation reservation details.", response.data);
                            }
                        }
                        else
                        {
                            Utils.UtilLogs.LogRegDay(inboundCode, inboundCode, "Booking", "확정 목록 정보 없음", true);
                            return await _commonService.CreateResponse<object>(true, ResultCode.NOT_FOUND, "Confirmation Reservation was fail", null);
                        }

                    }
                    catch (Exception ex)
                    {
                        Utils.UtilLogs.LogRegHour(inboundCode, inboundCode, "Booking", "예약요청 저장 실패", true);
                        return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
                    }

                    return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "Confirmation Reservation inquiry was successfully", null);

                }

                public async Task<IDataResult> Test()
                {

                    var rst = new BookingInfo
                    {
                        reservationId = "",
                        golfClubCode = "",
                        playDate = "",
                        startTime = "",
                        playerCount = 0,
                        status = 0,
                        orderDate = "",
                        currency = "s",
                        cancelPenaltyAmount = Convert.ToDecimal(1)
                    };



                    return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "Reservation confirmed successfully", rst);
                }

                private string GenerateSHA256Hash(string input)
                {
                    using (var sha256 = SHA256.Create())
                    {
                        var bytes = Encoding.UTF8.GetBytes(input);
                        var hash = sha256.ComputeHash(bytes);
                        return BitConverter.ToString(hash).Replace("-", "").ToLower();
                    }
                }
        */

        /// <summary>
        /// 테스트 예약 요청
        /// </summary>
        /// <param name="request"></param>
        /// <param name="supplierCode"></param>
        /// <returns></returns>
        public async Task<OAPIDataResponse<ReservationReponse>> PostTestBookingRequest(string supplierCode)
        {
            try
            {
                var supplier = await _context.Suppliers.Where(s => s.SupplierCode == supplierCode).FirstOrDefaultAsync();
                if (supplier == null)
                {
                    Utils.UtilLogs.LogRegHour(supplierCode, "BookingList", "BookingList", "공급사 검색 안됨");
                    return await _commonService.CreateResponse<List<ReservationReponse>>(false, ResultCode.INVALID_INPUT, "Supplier not found", null);
                }

                var golfClub = await _context.GolfClubs
                    .Where(s => s.SupplierId == supplier.SupplierId)
                        .Select(g => new
                        {
                            GolfClub = g,
                            RandomCourse = g.Courses.OrderBy(c => Guid.NewGuid()).FirstOrDefault(),
                            RandomTeeTime = g.Courses.SelectMany(c => c.TeeTimes).OrderBy(t => Guid.NewGuid()).FirstOrDefault(),
                            RandomMapping = g.Courses.SelectMany(c => c.TeeTimes)
                            .SelectMany(t => t.TeeTimeMappings)
                            .OrderBy(m => Guid.NewGuid())
                                .Select(tm => new
                                {
                                    TeeTimeMapping = tm,
                                    DateSlot = tm.DateSlot,
                                    TimeSlot = tm.TimeSlot,
                                    Price = tm.TeetimePricePolicy
                                })
                            .FirstOrDefault()
                        })
                    .OrderBy(r => Guid.NewGuid())
                    .FirstOrDefaultAsync();

                var GolfClubCode = golfClub?.GolfClub?.GolfClubCode ?? Util.GenerateRandomString(5) + Util.GenerateRandomNumber(3);
                var CourseCode = golfClub?.RandomCourse?.CourseCode ?? Util.GenerateRandomString(5) + Util.GenerateRandomNumber(3);
                var ReservationDate = golfClub?.RandomMapping?.DateSlot?.PlayDate ?? Util.GenerateRandomDate().ToString("yyyyMMdd");
                var ReservationStartTime = golfClub?.RandomMapping?.TimeSlot?.StartTime ?? Util.GenerateRandomTime().ToString("hhmm");
                var TotalPrice = golfClub?.RandomMapping?.Price?.UnitPrice_4 * 4 ?? Math.Floor(Util.GenerateRandomAmount());
                var PricePolicyId = golfClub?.RandomMapping?.Price?.PricePolicyId ?? null;

                // 예약 요청 시 티타임 재고 정리
                if (PricePolicyId != null)
                {
                    var teeTimeMapping = await _context.TeeTimeMappings
                        .Where(ttm => ttm.TeeTime.SupplierId == supplier.SupplierId
                        && ttm.TeeTime.GolfClub.GolfClubCode == GolfClubCode
                        && ttm.TeeTime.GolfClubCourse.CourseCode == CourseCode
                        && ttm.DateSlot.PlayDate == ReservationDate
                        && ttm.TimeSlot.StartTime == ReservationStartTime)
                        .FirstOrDefaultAsync();

                    if (teeTimeMapping != null)
                    {
                        teeTimeMapping.IsAvailable = false;
                        teeTimeMapping.UpdatedDate = DateTime.UtcNow;

                        await _context.SaveChangesAsync();
                    }
                }

                // 예약 저장
                var testReservation = new OAPI_ReservationManagement
                {
                    SupplierId = supplier.SupplierId,
                    ReservationId = "",
                    ReservationStatus = (byte)StatusCode.REQUEST,
                    GolfClubCode = GolfClubCode,
                    CourseCode = CourseCode,
                    ReservationDate = ReservationDate,
                    ReservationStartTime = ReservationStartTime,
                    ReservationMembers = 4,
                    Currency = "USD",
                    TotalPrice = TotalPrice,
                    HolderName = Util.GenerateRandomString(4) + " " + Util.GenerateRandomString(5),
                    ReservationPhone = Util.GenerateRandomNumber(3) + "-" + Util.GenerateRandomNumber(3) + "-" + Util.GenerateRandomNumber(3),
                    ReservationEmail = Util.GenerateRandomString(5) + "@" + Util.GenerateRandomString(6) + ".com",
                    ReservationCountry = "US",
                    CreatedDate = DateTime.UtcNow
                };
                _context.ReservationManagements.Add(testReservation);
                await _context.SaveChangesAsync();

                testReservation.ReservationId = ComputeSha256.ComputeSha256Hash(testReservation.ReservationManagementId.ToString());
                await _context.SaveChangesAsync();

                // 예약 이용객 랜덤 저장
                var randomGuests = Enumerable.Range(1, 4).Select(index => new OAPI_ReservationmanagementGuest
                {
                    ReservationManagementId = testReservation.ReservationManagementId,
                    Idx = (byte)index,
                    GuestName = Util.GenerateRandomString(4) + " " + Util.GenerateRandomString(5), // 랜덤 이름
                    GuestPhone = Util.GenerateRandomNumber(3) + "-" + Util.GenerateRandomNumber(3) + "-" + Util.GenerateRandomNumber(3), // 랜덤 전화번호
                    GuestGender = index % 2 == 0 ? "M" : "F", // 성별 랜덤 배치
                    GuestCountry = Util.GenerateRandomString(2) // 랜덤 국가코드
                }).ToList();

                if (randomGuests.Any())
                {
                    _context.ReservationManagementGuests.AddRange(randomGuests);
                    await _context.SaveChangesAsync();
                }

                var reservationManagement = await _context.ReservationManagements.Include(r => r.Guests).Where(r => r.SupplierId == supplier.SupplierId && r.ReservationManagementId == testReservation.ReservationManagementId).FirstOrDefaultAsync();

                var reservationDtos =  new ReservationReponse
                {
                    reservationId = reservationManagement.ReservationId,
                    golfClubCode = reservationManagement.GolfClubCode,
                    courseCode = reservationManagement.CourseCode,
                    reservationDate = reservationManagement.ReservationDate,
                    reservationStartTime = reservationManagement.ReservationStartTime,
                    reservationMembers = reservationManagement.ReservationMembers,
                    currency = reservationManagement.Currency,
                    totalPrice = reservationManagement.TotalPrice,
                    holderName = reservationManagement.HolderName,
                    reservationPhone = reservationManagement.ReservationPhone,
                    reservationEmail = reservationManagement.ReservationEmail,
                    reservationCountry = reservationManagement.ReservationCountry,
                    guestInfo = reservationManagement.Guests?.Select(guest => new GuestInfo
                    {
                        guestName = guest.GuestName,
                        guestCountry = guest.GuestCountry,
                        guestGender = guest.GuestGender,
                        guestPhone = guest.GuestPhone,
                    }).ToList() ?? new List<GuestInfo>(),
                };

                Utils.UtilLogs.LogRegHour(supplierCode, "TestBooking", $"TestBooking", $"예약요청 생성 테스트 성공");
                return await _commonService.CreateResponse<ReservationReponse>(true, ResultCode.SUCCESS, "Reservation request creation test succeeded", reservationDtos);
            }
            catch (Exception ex)
            {
                Utils.UtilLogs.LogRegHour(supplierCode, "TestBooking", "TestBooking", "예약요청 생성 테스트 실패", true);
                return await _commonService.CreateResponse<ReservationReponse>(false, ResultCode.SERVER_ERROR, ex.Message, null);
            }
        }

    }
}

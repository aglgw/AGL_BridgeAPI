using AGL.Api.Bridge_API.Interfaces;
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
using AGL.Api.ApplicationCore.Helpers;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIRequest;
using AGL.Api.ApplicationCore.Utilities;
using System.Globalization;

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

        /// <summary>
        /// 예약 요청
        /// </summary>
        /// <param name="Req"></param>
        /// <returns></returns>
        public async Task<IDataResult> POSTInboundBookingRequest(ReqBookingRequest Req)
        {
            var inboundCode = Req.inboundCode;
            if (string.IsNullOrWhiteSpace(inboundCode))
            {
                Utils.UtilLogs.LogRegDay(inboundCode, inboundCode, "Booking", "inboundCode 키 없음");
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "inboundCode not found", null);
            }

            var supplier = await _context.Suppliers.Include(s => s.Authentication).FirstOrDefaultAsync(s => s.Authentication.Deleted == false && s.GolfClubs.Any(g => g.InboundCode == inboundCode));

            if (supplier == null) // 공급사 유효성 검사
            {
                Utils.UtilLogs.LogRegDay(inboundCode, Req.golfClubCode, $"Booking", $"예약 요청 공급사 누락");
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Supplier not found", null);
            }

            if (!DateTime.TryParseExact(Req.reservationDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedReservationDate))  // 시작 날짜 유효성 검사
            {
                Utils.UtilLogs.LogRegDay(supplier.SupplierCode, Req.golfClubCode, $"Booking", $"reservationDate 포멧 오류");
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Invalid reservationDate format. Expected yyyy-MM-dd", null);
            }
            Req.reservationDate = parsedReservationDate.ToString("yyyyMMdd");

            if (!TimeSpan.TryParseExact(Req.reservationStartTime, "hhmm", CultureInfo.InvariantCulture, out TimeSpan parsedStartTime)) // 시작 시간 유효성 검사
            {
                Utils.UtilLogs.LogRegDay(supplier.SupplierCode, Req.golfClubCode, $"Booking", $"reservationStartTime 포멧 오류");
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Invalid reservationStartTime format. Expected HH:mm", null);
            }
            Req.reservationStartTime = parsedStartTime.ToString("hhmm");


            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        string reservationId = null;
                        if (!string.IsNullOrWhiteSpace(supplier.EndPoint))
                        {
                            // 엔드포인트가 존재하는 경우 API 호출
                            string EndpointUrl = supplier.EndPoint.TrimEnd('/') + "/reservation";
                            Utils.UtilLogs.LogRegDay(inboundCode, Req.golfClubCode, "Booking", "예약 정보 전송 시작");

                            // 인증 정보 설정
                            var authentication = supplier.Authentication;
                            string clientCode = authentication.AglCode;
                            string token = ComputeSha256.ComputeSha256Hash(authentication.TokenAgl);

                            var header = new Dictionary<string, string>
                            {
                                { "X-Client-Code", clientCode },
                                { "Authorization", $"Bearer {token}" }
                            };

                            string golfClubCode = Req.inboundCode.Split('_').Last();

                            var reservationRequest = new
                            {
                                golfClubCode,
                                Req.courseCode,
                                Req.reservationDate,
                                Req.reservationStartTime,
                                Req.reservationMembers,
                                Req.currency,
                                Req.totalPrice,
                                Req.holderName,
                                Req.reservationPhone,
                                Req.reservationEmail,
                                Req.reservationCountry,
                                Req.guestInfo
                            };

                            var response = new OAPIReservationResponse();
                            await RestfulClient.POSTAPIHeaderObject<OAPIReservationResponse>(
                                EndpointUrl, header, reservationRequest, async (httpStatusCode, reasonPhrase, apiResponse) =>
                                {
                                    if (httpStatusCode == System.Net.HttpStatusCode.OK)
                                    {
                                        response = apiResponse;
                                        string jsonString = System.Text.Json.JsonSerializer.Serialize(response);
                                        Utils.UtilLogs.LogRegDay(inboundCode, Req.golfClubCode, "Booking", $"예약 요청 성공 statusCode : {httpStatusCode} : {jsonString} ");
                                    }
                                    else
                                    {
                                        Utils.UtilLogs.LogRegDay(inboundCode, Req.golfClubCode, "Booking", $"예약 요청 실패 statusCode : {httpStatusCode} : {reasonPhrase} ");
                                        throw new Exception($"Reservation request failed with status code: {httpStatusCode}");
                                    }
                                });
                            reservationId = response?.reservationId;
                        }
                        else
                        {
                            Utils.UtilLogs.LogRegDay(inboundCode, Req.golfClubCode, "Booking", "엔드포인트가 null, 예약 정보만 저장"); // 엔드포인트가 null인 경우 예약 정보 저장만 진행
                        }

                        // 예약 정보 저장 공통 로직
                        var reservation = new OAPI_ReservationManagement
                        {
                            SupplierId = supplier.SupplierId,
                            ReservationId = reservationId, // API 호출이 없으면 null
                            ReservationStatus = (byte)StatusCode.REQUEST,
                            GolfClubCode = Req.inboundCode.Split('_').Last(),
                            CourseCode = Req.courseCode,
                            ReservationDate = Req.reservationDate,
                            ReservationStartTime = Req.reservationStartTime,
                            ReservationMembers = (byte)Req.reservationMembers,
                            Currency = Req.currency,
                            TotalPrice = Req.totalPrice,
                            HolderName = Req.holderName,
                            ReservationPhone = Req.reservationPhone,
                            ReservationEmail = Req.reservationEmail,
                            ReservationCountry = Req.reservationCountry,
                            CreatedDate = DateTime.UtcNow,
                        };
                        _context.ReservationManagements.Add(reservation);
                        await _context.SaveChangesAsync();
                        int reservationManagementId = reservation.ReservationManagementId;

                        if(string.IsNullOrEmpty(reservationId)) // API 전송하지 않은 예약번호 생성
                        {
                            reservationId = ComputeSha256.ComputeSha256Hash(reservationManagementId.ToString());
                            reservation.ReservationId = reservationId;

                            Utils.UtilLogs.LogRegDay(inboundCode, Req.golfClubCode, "Booking", $"ReservationId 생성됨: {reservationId}");

                            await _context.SaveChangesAsync();
                        }

                        // 예약 요청 시 티타임 재고 정리
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

                        // 예약 이용객 저장
                        if (Req.guestInfo != null && Req.guestInfo.Any())
                        {
                            var guestInfos = Req.guestInfo
                                .Where(g => !string.IsNullOrEmpty(g.guestName) || !string.IsNullOrEmpty(g.guestPhone))
                                .Select((guest, index) => new OAPI_ReservationmanagementGuest
                                {
                                    ReservationManagementId = reservationManagementId,
                                    Idx = (byte)(index + 1),
                                    GuestName = guest.guestName,
                                    GuestPhone = guest.guestPhone,
                                    GuestGender = guest.guestGender.ToUpper() == "MALE" || guest.guestGender.ToUpper() == "M" ? "M" :
                                                  guest.guestGender.ToUpper() == "FEMALE" || guest.guestGender.ToUpper() == "F" ? "F" : null,
                                    GuestCountry = guest.guestCountry
                                })
                                .ToList();

                            if (guestInfos.Any())
                            {
                                _context.ReservationManagementGuests.AddRange(guestInfos);
                                Utils.UtilLogs.LogRegDay(inboundCode, Req.golfClubCode, "Booking", "예약관리 이용객 저장");
                            }
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "Reservation processed successfully", new { reservationId });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Utils.UtilLogs.LogRegDay(inboundCode, Req.golfClubCode, "Booking", "예약 처리 실패", true);
                        return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
                    }
                }
            });
        }


        /// <summary>
        /// 예약 취소
        /// </summary>
        /// <param name="reservationId"></param>
        /// <returns></returns>
        public async Task<IDataResult> PostInboundBookingCancel(ReservationInboundRequest Req)
        {
            var inboundCode = Req.inboundCode;
            if (string.IsNullOrWhiteSpace(inboundCode))
            {
                Utils.UtilLogs.LogRegDay(inboundCode, inboundCode, "Booking", "Booking 연동 키 없음");
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "inboundCode not found", null);
            }

            string goflClubCode = inboundCode.Split("_").Last();

            var reservationManagement = await _context.ReservationManagements.Where(r => r.ReservationId == Req.reservationId && r.GolfClubCode == goflClubCode && r.ReservationStatus != (byte)StatusCode.CANCELLATION).FirstOrDefaultAsync();

            if (reservationManagement == null)
            {
                Utils.UtilLogs.LogRegDay(inboundCode, inboundCode, $"Booking", $"이미 취소했거나 예약 내역이 없습니다.");
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Reservation has already been canceled or could not be found", null);
            }

            var supplier = await _context.Suppliers.Include(s => s.Authentication).FirstOrDefaultAsync(s => s.Authentication.Deleted == false && s.GolfClubs.Any(g => g.InboundCode == inboundCode));

            if (supplier == null)
            {
                Utils.UtilLogs.LogRegDay(inboundCode, inboundCode, $"Booking", $"예약 요청 공급사 누락");
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Supplier not found", null);
            }

            // 엔드포인트 설정
            string EndpointUrl = supplier.EndPoint;

            try
            {
                if (string.IsNullOrWhiteSpace(EndpointUrl))
                {
                    // 엔드포인트가 없는 경우 저장 로직만 수행
                    Utils.UtilLogs.LogRegDay(inboundCode, inboundCode, "Booking", "엔드포인트 없음, 저장 로직만 수행");

                    reservationManagement.ReservationStatus = (byte)StatusCode.CANCELLATIONREQUEST;
                    reservationManagement.UpdatedDate = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    Utils.UtilLogs.LogRegDay(inboundCode, inboundCode, "Booking", "예약 취소 정보 저장 완료");
                    return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "Reservation cancel information saved successfully", null);
                }
                else
                {
                    // 인증 정보 설정
                    var authentication = supplier.Authentication;
                    string clientCode = authentication.AglCode;
                    string token = ComputeSha256.ComputeSha256Hash(authentication.TokenAgl);

                    EndpointUrl = $"{EndpointUrl.TrimEnd('/')}/reservation/cancel";

                    Utils.UtilLogs.LogRegDay(inboundCode, inboundCode, "Booking", "예약 정보 전송 시작");
                    // HTTP 요청 설정

                    var header = new Dictionary<string, string>
                    {
                        { "X-Client-Code", clientCode },
                        { "Authorization", $"Bearer {token}" },
                    };

                    // JSON 컨텐츠 생성
                    var reservationRequest = new
                    {
                        reservationId = Req.reservationId,
                    };

                    var response = new OAPIDataResponse<CancelResponse>();
                    string strReaponse = string.Empty;

                    await RestfulClient.POSTAPIHeaderObject<OAPIDataResponse<CancelResponse>>(EndpointUrl, header, reservationRequest, async (httpStatusCode, reasonPhrase, apiResponse) =>
                            {
                                if (httpStatusCode == System.Net.HttpStatusCode.OK)
                                {
                                    response = apiResponse;
                                    string jsonString = System.Text.Json.JsonSerializer.Serialize(response);
                                    Utils.UtilLogs.LogRegDay(inboundCode, inboundCode, $"Booking", $"예약 요청 성공 statusCode : {httpStatusCode} : {jsonString} ");
                                }
                                else
                                {
                                    strReaponse = reasonPhrase;
                                    Utils.UtilLogs.LogRegDay(inboundCode, inboundCode, "Booking", $"예약 요청 실패 statusCode : {httpStatusCode} : {strReaponse} ");
                                    await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, $"Reservation request failed with status code: {httpStatusCode}", null);
                                }
                            });

                    if (response != null)
                    {
                        var existingReservationManagement = await _context.ReservationManagements.FirstOrDefaultAsync(m => m.ReservationId == Req.reservationId && m.SupplierId == supplier.SupplierId);

                        if (existingReservationManagement != null)
                        {
                            if (response != null) // 응답이 있을시 각각의 정보를 저장
                            {
                                existingReservationManagement.ReservationStatus = (byte)StatusCode.CANCELLATION;
                                existingReservationManagement.UpdatedDate = DateTime.UtcNow;
                                existingReservationManagement.cancelDate = response?.data?.cancelDate;
                                existingReservationManagement.cancelPenaltyAmount = response?.data?.cancelPenaltyAmount;
                                existingReservationManagement.cancelCurrency = response?.data?.currency;

                            }

                            await _context.SaveChangesAsync();
                            Utils.UtilLogs.LogRegDay(inboundCode, inboundCode, "Booking", "예약 요청 저장 성공");
                            return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "Reservation Request was successfully", null);
                        }
                        else
                        {
                            Utils.UtilLogs.LogRegDay(inboundCode, inboundCode, "Booking", "예약 관리 정보 없음", true);
                            return await _commonService.CreateResponse<object>(true, ResultCode.NOT_FOUND, "No reservation management information found", null);
                        }
                    }
                    else
                    {
                        Utils.UtilLogs.LogRegDay(inboundCode, inboundCode, "Booking", "API 응답 없음", true);
                        return await _commonService.CreateResponse<object>(true, ResultCode.SERVER_ERROR, "No response received from the API", null);
                    } 
                }


            }
            catch (Exception ex)
            {
                Utils.UtilLogs.LogRegDay(inboundCode, inboundCode, "Booking", "예약 요청 저장 실패", true);
                return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
            }

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

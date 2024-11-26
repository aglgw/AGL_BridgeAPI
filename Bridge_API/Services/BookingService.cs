using AGL.Api.Bridge_API.Interfaces;
using AGL.Api.Bridge_API.Models.OAPI;
using AGL.Api.Bridge_API.Utils;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;
using static AGL.Api.Bridge_API.Models.OAPI.OAPI;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIResponse;
using static System.Runtime.InteropServices.JavaScript.JSType;
using AGL.Api.ApplicationCore.Utilities;
using System.Globalization;
using AGL.Api.ApplicationCore.Helpers;
using System.Net;
using System.Net.Mail;
using System.Diagnostics;

namespace AGL.Api.Bridge_API.Services
{
    public class BookingService : BaseService,IBookingService
    {
        private readonly OAPI_DbContext _context;
        private IConfiguration _configuration { get; }
        private readonly ICommonService _commonService;

        public BookingService(OAPI_DbContext context,
            ICommonService commonService,
            IConfiguration configuration)
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
        public async Task<IDataResult> POSTBookingRequest(ReqBookingRequest Req)
        {
            //ComputeSha256.ComputeSha256Hash


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

            // 인증 정보 설정
            var authentication = supplier.Authentication;
            string clientCode = authentication.AglCode;
            string token = GenerateSHA256Hash(authentication.TokenAgl);

            // 엔드포인트 설정
            string EndpointUrl = supplier.EndPoint;

            EndpointUrl = $"{EndpointUrl.TrimEnd('/')}/reservation";

            Utils.UtilLogs.LogRegDay(inboundCode, Req.golfClubCode, "Booking", "예약 정보 전송 시작");
            // HTTP 요청 설정

            var header = new Dictionary<string, string>
            {
                { "X-Client-Code", clientCode },
                { "Authorization", $"Bearer {token}" },
                //{ "Content-Type", "application/json" }
            };

            string golfClubCode = Req.inboundCode.Split('_').Last();

            // JSON 컨텐츠 생성
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
            string strReaponse = string.Empty;

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // API 전송
                        await RestfulClient.POSTAPIHeaderObject<OAPIReservationResponse>(EndpointUrl, header, reservationRequest, async (httpStatusCode, reasonPhrase, apiResponse) =>
                        {
                            if (httpStatusCode == System.Net.HttpStatusCode.OK)
                            {
                                response = apiResponse;
                                string jsonString = System.Text.Json.JsonSerializer.Serialize(response);
                                Utils.UtilLogs.LogRegDay(inboundCode, Req.golfClubCode, $"Booking", $"예약 요청 성공 statusCode : {httpStatusCode} : {jsonString} ");
                            }
                            else
                            {
                                strReaponse = reasonPhrase;
                                Utils.UtilLogs.LogRegDay(inboundCode, Req.golfClubCode, "Booking", $"예약 요청 실패 statusCode : {httpStatusCode} : {strReaponse} ");
                                await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, $"Reservation request failed with status code: {httpStatusCode}", null);
                            }
                        });

                        Dictionary<string, string> tmpData = new Dictionary<string, string>();
                        if (response != null)
                        {
                            // 예약 관리 DB에 추가
                            var reservation = new OAPI_ReservationManagement
                            {
                                SupplierId = supplier.SupplierId,
                                ReservationId = response?.reservationId,
                                ReservationStatus = (byte)StatusCode.REQUEST,
                                GolfClubCode = golfClubCode,
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

                            Utils.UtilLogs.LogRegDay(inboundCode, Req.golfClubCode, "Booking", "예약 정보 저장");

                            // 예약 관리 이용객 DB에 추가
                            var newGuestInfos = new List<OAPI_ReservationmanagementGuest>();
                            if (Req.guestInfo != null)
                            {
                                var i = 1;
                                foreach (var guestInfo in Req.guestInfo)
                                {
                                    if (string.IsNullOrEmpty(guestInfo.guestName) && string.IsNullOrEmpty(guestInfo.guestPhone) && string.IsNullOrEmpty(guestInfo.guestGender) && string.IsNullOrEmpty(guestInfo.guestCountry))
                                    {
                                        continue;
                                    }

                                    if (guestInfo.guestGender.ToUpper() == "MALE" || guestInfo.guestGender.ToUpper() == "M")
                                        guestInfo.guestGender = "M";
                                    else if (guestInfo.guestGender.ToUpper() == "FEMALE" || guestInfo.guestGender.ToUpper() == "F")
                                        guestInfo.guestGender = "F";
                                    else
                                        guestInfo.guestGender = null;

                                    var newGuestInfo = new OAPI_ReservationmanagementGuest
                                    {
                                        ReservationManagementId = reservationManagementId,
                                        Idx = (byte)i,
                                        GuestName = guestInfo.guestName,
                                        GuestPhone = guestInfo.guestPhone,
                                        GuestGender = guestInfo.guestGender,
                                        GuestCountry = guestInfo.guestCountry
                                    };
                                    i++;
                                    newGuestInfos.Add(newGuestInfo);
                                }

                                if (newGuestInfos.Any())
                                {
                                    _context.ReservationManagementGuests.AddRange(newGuestInfos);
                                    Utils.UtilLogs.LogRegDay(inboundCode, Req.golfClubCode, "Booking", "예약관리 이용객 저장");
                                }

                            }
                            tmpData.Add("reservationId", response?.reservationId);
                            Utils.UtilLogs.LogRegDay(inboundCode, Req.golfClubCode, "Booking", "예약 정보 전송 종료");
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "Reservation Cancel Request was successfully", tmpData);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Utils.UtilLogs.LogRegDay(inboundCode, Req.golfClubCode, "Booking", "예약요청 저장 실패",true);
                        return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
                    }
                }
            });
        }

        /// <summary>
        /// 예약 조회
        /// </summary>
        /// <param name="Req"></param>
        /// <returns></returns>
        public async Task<IDataResult> GetBookingInquiry(ReqBookingInquiry Req)
        {


            using (var httpClient = new HttpClient())
            {
                var url = $"";

                var response = await httpClient.GetAsync(url);

                var responseString = await response.Content.ReadAsStringAsync();


                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {

                    var resp = JsonConvert.DeserializeObject<OAPICommonListResponse<List<BookingInfo>>>(responseString);

                    var rstMsg = string.Empty;

                    if (string.Equals(resp?.rstCd.ToLower(), "success"))
                    {

                        //데이터 처리 부분 추가 필요

                    }
                    else
                    {
                        rstMsg = resp != null ? resp.rstMsg : "Not Found Result";

                        return await _commonService.CreateResponse<object>(true, ResultCode.SERVER_ERROR, rstMsg, null);
                    }
                }
            }


            return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "Reservation inquiry was successfully",null);

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
                await RestfulClient.GETAPIHeaderQuery<OAPIReservationConfirmListResponse>(EndpointUrl, header, null, (status, reasonPhrase, apiResponse) =>
                {
                    if (status == System.Net.HttpStatusCode.OK)
                    {
                        string jsonString = System.Text.Json.JsonSerializer.Serialize(apiResponse);
                        response = apiResponse;
                        
                    }
                    else
                    {
                        strReaponse = reasonPhrase;
                        Utils.UtilLogs.LogRegHour(inboundCode, inboundCode, $"Booking", $"확정 목록 조회 실패 statusCode : {status} : {strReaponse} ",true);
                    }
                });

                if (response != null)
                {


                    Utils.UtilLogs.LogRegHour(inboundCode, inboundCode, $"Booking", $"확정 목록 조회 성공");
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

        /// <summary>
        /// 예약 취소
        /// </summary>
        /// <param name="reservationId"></param>
        /// <returns></returns>
        public async Task<IDataResult> PostBookingCancel(ReservationInboundRequest Req)
        {
            var inboundCode = Req.inboundCode;
            if (string.IsNullOrWhiteSpace(inboundCode))
            {
                Utils.UtilLogs.LogRegDay(inboundCode, inboundCode, "Booking", "Booking 연동 키 없음");
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "inboundCode not found", null);
            }

            string goflClubCode = inboundCode.Split("_").Last();

            var reservationManagement = await  _context.ReservationManagements.Where(r => r.ReservationId == Req.reservationId && r.GolfClubCode == goflClubCode && r.ReservationStatus != 3).FirstOrDefaultAsync();

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

            // 인증 정보 설정
            var authentication = supplier.Authentication;
            string clientCode = authentication.AglCode;
            string token = GenerateSHA256Hash(authentication.TokenAgl);

            // 엔드포인트 설정
            string EndpointUrl = supplier.EndPoint;

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

            var response = new OAPIDataResponse<cancelResponse>();
            string strReaponse = string.Empty;

            try
            {
                await RestfulClient.POSTAPIHeaderObject<OAPIDataResponse<cancelResponse>>(EndpointUrl, header, reservationRequest, async (httpStatusCode, reasonPhrase, apiResponse) =>
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
            catch (Exception ex)
            {
                Utils.UtilLogs.LogRegDay(inboundCode, inboundCode, "Booking", "예약 요청 저장 실패", true);
                return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
            }

        }


        public async Task<IDataResult> PostBookingConfirm(ReservationRequest request, string supplierCode)
        {
            var reservationId = request.reservationId;

            // 입력 값 검증 - reservationId와 supplierCode가 비어 있는지 확인
            if (string.IsNullOrEmpty(reservationId) || string.IsNullOrEmpty(supplierCode))
            {
                Utils.UtilLogs.LogRegHour(supplierCode, "Confirm", "Confirm", $"예약번호 또는 공급사 코드 없음 {reservationId} : {supplierCode}");
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "reservationId or supplierCode is invalid", null);
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
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

                        if (reservation != null)
                        {
                            reservation.ReservationStatus = (byte)StatusCode.CONFIRMATION; // 1 예약요청 2 예약확정 3 예약취소
                            reservation.UpdatedDate = DateTime.UtcNow;

                            await _context.SaveChangesAsync();
                            Utils.UtilLogs.LogRegHour(supplierCode, "Confirm", "Confirm", "예약관리에 변경 완료");
                        }
                        else
                        {
                            Utils.UtilLogs.LogRegHour(supplierCode, "Confirm", "Confirm", "예약관리에서 검색 안됨");
                            return await _commonService.CreateResponse<object>(false, ResultCode.NOT_FOUND, "Reservation not found", null);
                        }

                        await transaction.CommitAsync();
                        Utils.UtilLogs.LogRegHour(supplierCode, "Confirm", "Confirm", "예약확정 완료");
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

    }
}

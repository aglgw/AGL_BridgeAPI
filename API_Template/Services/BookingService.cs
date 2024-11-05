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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;
using static AGL.Api.API_Template.Models.OAPI.OAPI;
using static AGL.Api.API_Template.Models.OAPI.OAPIResponse;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AGL.Api.API_Template.Services
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
        public async Task<IDataResult> POSTBookingRequest(ReqBookingRequest Req, string SupplierCode)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    /*
                     * - 공급사 코드로 API 인증을 위한 인증 키, 코드 가져오기
                     * - ReservationManagements 예약관리 DB에 추가
                     * - json 양식 맞춘후 공급사 코드, 토큰, 엔드포인트로 전송
                     */
                    OAPI_Supplier? supplier = _context.Suppliers.Where(s => s.SupplierCode == SupplierCode).FirstOrDefault();

                    if (supplier == null)
                    {
                        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Supplier not found", null);
                    }

                    // 인증 정보 설정
                    string clientCode = supplier.SupplierCode;
                    string token = GenerateSHA256Hash(supplier.TokenAglToSupplier);

                    // 엔드포인트 설정
                    string EndpointUrl;
                    if (_configuration["AppSetting:Environment"] == "dev")
                    {
                        EndpointUrl = supplier.EndPointDev;
                    }
                    else
                    {
                        EndpointUrl = supplier.EndPointProd;
                    }

                    // HTTP 요청 설정
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("X-Client-Code", clientCode);
                        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                        var jsonContent = JsonConvert.SerializeObject(Req);
                        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                        var response = await client.PostAsync(EndpointUrl, httpContent);

                        if (response.IsSuccessStatusCode)
                        {
                            var responseContent = await response.Content.ReadAsStringAsync();
                            var responseJson = JsonConvert.DeserializeObject<dynamic>(responseContent);

                            // 예약 관리 DB에 추가
                            var reservation = new OAPI_ReservationManagement
                            {
                                SupplierId = supplier.SupplierId,
                                ReservationId = responseJson?.ReservationId,
                                SalesChannel = "",
                                Endpoint = "",
                                ReservationStatus = 1,
                                CreatedDate = DateTime.UtcNow,
                            };
                            _context.ReservationManagements.Add(reservation);
                            await _context.SaveChangesAsync();

                            await transaction.CommitAsync();

                            return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "Reservation Request was successfully", null);
                        }
                        else
                        {
                            return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, "Failed to send reservation request", null);
                        }
                    }

                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
                }
            }
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

                    if (string.Equals(resp?.RstCd.ToLower(), "success"))
                    {

                        //데이터 처리 부분 추가 필요

                    }
                    else
                    {
                        rstMsg = resp != null ? resp.RstMsg : "Not Found Result";

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
        public async Task<IDataResult> GetConfirmBookingInquiry(string reservationId)
        {

            if (await _context.ReservationManagements.AnyAsync(x=>x.ReservationId==reservationId))
                return await _commonService.CreateResponse<object>(true, ResultCode.NOT_FOUND, "Reservation data does not exist", null);



            using (var httpClient = new HttpClient())
            {
                var url = $"";

                var response = await httpClient.GetAsync(url);

                var responseString = await response.Content.ReadAsStringAsync();


                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {

                    var resp = JsonConvert.DeserializeObject<OAPICommonResponse<ConfirmBookingInfo>>(responseString);

                    var rstMsg = string.Empty;

                    if (string.Equals(resp?.RstCd.ToLower(), "success"))
                    {

                        //데이터 처리 부분 추가 필요

                    }
                    else
                    {
                        rstMsg = resp != null ? resp.RstMsg : "Not Found Result";

                        return await _commonService.CreateResponse<object>(true, ResultCode.SERVER_ERROR, rstMsg, null);
                    }
                }
            }


            return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "Confirmation Reservation inquiry was successfully", null);

        }


        public async Task<IDataResult> GetBookingCancel(OAPIReservationRequest Req)
        {

            if(string.IsNullOrWhiteSpace(Req.ReservationId))
                return await _commonService.CreateResponse<object>(true, ResultCode.INVALID_INPUT, "reservationId is invalid", null);


            if (await _context.ReservationManagements.AnyAsync(x => x.ReservationId == Req.ReservationId))
                return await _commonService.CreateResponse<object>(true, ResultCode.NOT_FOUND, "Reservation data does not exist", null);


            using (var httpClient = new HttpClient())
            {
                var url = $"";

                var response = await httpClient.GetAsync(url);

                var responseString = await response.Content.ReadAsStringAsync();


                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {

                    var resp = JsonConvert.DeserializeObject<OAPICommonResponse<CancelBookingInfo>>(responseString);

                    var rstMsg = string.Empty;

                    if (string.Equals(resp?.RstCd.ToLower(), "success"))
                    {

                        //데이터 처리 부분 추가 필요

                    }
                    else
                    {
                        rstMsg = resp != null ? resp.RstMsg : "Not Found Result";

                        return await _commonService.CreateResponse<object>(true, ResultCode.SERVER_ERROR, rstMsg, null);
                    }
                }
            }

            return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "Reservation cancel was successfully", null);

        }


        public async Task<IDataResult> PostBookingConfirm(OAPIReservationRequest request, string supplierCode)
        {
            var reservationId = request.ReservationId;

            // 입력 값 검증 - reservationId와 supplierCode가 비어 있는지 확인
            if (string.IsNullOrEmpty(reservationId) || string.IsNullOrEmpty(supplierCode))
            {
                return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "reservationId or supplierCode is invalid", null);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 공급자 정보 조회 - supplierCode에 해당하는 공급자를 데이터베이스에서 검색
                    var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.SupplierCode == supplierCode);
                    if (supplier == null)
                    {
                        return await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Supplier not found", null);
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
                        return await _commonService.CreateResponse<object>(false, ResultCode.NOT_FOUND, "Reservation not found", null);
                    }

                    // 트랜잭션 커밋
                    await transaction.CommitAsync();

                    return await _commonService.CreateResponse<object>(true, ResultCode.SUCCESS, "Reservation confirmed successfully", null);
                }
                catch (Exception ex)
                {
                    // 오류 발생 시 트랜잭션 롤백
                    await transaction.RollbackAsync();

                    return await _commonService.CreateResponse<object>(false, ResultCode.SERVER_ERROR, ex.Message, null);
                }
            }
        }

        public async Task<IDataResult> Test()
        {

            var rst = new BookingInfo
            {
                ReservationId = "",
                GolfclubCode = "",
                PlayDate = "",
                StartTime = "",
                PlayerCount = 0,
                Status = 0,
                OrderDate = "",
                Currency = "s",
                CancelPenaltyAmount = Convert.ToDecimal(1)
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

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


    }
}

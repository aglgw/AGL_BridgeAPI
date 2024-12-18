using AGL.Api.Bridge_API.Interfaces;
using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using AGL.Api.ApplicationCore.Filters;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIRequest;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIResponse;
using AGL.Api.ApplicationCore.Models.Enum;
using AGL.Api.Bridge_API.Services;
using static AGL.Api.Bridge_API.Utils.Util;
using Microsoft.AspNetCore.Http.Extensions;

namespace AGL.Api.Bridge_API.Controllers
{
    public class BookingController : ApiControllerBase
    {
        private readonly ILogger<BookingController> _logger;
        private readonly IBookingService _bookingService;
        private readonly ICommonService _commonService;

        public BookingController(ILogger<BookingController> logger, IBookingService bookingService, ICommonService commonService)
        {
            _logger = logger;
            _bookingService = bookingService;
            _commonService = commonService;
        }
        /*
                [AllowAnonymous]
                [Route("test")]
                [HttpGet]
                public async Task<IDataResult> GetTest()
                {


                    var result = await _bookingService.Test();


                    return result;
                }


                /// <summary>
                /// 예약 조회
                /// </summary>        
                /// <returns></returns>
                [Route("reservation/list")]
                [HttpGet]
                [SkipAuthentication] // 인증 미들웨어 패스
                [EnvironmentSpecific("Development")] // Development 환경에서만 표시
                public async Task<IDataResult> GetBookingInquiry(
                    [FromHeader(Name = "X-Supplier-Code")][Required] string SupplierCode, [FromBody]ReqBookingInquiry Req)
                {


                    var result = await _bookingService.GetBookingInquiry(Req);


                    return result;
                }

                /// <summary>
                /// 예약 확정 조회
                /// </summary>
                /// <returns></returns>
                [Route("reservation/confirm/{reservationId}")]
                [HttpGet]
                [SkipAuthentication] // 인증 미들웨어 패스
                [EnvironmentSpecific("Development")] // Development 환경에서만 표시
                public async Task<IDataResult> GetConfirmBookingInquiry(
                    [FromRoute] string reservationId,
                    [FromQuery] string daemonId)
                {
                    var result = await _bookingService.GetConfirmBookingInquiry(reservationId, daemonId);

                    return result;
                }
                */

        /// <summary>
        /// Creates a test reservation request in the system (for sandbox use only)
        /// </summary>
        /// <returns></returns>
        [Route("reservation/test")]
        [HttpPost]
        [EnvironmentSpecific("Development", "SandBox")]
        [DisableValidation]
        public async Task<ActionResult<OAPIDataResponse<ReservationReponse>>> PostTestBookingRequest(
            [FromHeader(Name = "X-Supplier-Code")][Required] string supplierCode,
            [FromServices] IWebHostEnvironment env)
        {
            // 현재 환경 확인
            if (env.EnvironmentName != "Development" && env.EnvironmentName != "SandBox")
            {
                var errorResponse = await _commonService.CreateResponse<object>(false, ResultCode.INVALID_INPUT, "Supplier not found", null);
                return ResponseUtil.HandleResponse(errorResponse.Value);
            }
            else
            {
                var result = await _bookingService.PostTestBookingRequest(supplierCode);

                return ResponseUtil.HandleResponse(result);
            }
        }

        /// <summary>
        /// Retrieves a list of reservation requests for confirmation or cancellation
        /// </summary>
        /// <returns></returns>
        [Route("reservation/requests")]
        [HttpGet]
        [DisableValidation]
        public async Task<ActionResult<OAPIDataResponse<List<ReservationReponse>>>> GetBookingRequest(
            [FromHeader(Name = "X-Supplier-Code")][Required] string supplierCode,
            [FromQuery] ReservationListRequest request)
        {
            var currentUrl = HttpContext.Request.GetDisplayUrl();
            Utils.UtilLogs.LogRegHour(supplierCode, "Booking", "BookingList", $"예약 요청 검색 URL : {currentUrl}");

            var result = await _bookingService.GetBookingRequest(request, supplierCode);

            return ResponseUtil.HandleResponse(result);
        }

        /// <summary>
        /// Confirms a reservation request
        /// </summary>
        /// <returns></returns>
        [Route("reservation/confirm")]
        [HttpPost]
        public async Task<ActionResult<IDataResult>> PostBookingConfirm(
            [FromHeader(Name = "X-Supplier-Code")][Required] string supplierCode, 
            ReservationRequest request)
        {
            var result = await _bookingService.PostBookingConfirm(request, supplierCode);

            return ResponseUtil.HandleResponse(result);
        }

        /// <summary>
        /// Retrieves a list of cancellation requests for processing
        /// </summary>
        /// <returns></returns>
        [Route("reservation/cancellations")]
        [HttpGet]
        [DisableValidation]
        public async Task<ActionResult<OAPIDataResponse<List<ReservationReponse>>>> GetBookingCancellations(
            [FromHeader(Name = "X-Supplier-Code")][Required] string supplierCode,
            [FromQuery] ReservationListRequest request)
        {
            var currentUrl = HttpContext.Request.GetDisplayUrl();
            Utils.UtilLogs.LogRegHour(supplierCode, "Booking", "BookingList", $"예약 취소요청 검색 URL : {currentUrl}");

            var result = await _bookingService.GetBookingCancellations(request, supplierCode);

            return ResponseUtil.HandleResponse(result);
        }


        /// <summary>
        /// Cancels a specific reservation
        /// </summary>
        /// <returns></returns>
        [Route("reservation/cancel")]
        [HttpPost]
        public async Task<ActionResult<IDataResult>> PostBookingCancel(
            [FromHeader(Name = "X-Supplier-Code")][Required] string supplierCode,
            cancelRequest request)
        {
            var result = await _bookingService.PostBookingCancel(request, supplierCode);

            return ResponseUtil.HandleResponse(result);
        }
    }
}

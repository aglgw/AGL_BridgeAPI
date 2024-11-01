using AGL.Api.API_Template.Interfaces;
using AGL.Api.API_Template.Models.OAPI;
using AGL.Api.API_Template.Services;
using AGL.Api.ApplicationCore.Extensions;
using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using static AGL.Api.API_Template.Models.OAPI.OAPIResponse;

namespace AGL.Api.API_Template.Controllers
{
    public class BookingController : ApiControllerBase
    {
        private readonly ILogger<BookingController> _logger;
        private readonly IBookingService _bookingService;

        public BookingController(ILogger<BookingController> logger, IBookingService bookingService)
        {
            _logger = logger;
            _bookingService = bookingService;
        }

        /// <summary>
        /// 예약 조회
        /// </summary>
        /// <returns></returns>
        [Route("reservationList")]
        [HttpGet]
        public async Task<IDataResult> GetBookingInquiry(
            [FromHeader(Name = "X-Client-Code")][Required] string ClientCode, [FromBody]ReqBookingInquiry Req)
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
        public async Task<IDataResult> GetConfirmBookingInquiry(
            [FromHeader(Name = "X-Client-Code")][Required] string ClientCode, [FromRoute] string reservationId)
        {
            var result = await _bookingService.GetConfirmBookingInquiry(reservationId);

            return result;
        }


        /// <summary>
        /// 예약 취소
        /// </summary>
        /// <returns></returns>
        [Route("cancel")]
        [HttpPost]
        public async Task<IDataResult> GetBookingCancel(
            [FromHeader(Name = "X-Client-Code")][Required] string ClientCode, [FromBody] OAPIReservationRequest Req)
        {

            var result = await _bookingService.GetBookingCancel(Req);

            return result;
        }


        /// <summary>
        /// 예약 확정
        /// </summary>
        /// <returns></returns>
        [Route("reservation/confirm")]
        [HttpPost]
        public async Task<IDataResult> PostBookingConfirm(
            [FromHeader(Name = "X-Supplier-Code")][Required] string X_Supplier_Code, OAPIReservationRequest request)
        {
            var result = await _bookingService.PostBookingConfirm(request, X_Supplier_Code);

            return result;
        }
    }
}

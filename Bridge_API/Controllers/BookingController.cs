﻿using AGL.Api.Bridge_API.Interfaces;
using AGL.Api.Bridge_API.Models.OAPI;
using AGL.Api.Bridge_API.Services;
using AGL.Api.ApplicationCore.Extensions;
using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using AGL.Api.ApplicationCore.Models.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIResponse;

namespace AGL.Api.Bridge_API.Controllers
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
#if DEBUG
        [AllowAnonymous]
        [Route("test")]
        [HttpGet]
        public async Task<IDataResult> GetTest()
        {


            var result = await _bookingService.Test();


            return result;
        }

        /// <summary>
        /// 예약 요청
        /// </summary>
        /// <returns></returns>
        [Route("reservation")]
        [HttpPost]
        public async Task<IDataResult> POSTBookingRequest(
            [FromHeader(Name = "X-Supplier-Code")][Required] string SupplierCode, ReqBookingRequest request)
        {
            var result = await _bookingService.POSTBookingRequest(request, SupplierCode);

            return result;
        }

        /// <summary>
        /// 예약 조회
        /// </summary>
        /// <returns></returns>
        [Route("reservation/list")]
        [HttpGet]
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
        public async Task<IDataResult> GetConfirmBookingInquiry(
            [FromHeader(Name = "X-Supplier-Code")][Required] string SupplierCode, [FromRoute] string reservationId)
        {
            var result = await _bookingService.GetConfirmBookingInquiry(reservationId);

            return result;
        }


        /// <summary>
        /// 예약 취소
        /// </summary>
        /// <returns></returns>
        [Route("reservation/cancel")]
        [HttpPost]
        public async Task<IDataResult> GetBookingCancel(
            [FromHeader(Name = "X-Supplier-Code")][Required] string SupplierCode, [FromBody] ReservationRequest Req)
        {

            var result = await _bookingService.GetBookingCancel(Req);

            return result;
        }

#endif
        /// <summary>
        /// 예약 확정
        /// </summary>
        /// <returns></returns>
        [Route("reservation/confirm")]
        [HttpPost]
        public async Task<IDataResult> PostBookingConfirm(
            [FromHeader(Name = "X-Supplier-Code")][Required] string SupplierCode, ReservationRequest request)
        {
            var result = await _bookingService.PostBookingConfirm(request, SupplierCode);

            return result;
        }
    }
}

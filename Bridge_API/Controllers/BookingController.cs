﻿using AGL.Api.Bridge_API.Interfaces;
using AGL.Api.ApplicationCore.Infrastructure;
using AGL.Api.ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using AGL.Api.ApplicationCore.Filters;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIRequest;

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
        [SkipAuthentication] // 인증 미들웨어 패스
        [EnvironmentSpecific("Development")] // Development 환경에서만 표시
        public async Task<IDataResult> POSTBookingRequest(
            [FromBody] ReqBookingRequest request)
        {
            var result = await _bookingService.POSTBookingRequest(request);

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


        /// <summary>
        /// 예약 취소
        /// </summary>
        /// <returns></returns>
        [Route("reservation/cancel")]
        [HttpPost]
        [SkipAuthentication] // 인증 미들웨어 패스
        [EnvironmentSpecific("Development")] // Development 환경에서만 표시
        public async Task<IDataResult> PostBookingCancel(
            [FromBody] ReservationInboundRequest Req)
        {

            var result = await _bookingService.PostBookingCancel(Req);

            return result;
        }

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

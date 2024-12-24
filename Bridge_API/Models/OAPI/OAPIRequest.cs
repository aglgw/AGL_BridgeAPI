using AGL.Api.ApplicationCore.Helpers;
using AGL.Api.ApplicationCore.Utilities;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using static AGL.Api.Bridge_API.Models.OAPI.OAPI;

namespace AGL.Api.Bridge_API.Models.OAPI
{

    public class OAPIRequest
    {
        /// <summary>
        /// 골프장 코드 인터페이스
        /// </summary>
        public interface IGolfClubCode
        {
            string? golfClubCode { get; set; }
        }

        /// <summary>
        /// 인바운드 코드 인터페이스
        /// </summary>
        public interface IInboundCode
        {
            string inboundCode { get; set; }
        }

        /// <summary>
        /// 날짜 범위 인터페이스
        /// </summary>
        public interface IDateRange
        {
            string startDate { get; set; }
            string endDate { get; set; }
        }

        /// <summary>
        /// 예약 기본 인터페이스
        /// </summary>
        public interface IReservationBase
        {
            string? reservationId { get; set; }
        }

        /// <summary>
        /// 골프장 기본 베이스
        /// </summary>
        [DataContract]
        public class GolfClubCodeBase : IGolfClubCode
        {
            /// <summary>
            /// 골프장 코드
            /// </summary>
            [DataMember]
            public string? golfClubCode { get; set; }
        }

        /// <summary>
        /// 티타임 등록/수정 요청
        /// </summary>
        [DataContract]
        public class TeeTimeRequest : IGolfClubCode
        {
            /// <summary>
            /// 골프장 코드
            /// </summary>
            [DataMember]
            [Required]
            public string? golfClubCode { get; set; }

            /// <summary>
            /// 날짜적용방법 - 특정 공휴일이 있을시 2번 사용 (1 기간, 2 적용일)
            /// </summary>
            [DataMember]
            [Required]
            public int dateApplyType { get; set; }

            /// <summary>
            /// 기간 시작일
            /// </summary>
            [DataMember]
            public string? startPlayDate { get; set; }

            /// <summary>
            /// 기간 종료일
            /// </summary>
            [DataMember]
            public string? endPlayDate { get; set; }

            /// <summary>
            /// 예외일 (공휴일 적용)
            /// </summary>
            [DataMember]
            public List<string>? exceptionDate { get; set; }

            /// <summary>
            /// 기간 중 적용 요일 (1 월요일, 2 화요일, 3 수요일, 4 목요일, 5 금요일, 6 토요일, 7 일요일)
            /// </summary>
            [DataMember]
            public List<int>? week { get; set; }

            /// <summary>
            /// 적용일
            /// </summary>
            [DataMember]
            public List<string>? effectiveDate { get; set; }

            /// <summary>
            /// 티타임 정보 리스트
            /// </summary>
            [DataMember]
            [Required]
            public List<TeeTimeInfo> teeTimeInfo { get; set; }
        }

        /// <summary>
        /// 티타임 등록/수정 백그라운드
        /// </summary>
        [DataContract]
        public class TeeTimeBackgroundRequest : TeeTimeRequest, IInboundCode
        {
            /// <summary>
            /// 인바운드 코드
            /// </summary>
            [DataMember]
            public string inboundCode { get; set; }

            /// <summary>
            /// 공급사 코드
            /// </summary>
            [DataMember]
            public string supplierCode { get; set; }
        }

        /// <summary>
        /// 티타임 조회 요청
        /// </summary>
        [DataContract]
        public class TeeTimeGetRequest : IGolfClubCode, IDateRange
        {
            /// <summary>
            /// 시작일
            /// </summary>
            [DataMember]
            [Required]
            public string startDate { get; set; }

            /// <summary>
            /// 종료일
            /// </summary>
            [DataMember]
            [Required]
            public string endDate { get; set; }

            /// <summary>
            /// 골프장 코드
            /// </summary>
            [DataMember]
            [Required]
            public string golfClubCode { get; set; }
        }

        /// <summary>
        /// 티타임 상태 수정 요청
        /// </summary>
        [DataContract]
        public class TeeTimeAvailabilityRequest : IGolfClubCode
        {
            /// <summary>
            /// 골프장 코드
            /// </summary>
            [DataMember]
            [Required]
            public string golfClubCode { get; set; }

            /// <summary>
            /// 플레이 날짜
            /// </summary>
            [DataMember]
            public string? playDate { get; set; }

            /// <summary>
            /// 코스 코드 리스트
            /// </summary>
            [DataMember]
            [JsonConverter(typeof(ObjectToListConverter))]
            public object? courseCode { get; set; }

            /// <summary>
            /// 시간 정보 리스트
            /// </summary>
            [DataMember]
            public List<TimeInfo>? time { get; set; }

            /// <summary>
            /// 판매 여부 (true: 판매, false: 판매 안 함)
            /// </summary>
            [DataMember]
            [Required]
            public bool available { get; set; }
        }
        /// <summary>
        /// 예약 확정 요청
        /// </summary>
        [DataContract]
        public class ReservationListRequest : IReservationBase
        {
            /// <summary>
            /// 예약일
            /// </summary>
            [DataMember]
            public string? reservationDate { get; set; }
            /// <summary>
            /// 예약 ID
            /// </summary>
            [DataMember]
            public string? reservationId { get; set; }
            /// <summary>
            /// 상태
            /// </summary>
            [DataMember]
            public byte? status { get; set; }
        }

        /// <summary>
        /// 예약 확정 요청
        /// </summary>
        [DataContract]
        public class ReservationRequest : IReservationBase
        {
            /// <summary>
            /// 예약 ID
            /// </summary>
            [DataMember]
            [Required]
            public string? reservationId { get; set; }
        }

        /// <summary>
        /// 예약 확정 - 내부 연동용 요청
        /// </summary>
        [DataContract]
        public class ReservationInboundRequest : ReservationRequest, IInboundCode
        {
            /// <summary>
            /// 인바운드 코드
            /// </summary>
            [DataMember]
            public string inboundCode { get; set; }
        }

        /// <summary>
        /// 예약 목록 조회 요청
        /// </summary>
        [DataContract]
        public class ReqBookingInquiry : GolfClubCodeBase, IInboundCode, IDateRange
        {
            /// <summary>
            /// 인바운드 코드
            /// </summary>
            [DataMember]
            public string inboundCode { get; set; }

            /// <summary>
            /// 시작일 (YYYY-MM-DD)
            /// </summary>
            [DataMember, Required]
            public string startDate { get; set; } = string.Empty;

            /// <summary>
            /// 종료일 (YYYY-MM-DD)
            /// </summary>
            [DataMember, Required]
            public string endDate { get; set; } = string.Empty;

            /// <summary>
            /// 예약 ID
            /// </summary>
            [DataMember]
            public string? reservationId { get; set; }

            /// <summary>
            /// 예약 상태
            /// </summary>
            [DataMember]
            public int? status { get; set; }
        }

        /// <summary>
        /// 예약 요청
        /// </summary>
        [DataContract]
        public class ReqBookingRequest : GolfClubCodeBase, IInboundCode
        {
            /// <summary>
            /// 인바운드 코드
            /// </summary>
            [DataMember, Required]
            public string inboundCode { get; set; }

            /// <summary>
            /// 공급사 코드
            /// </summary>
            [DataMember]
            public string? supplierCode { get; set; }

            /// <summary>
            /// 코스 코드
            /// </summary>
            [DataMember, Required]
            public string courseCode { get; set; }

            /// <summary>
            /// 예약 요청일 (YYYY-MM-DD)
            /// </summary>
            [DataMember]
            public string reservationDate { get; set; }

            /// <summary>
            /// 시작 시간 (HHMM)
            /// </summary>
            [DataMember]
            public string reservationStartTime { get; set; }

            /// <summary>
            /// 플레이어 수
            /// </summary>
            [DataMember]
            public int reservationMembers { get; set; }

            /// <summary>
            /// 화폐 단위
            /// </summary>
            [DataMember]
            public string currency { get; set; }

            /// <summary>
            /// 총 요금
            /// </summary>
            [DataMember]
            public decimal totalPrice { get; set; }

            /// <summary>
            /// 예약자 이름
            /// </summary>
            [DataMember]
            public string holderName { get; set; }

            /// <summary>
            /// 예약자 연락처
            /// </summary>
            [DataMember]
            public string reservationPhone { get; set; }

            /// <summary>
            /// 예약자 이메일
            /// </summary>
            [DataMember]
            public string reservationEmail { get; set; }

            /// <summary>
            /// 예약자 국적
            /// </summary>
            [DataMember]
            public string reservationCountry { get; set; }

            /// <summary>
            /// 내장객 정보 리스트
            /// </summary>
            [DataMember]
            public List<GuestInfo>? guestInfo { get; set; }
        }

        /// <summary>
        /// 취소 요청
        /// </summary>
        [DataContract]
        public class cancelRequest : IReservationBase
        {
            /// <summary>
            /// 예약 ID
            /// </summary>
            [DataMember]
            [Required]
            public string? reservationId { get; set; }

            /// <summary>
            /// 취소 날짜
            /// </summary>
            [DataMember]
            public DateTime? cancelDate { get; set; }

            /// <summary>
            /// 취소 패널티 금액
            /// </summary>
            [DataMember]
            //[JsonConverter(typeof(DecimalOrStringConverter))]
            public decimal? cancelPenaltyAmount { get; set; }

            /// <summary>
            /// 통화
            /// </summary>
            [DataMember]
            public string? currency { get; set; }
        }

        /// <summary>
        /// 인증 요청 및 조회
        /// </summary>
        [DataContract]
        public class AuthenticationRequest
        {
            /// <summary>
            /// 생성 코드 입력값 없을시 SUP + 고유 랜덤8자리 문자열
            /// </summary>
            [DataMember]
            public string? authCode { get; set; }
            /// <summary>
            /// 생성 이름
            /// </summary>
            [DataMember]
            public string? authName { get; set; }
            /// <summary>
            /// 생성 구분 (1: 공급사, 2: 클라이언트)
            /// </summary>
            [DataMember, Required]
            public string authType { get; set; }

            /// <summary>
            /// 엔드포인트
            /// </summary>
            [DataMember]
            public string? endPoint { get; set; }
        }

        /// <summary>
        /// 인증 체크 요청
        /// </summary>
        [DataContract]
        public class CheckAuthenticationRequest
        {
            /// <summary>
            /// 클라이언트 토큰
            /// </summary>
            [DataMember]
            [FromHeader(Name = "token"), Required]
            public string token { get; set; }

            /// <summary>
            /// 클라이언트 코드
            /// </summary>
            [DataMember]
            [FromHeader(Name = "ClientCode"), Required]
            public string clientCode { get; set; }
        }
    }
}

using System.Runtime.Serialization;
using AGL.Api.ApplicationCore.Interfaces;
using System.Text.Json.Serialization;
using AGL.Api.ApplicationCore.Utilities;
using static AGL.Api.Bridge_API.Models.OAPI.OAPI;
using static AGL.Api.Bridge_API.Models.OAPI.OAPIRequest;
using System.ComponentModel.DataAnnotations;

namespace AGL.Api.Bridge_API.Models.OAPI
{
    public class OAPIResponse
    {
        /// <summary>
        /// 기본 응답
        /// </summary>
        [DataContract]
        public class OAPIResponseBase : IDataResult
        {
            /// <summary>
            /// 성공여부
            /// </summary>
            [DataMember(Name = "isSuccess")]
            [JsonPropertyName("success")]
            public bool isSuccess { get; set; }

            /// <summary>
            /// 응답 코드
            /// </summary>
            [DataMember]
            public string rstCd { get; set; }

            /// <summary>
            /// 응답 숫자 코드
            /// </summary>
            [DataMember]
            public int statusCode { get; set; }

            /// <summary>
            /// 응답 메세지
            /// </summary>
            [DataMember(Name = "rstMsg")]
            [JsonPropertyName("message")]
            public string rstMsg { get; set; }
        }

        /// <summary>
        /// 예약요청 응답
        /// </summary>
        [DataContract]
        public class OAPIReservationResponse : OAPIResponseBase
        {
            /// <summary>
            /// 예약번호
            /// </summary>
            [DataMember]
            public string reservationId { get; set; }
        }

        /// <summary>
        /// 데이타 제너릭 응답
        /// </summary>
        [DataContract]
        public class OAPIDataResponse<T> : OAPIResponseBase
        {
            /// <summary>
            /// 데이타
            /// </summary>
            [DataMember]
            public T data { get; set; }
        }

        /// <summary>
        /// 취소 응답
        /// </summary>
        [DataContract]
        public class CancelResponse
        {
            /// <summary>
            /// 취소 날짜
            /// </summary>
            [DataMember]
            public DateTime? cancelDate { get; set; }

            /// <summary>
            /// 취소 패널티 금액
            /// </summary>
            [DataMember]
            [JsonConverter(typeof(DecimalOrStringConverter))]
            public decimal? cancelPenaltyAmount { get; set; }

            /// <summary>
            /// 통화
            /// </summary>
            [DataMember]
            public string? currency { get; set; }
        }

        /// <summary>
        /// 예약 확정 목록 응답
        /// </summary>
        [DataContract]
        public class OAPIReservationConfirmListResponse : OAPIResponseBase
        {
            /// <summary>
            /// 예약 확정 목록
            /// </summary>
            [DataMember]
            public List<OAPIReservationConfirm> data { get; set; }
        }

        /// <summary>
        /// 예약 조회
        /// </summary>
        [DataContract]
        public class ReservationReponse 
        {
            /// <summary>
            /// 예약 ID
            /// </summary>
            [DataMember]
            [Required]
            public string reservationId { get; set; }
            /// <summary>
            /// 골프장 코드
            /// </summary>
            [DataMember]
            [Required]
            public string golfClubCode { get; set; }

            /// <summary>
            /// 코스 코드
            /// </summary>
            [DataMember]
            [Required]
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
            public byte? reservationMembers { get; set; }

            /// <summary>
            /// 화폐 단위
            /// </summary>
            [DataMember]
            public string currency { get; set; }

            /// <summary>
            /// 총 요금
            /// </summary>
            [DataMember]
            public decimal? totalPrice { get; set; }

            /// <summary>
            /// 예약자 이름
            /// </summary>
            [DataMember]
            public string? holderName { get; set; }

            /// <summary>
            /// 예약자 연락처
            /// </summary>
            [DataMember]
            public string? reservationPhone { get; set; }

            /// <summary>
            /// 예약자 이메일
            /// </summary>
            [DataMember]
            public string? reservationEmail { get; set; }

            /// <summary>
            /// 예약자 국적
            /// </summary>
            [DataMember]
            public string? reservationCountry { get; set; }

            /// <summary>
            /// 내장객 정보 리스트
            /// </summary>
            [DataMember]
            public List<GuestInfo>? guestInfo { get; set; }
        }

        /// <summary>
        /// 예약 확정 응답
        /// </summary>
        [DataContract]
        public class OAPIReservationConfirm
        {
            /// <summary>
            /// 예약번호
            /// </summary>
            [DataMember]
            public string? reservationId { get; set; }

            /// <summary>
            /// 골프장의 고유 코드
            /// </summary>
            [DataMember]
            public string? golfClubCode { get; set; }

            /// <summary>
            /// 날짜
            /// </summary>
            [DataMember]
            public string? playDate { get; set; }

            /// <summary>
            /// 시작 시간 HHMM (예: 0600)
            /// </summary>
            [DataMember]
            public string? startTime { get; set; }

            /// <summary>
            /// 예약 상태
            /// 1 예약요청
            /// 2 예약확정
            /// 3 예약취소
            /// </summary>
            [DataMember]
            public int status { get; set; }

            /// <summary>
            /// 예약 요청일 YYYY-MM-DD
            /// </summary>
            [DataMember]
            public string reservationDate { get; set; }
        }

        /// <summary>
        /// 인증 요청 응답
        /// </summary>
        [DataContract]
        public class authAuthenticationResponse
        {
            /// <summary>
            /// 코드
            /// </summary>
            [DataMember]
            public string authCode { get; set; }

            /// <summary>
            /// 공급사 토큰
            /// </summary>
            [DataMember]
            public string? TokenSupplier { get; set; }

            /// <summary>
            /// 클라이언트 토큰
            /// </summary>
            [DataMember]
            public string? TokenClient { get; set; }

            /// <summary>
            /// AGL 코드
            /// </summary>
            [DataMember]
            public string? AglCode { get; set; }

            /// <summary>
            /// AGL 토큰
            /// </summary>
            [DataMember]
            public string? TokenAgl { get; set; }
        }

        /// <summary>
        /// 티타임 조회 응답
        /// </summary>
        [DataContract]
        public class TeeTimeResponse : OAPIResponseBase
        {
            /// <summary>
            /// 데이타
            /// </summary>
            [DataMember]
            public TeeTimeData data { get; set; }
        }

        /// <summary>
        /// 티타임 리스트 정보
        /// </summary>
        [DataContract]
        public class TeeTimeData
        {
            /// <summary>
            /// 티타임 정보
            /// </summary>
            [DataMember]
            public List<TeeTimeInfo> teeTimeInfo { get; set; }
        }
    }
}

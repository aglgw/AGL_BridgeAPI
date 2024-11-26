using System.Runtime.Serialization;
using AGL.Api.ApplicationCore.Interfaces;
using System.Text.Json.Serialization;
using AGL.Api.ApplicationCore.Utilities;
using static AGL.Api.Bridge_API.Models.OAPI.OAPI;

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

        [DataContract]
        public class OAPIReservationResponse : OAPIResponseBase
        {
            /// <summary>
            /// 예약번호
            /// </summary>
            [DataMember]
            public string reservationId { get; set; }
        }

        [DataContract]
        public class OAPIDataResponse<T> : OAPIResponseBase
        {
            /// <summary>
            /// 데이타
            /// </summary>
            [DataMember]
            public T data { get; set; }
        }

        [DataContract]
        public class cancelResponse
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

        [DataContract]
        public class OAPIReservationConfirmListResponse : OAPIResponseBase
        {
            /// <summary>
            /// 예약 확정 목록
            /// </summary>
            [DataMember]
            public List<OAPIReservationConfirm> data { get; set; }
        }

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

        [DataContract]
        public class OAPITeeTimeGetResponse : OAPIResponseBase
        {
            /// <summary>
            /// 데이타
            /// </summary>
            [DataMember]
            public Dictionary<string, List<TeeTimeInfo>>? data { get; set; }
        }

        [DataContract]
        public class OAPICommonListResponse<T> : OAPIResponseBase
        {
            /// <summary>
            /// 데이타
            /// </summary>
            [DataMember]
            public List<T>? data { get; set; }
        }

        [DataContract]
        public class OAPICommonResponse<T> : OAPIResponseBase
        {
            /// <summary>
            /// 데이타
            /// </summary>
            [DataMember]
            public T? data { get; set; }
        }

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

    }
}

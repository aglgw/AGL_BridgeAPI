using System.Runtime.Serialization;
using static AGL.Api.Bridge_API.Models.OAPI.OAPI;
using System;
using AGL.Api.ApplicationCore.Interfaces;
using System.Text.Json.Serialization;
using AGL.Api.ApplicationCore.Utilities;
using System.ComponentModel.DataAnnotations;

namespace AGL.Api.Bridge_API.Models.OAPI
{
    public class OAPIResponse
    {
        /// <summary>
        /// 조회 응답
        /// </summary>
        [DataContract]
        public class OAPIResponseBase : IDataResult
        {
            /// <summary>
            /// 성공여부
            /// </summary>
            [DataMember]
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
            [DataMember]
            public string rstMsg { get; set; }
        }

        [DataContract]
        //public class OAPITeeTimeGetResponse<T> : OAPIResponseBase
        public class OAPIReservationResponse : IDataResult
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
            [DataMember(Name = "isSuccess")]
            [JsonPropertyName("message")]
            public string rstMsg { get; set; }
            /// <summary>
            /// 예약번호
            /// </summary>
            [DataMember]
            //public Dictionary<string, List<T>>? data { get; set; }
            public string reservationId { get; set; }
        }

        /// <summary>
        /// 예약 취소 응답
        /// </summary>
        [DataContract]
        public class OAPIDataResponse<T> : IDataResult
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
            [DataMember(Name = "isSuccess")]
            [JsonPropertyName("message")]
            public string rstMsg { get; set; }
            /// <summary>
            /// 예약번호
            /// </summary>
            [DataMember]
            //public Dictionary<string, List<T>>? data { get; set; }
            public T data { get; set; }
        }

        /// <summary>
        /// 예약 취소 응답
        /// </summary>
        public class cancelResponse
        {
            /// <summary>
            /// 데이타
            /// </summary>
            [DataMember]
            public DateTime? cancelDate { get; set; }

            /// <summary>
            /// 데이타
            /// </summary>
            [DataMember]
            [JsonConverter(typeof(DecimalOrStringConverter))]
            public decimal? cancelPenaltyAmount { get; set; }

            /// <summary>
            /// 데이타
            /// </summary>
            [DataMember]
            public string? currency { get; set; }
        }

        /// <summary>
        /// 예약 확정 조회 목록
        /// </summary>
        [DataContract]
        public class OAPIReservationConfirmListResponse : IDataResult
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
            [DataMember(Name = "isSuccess")]
            [JsonPropertyName("message")]
            public string rstMsg { get; set; }
            /// <summary>
            /// 예약번호
            /// </summary>
            [DataMember]
            //public Dictionary<string, List<T>>? data { get; set; }
            public List<OAPIReservationConfirm> data { get; set; }
        }

        /// <summary>
        /// 예약 확정 데이타
        /// </summary>
        [DataContract]
        public class OAPIReservationConfirm 
        {
            /// <summary>
            /// 예약번호
            /// </summary>
            [DataMember]
            //public Dictionary<string, List<T>>? data { get; set; }
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
            /// Booking Status
            /// 1 예약요청
            /// 2 예약확정
            /// 3 예약취소
            /// </summary>
            public int status { get; set; }
            /// <summary>
            /// 예약요청일 YYYY-MM-DD
            /// </summary>
            [DataMember]
            public string reservationDate { get; set; }
        }
            
        [DataContract]
        //public class OAPITeeTimeGetResponse<T> : OAPIResponseBase
        public class OAPITeeTimeGetResponse : OAPIResponseBase
        {
            /// <summary>
            /// 데이타
            /// </summary>
            [DataMember]
            //public Dictionary<string, List<T>>? data { get; set; }
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


    }
}

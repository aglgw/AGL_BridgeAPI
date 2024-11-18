using System.Runtime.Serialization;
using static AGL.Api.Bridge_API.Models.OAPI.OAPI;
using System;
using AGL.Api.ApplicationCore.Interfaces;
using System.Text.Json.Serialization;
using AGL.Api.ApplicationCore.Utilities;

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


        [DataContract]
        //public class OAPITeeTimeGetResponse<T> : OAPIResponseBase
        public class OAPIReservationCancelResponse : IDataResult
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
            public cancelResponse data { get; set; }
        }

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

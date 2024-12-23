using System.Runtime.Serialization;
using AGL.Api.ApplicationCore.Interfaces;
using System.Text.Json.Serialization;
using AGL.Api.ApplicationCore.Utilities;
using static AGL.Api.OTA_API.Models.OAPI.OTA;
using static AGL.Api.OTA_API.Models.OAPI.OTARequest;
using System.ComponentModel.DataAnnotations;

namespace AGL.Api.OTA_API.Models.OAPI
{
    public class OTAResponse
    {
        /// <summary>
        /// 기본 응답
        /// </summary>
        [DataContract]
        public class OTAResponseBase : IOTAResult
        {
            /// <summary>
            /// 성공여부
            /// </summary>
            [DataMember]
            public bool success { get; set; }

            /// <summary>
            /// 응답 숫자 코드
            /// </summary>
            [DataMember]
            public int statusCode { get; set; }

            /// <summary>
            /// 응답 메세지
            /// </summary>
            [DataMember]
            public string message { get; set; }
        }

        /// <summary>
        /// 기본 응답
        /// </summary>
        [DataContract]
        public class OTADataResponseBase<T> : OTAResponseBase
        {
            /// <summary>
            /// 데이타
            /// </summary>
            [DataMember]
            public T data { get; set; }
        }

    }
}

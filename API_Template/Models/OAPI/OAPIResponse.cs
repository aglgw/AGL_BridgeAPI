using System.Runtime.Serialization;
using static AGL.Api.API_Template.Models.OAPI.OAPI;
using System;
using AGL.Api.ApplicationCore.Interfaces;

namespace AGL.Api.API_Template.Models.OAPI
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
            public bool IsSuccess { get; set; }
            /// <summary>
            /// 응답 코드
            /// </summary>
            [DataMember]
            public string RstCd { get; set; }
            /// <summary>
            /// 응답 숫자 코드
            /// </summary>
            [DataMember]
            public int StatusCode { get; set; }
            /// <summary>
            /// 응답 메세지
            /// </summary>
            [DataMember]
            public string RstMsg { get; set; }
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

﻿using System.Runtime.Serialization;
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

        /// <summary>
        /// 조회 응답
        /// </summary>
        [DataContract]
        public class OAPITeeTimeGetResponse
        {
            /// <summary>
            /// 조회 응답
            /// </summary>
            [DataMember]
            public OAPIResponseBase OAPIResponseBase { get; set; }
            /// <summary>
            /// 데이타
            /// </summary>
            [DataMember]
            public Dictionary<string, List<TeeTimeInfo>> Data { get; set; }

        }

    }
}
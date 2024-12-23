using AGL.Api.ApplicationCore.Utilities;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using static AGL.Api.OTA_API.Models.OAPI.OTA;

namespace AGL.Api.OTA_API.Models.OAPI
{

    public class OTARequest
    {
        /// <summary>
        /// 요청 베이스 인터페이스
        /// </summary>
        public interface IRequestBase
        {
            /// <summary>
            /// 헤더 토큰
            /// </summary>
            [FromHeader(Name = "token"), Required]
            string Token { get; set; }

            /// <summary>
            /// 헤더 업체 코드
            /// </summary>
            [FromHeader(Name = "companyCode"), Required]
            string CompanyCode { get; set; }
        }

        /// <summary>
        /// 예약 확정 - 내부 연동용 요청
        /// </summary>
        [DataContract]
        public class OTARequestBase : IRequestBase
        {
            /// <summary>
            /// 헤더 토큰
            /// </summary>
            [FromHeader(Name = "token"), Required]
            public string Token { get; set; }

            /// <summary>
            /// 헤더 업체 코드
            /// </summary>
            [FromHeader(Name = "companyCode"), Required]
            public string CompanyCode { get; set; }
        }

    }
}

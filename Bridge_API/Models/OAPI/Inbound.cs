using static AGL.Api.Bridge_API.Models.OAPI.OAPI;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AGL.Api.Bridge_API.Models.OAPI
{
    public class Inbound
    {
        /// <summary>
        /// 내부연동 티타임정보 Response
        /// </summary>
        [DataContract]
        public class InboundTeeTimeRequest
        {
            [FromHeader(Name = "token"), Required]
            public string token { get; set; }

            [FromQuery(Name = "StartDate"), Required]
            public string startDate { get; set; }

            [FromQuery(Name = "EndDate"), Required]
            public string endDate { get; set; }

            [FromQuery(Name = "GolfclubCode"), Required]
            public string golfclubCode { get; set; }
        }

        /// <summary>
        /// 내부연동 티타임정보 Response
        /// </summary>
        [DataContract]
        public class InboundTeeTimeResponse
        {
            /// <summary>
            /// 날짜
            /// </summary>
            [DataMember]
            public string PlayDate { get; set; }
            /// <summary>
            /// 골프장 코스 코드
            /// </summary>
            [DataMember]
            public string CourseCode { get; set; }
            /// <summary>
            /// 시간 
            /// </summary>
            [DataMember]
            public string PlayTime { get; set; }
            /// <summary>
            /// 최소인원 
            /// </summary>
            [DataMember]
            public int MinMember { get; set; }
            /// <summary>
            /// 3인 일때 1인 요금
            /// </summary>
            [DataMember]
            public decimal? sumAmt_3 { get; set; }
            /// <summary>
            /// 4인 일때 1인 요금
            /// </summary>
            [DataMember]
            public decimal? sumAmt_4 { get; set; }
        }

    }
}

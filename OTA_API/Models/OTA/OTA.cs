using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace AGL.Api.OTA_API.Models.OAPI
{
    public class OTA
    {
        /// <summary>
        /// 아웃링크 
        /// </summary>
        [DataContract]
        public class Outlink 
        {
            /// <summary>
            /// 날짜
            /// </summary>
            [DataMember]
            public string? aaaa { get; set; }
        }

    }
}

using System.Runtime.Serialization;

namespace AGL.Api.API_Schedulers.Models.OAPI
{
    public class OAPIScheduler
    {
        public interface ISyncTeeTimeRequest { }

        /// <summary>
        /// 티타임 상태수정
        /// </summary>
        [DataContract]
        public class SyncTeeTimeRequest : ISyncTeeTimeRequest
        {
            /// <summary>
            /// 데몬 ID
            /// </summary>
            [DataMember]
            public string daemonId { get; set; }
            /// <summary>
            /// 골프장 코드
            /// </summary>
            [DataMember]
            public string golfClubCode { get; set; }
            /// <summary>
            /// 코스 코드
            /// </summary>
            [DataMember]
            public string courseCode { get; set; }
            /// <summary>
            /// 날짜
            /// </summary>
            [DataMember]
            public string playDate { get; set; }
            /// <summary>
            /// 시간
            /// </summary>
            [DataMember]
            public string startTime { get; set; }
            /// <summary>
            /// 가격
            /// </summary>
            [DataMember]
            public decimal? price { get; set; }
            /// <summary>
            /// 최소인원
            /// </summary>
            [DataMember]
            public int minMembers { get; set; }
            /// <summary>
            /// 판매여부
            /// </summary>
            [DataMember]
            public bool IsAvailable { get; set; }
        }
    }
}

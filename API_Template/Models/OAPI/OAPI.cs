using System.IO;
using System.Runtime.Serialization;

namespace AGL.Api.API_Template.Models.OAPI
{
    public class OAPI
    {
        /// <summary>
        /// 티타임 정보
        /// </summary>
        [DataContract]
        public class TeeTimeInfo
        {
            /// <summary>
            /// 코스 정보
            /// </summary>
            [DataMember]
            public List<string> CourseCode { get; set; }
            /// <summary>
            /// 최소인원
            /// </summary>
            [DataMember]
            public int MinMembers { get; set; }
            /// <summary>
            /// 최대인원
            /// </summary>
            [DataMember]
            public int MaxMembers { get; set; }
            /// <summary>
            /// 카트 포함 여부 ( true  포함, false  불포함 )
            /// </summary>
            [DataMember]
            public bool IncludeCart { get; set; }
            /// <summary>
            /// 캐디 포함 여부 ( true  포함, false  불포함 )
            /// </summary>
            [DataMember]
            public bool IncludeCaddie { get; set; }
            /// <summary>
            /// 예약 유형 (0: 즉시예약, 1: 대기예약 등)
            /// </summary>
            [DataMember]
            public int ReservationType { get; set; }

            /// <summary>
            /// 티타임 정보 리스트
            /// </summary>
            [DataMember]
            public List<TimeInfo> Time { get; set; }
            /// <summary>
            /// 가격 정보 리스트
            /// </summary>
            [DataMember]
            public List<PriceInfo> Price { get; set; }
            /// <summary>
            /// 환불 정책 리스트
            /// </summary>
            [DataMember]
            public List<RefundPolicy> RefundPolicy { get; set; }
        }

        /// <summary>
        /// 티타임 정보
        /// </summary>
        [DataContract]
        public class TimeInfo
        {
            /// <summary>
            /// 시작 시간 HHMM (예: 0600)
            /// </summary>
            [DataMember]
            public string StartTime { get; set; }
            /// <summary>
            /// 티타임 코드 리스트 (예: 0600-18-in, 0600-18-out)
            /// </summary>
            [DataMember]
            public List<string> TeeTimeCode { get; set; }
        }

        /// <summary>
        /// 가격 정보
        /// </summary>
        [DataContract]
        public class PriceInfo
        {
            /// <summary>
            /// 플레이어 수
            /// </summary>
            [DataMember]
            public int PlayerCount { get; set; }
            /// <summary>
            /// 그린피
            /// </summary>
            [DataMember]
            public decimal GreenFee { get; set; }
            /// <summary>
            /// 카트피
            /// </summary>
            [DataMember]
            public decimal? CartFee { get; set; }
            /// <summary>
            /// 캐디피
            /// </summary>
            [DataMember]
            public decimal? CaddyFee { get; set; }
            /// <summary>
            /// 세금
            /// </summary>
            [DataMember]
            public decimal? Tax { get; set; }
            /// <summary>
            /// 추가세금
            /// </summary>
            [DataMember]
            public decimal? AdditionalTax { get; set; }
            /// <summary>
            /// 1인 총 요금 (greenFee + cartFee + caddyFee + tax + additionalTax)
            /// </summary>
            [DataMember]
            public decimal UnitPrice { get; set; }
        }

        /// <summary>
        /// 환불 정책
        /// </summary>
        [DataContract]
        public class RefundPolicy
        {
            /// <summary>
            /// 환불 가능한 남은 일자
            /// </summary>
            [DataMember]
            public int RefundDate { get; set; }
            /// <summary>
            /// 환불 수수료
            /// </summary>
            [DataMember]
            public decimal RefundFee { get; set; }
            /// <summary>
            /// 환불 단위 (1 비율 - 특정 비율에 따라 환불 , 2 고정액 – 정해진 금액으로 환불)
            /// </summary>
            [DataMember]
            public int RefundUnit { get; set; }
        }

        /// <summary>
        /// 이미지
        /// </summary>
        [DataContract]
        public class images
        {
            /// <summary>
            /// 경로
            /// </summary>
            [DataMember]
            public string url { get; set; }
            /// <summary>
            /// 제목
            /// </summary>
            [DataMember]
            public string Title { get; set; }
            /// <summary>
            /// 이미지설명
            /// </summary>
            [DataMember]
            public string ImageDescription { get; set; }
        }

        /// <summary>
        /// 코스
        /// </summary>
        [DataContract]
        public class course
        {
            /// <summary>
            /// 코스 코드
            /// </summary>
            [DataMember]
            public string courseCode { get; set; }
            /// <summary>
            /// 코스 명
            /// </summary>
            [DataMember]
            public string courseName { get; set; }
            /// <summary>
            /// 코스 홀 수
            /// </summary>
            [DataMember]
            public int courseHoleCount { get; set; }
            /// <summary>
            /// 시작 홀
            /// </summary>
            [DataMember]
            public int startHole { get; set; }
        }

        /// <summary>
        /// 홀정보
        /// </summary>
        [DataContract]
        public class holeInfo
        {
            /// <summary>
            /// 홀 번호
            /// </summary>
            [DataMember]
            public string holeNumber { get; set; }
            /// <summary>
            /// 홀 명
            /// </summary>
            [DataMember]
            public string holeName { get; set; }
            /// <summary>
            /// 파
            /// </summary>
            [DataMember]
            public int par { get; set; }
            /// <summary>
            /// 거리 단위 ( 1 yards , 2 meters )
            /// </summary>
            [DataMember]
            public int distanceUnit { get; set; }
            /// <summary>
            /// 거리
            /// </summary>
            [DataMember]
            public int distance { get; set; }
        }
    }
}

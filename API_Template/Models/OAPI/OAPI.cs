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
            /// 날짜
            /// </summary>
            [DataMember(EmitDefaultValue = false)]
            public object? PlayDate { get; set; }
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
            public int? PlayerCount { get; set; }
            /// <summary>
            /// 그린피
            /// </summary>
            [DataMember]
            public decimal? GreenFee { get; set; }
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
            public decimal? UnitPrice { get; set; }
        }

        /// <summary>
        /// 골프장 등록/수정/조회
        /// </summary>
        [DataContract]
        public class GolfClubInfo
        {
            /// <summary>
            /// 골프장의 고유 코드
            /// </summary>
            [DataMember]
            public string GolfclubCode { get; set; }

            /// <summary>
            /// 골프장 이름
            /// </summary>
            [DataMember]
            public string GolfclubName { get; set; }

            /// <summary>
            /// ISO alpha-2 형식의 국가 코드
            /// </summary>
            [DataMember]
            public string CountryCode { get; set; }

            /// <summary>
            /// 언어 코드
            /// </summary>
            [DataMember]
            public string Language { get; set; }

            /// <summary>
            /// 통화 코드
            /// </summary>
            [DataMember]
            public string Currency { get; set; }

            /// <summary>
            /// 골프장 설명
            /// </summary>
            [DataMember]
            public string? Description { get; set; }

            /// <summary>
            /// 골프장 주소
            /// </summary>
            [DataMember]
            public string? Address { get; set; }

            /// <summary>
            /// 골프장 위치의 위도
            /// </summary>
            [DataMember]
            public string? Latitude { get; set; }

            /// <summary>
            /// 골프장 위치의 경도
            /// </summary>
            [DataMember]
            public string? Longitude { get; set; }

            /// <summary>
            /// 골프장 전화번호
            /// </summary>
            [DataMember]
            public string? Phone { get; set; }

            /// <summary>
            /// 골프장 팩스 번호
            /// </summary>
            [DataMember]
            public string? Fax { get; set; }

            /// <summary>
            /// 골프장 이메일 주소
            /// </summary>
            [DataMember]
            public string? Email { get; set; }

            /// <summary>
            /// 골프장 홈페이지 URL
            /// </summary>
            [DataMember]
            public string? Homepage { get; set; }

            /// <summary>
            /// 골프장의 총 홀 수
            /// </summary>
            [DataMember]
            public int? TotalHoleCount { get; set; }

            /// <summary>
            /// 골프장의 총 코스 수
            /// </summary>
            [DataMember]
            public int? TotalCourseCount { get; set; }

            /// <summary>
            /// 예약 시 내장객 정보 필수 여부
            /// </summary>
            [DataMember]
            public bool? IsGuestInfoRequired { get; set; } 

            /// <summary>
            /// 골프장 이미지 목록
            /// </summary>
            [DataMember]
            public List<Images>? Image { get; set; }

            /// <summary>
            /// 골프장의 환불 정책 목록
            /// </summary>
            [DataMember]
            public List<RefundPolicy>? RefundPolicy { get; set; }

            /// <summary>
            /// 골프장의 코스 목록
            /// </summary>
            [DataMember]
            public List<Course> Course { get; set; }

            /// <summary>
            /// 골프장의 홀 정보 목록
            /// </summary>
            [DataMember]
            public List<HoleInfo>? HoleInfo { get; set; }
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
            /// 환불 가능한 시간
            /// </summary>
            [DataMember]
            public string? RefundHour { get; set; }
            /// <summary>
            /// 환불 수수료
            /// </summary>
            [DataMember]
            public decimal RefundFee { get; set; }
            /// <summary>
            /// 환불 단위 (1 비율 - 특정 비율에 따라 환불 , 2 고정액 – 정해진 금액으로 환불)
            /// </summary>
            [DataMember]
            public byte RefundUnit { get; set; }
        }

        /// <summary>
        /// 이미지
        /// </summary>
        [DataContract]
        public class Images
        {
            /// <summary>
            /// 번호
            /// </summary>
            [DataMember]
            public int? id { get; set; }
            /// <summary>
            /// 경로
            /// </summary>
            [DataMember]
            public string Url { get; set; }
            /// <summary>
            /// 제목
            /// </summary>
            [DataMember]
            public string Title { get; set; }
            /// <summary>
            /// 이미지설명
            /// </summary>
            [DataMember]
            public string Description { get; set; }
        }

        /// <summary>
        /// 코스
        /// </summary>
        [DataContract]
        public class Course
        {
            /// <summary>
            /// 코스 코드
            /// </summary>
            [DataMember]
            public string CourseCode { get; set; }
            /// <summary>
            /// 코스 명
            /// </summary>
            [DataMember]
            public string CourseName { get; set; }
            /// <summary>
            /// 코스 홀 수
            /// </summary>
            [DataMember]
            public int? CourseHoleCount { get; set; }
            /// <summary>
            /// 시작 홀
            /// </summary>
            [DataMember]
            public int? StartHole { get; set; }
        }

        /// <summary>
        /// 홀정보
        /// </summary>
        [DataContract]
        public class HoleInfo
        {
            /// <summary>
            /// 홀 번호
            /// </summary>
            [DataMember]
            public int HoleNumber { get; set; }
            /// <summary>
            /// 홀 명
            /// </summary>
            [DataMember]
            public string? HoleName { get; set; }
            /// <summary>
            /// 파
            /// </summary>
            [DataMember]
            public int? Par { get; set; }
            /// <summary>
            /// 거리 단위 ( 1 yards , 2 meters )
            /// </summary>
            [DataMember]
            public int? DistanceUnit { get; set; }
            /// <summary>
            /// 거리
            /// </summary>
            [DataMember]
            public int? Distance { get; set; }
        }



        [DataContract]
        public class BookingInfo
        {
            /// <summary>
            /// Booking ID
            /// </summary>
            public string ReservationId { get; set; } = string.Empty;
            /// <summary>
            /// Golf Club Code
            /// </summary>
            public string GolfclubCode { get; set; } = string.Empty;
            /// <summary>
            /// Play Date
            /// </summary>
            public string PlayDate { get; set; } = string.Empty;
            /// <summary>
            /// Tee-off Time
            /// </summary>
            public string StartTime { get; set; } = string.Empty;
            /// <summary>
            /// Number of Players
            /// </summary>
            public int PlayerCount { get; set; }
            /// <summary>
            /// Booking Status
            /// 1 예약요청
            /// 2 예약확정
            /// 3 예약취소
            /// </summary>
            public int Status { get; set; }
            /// <summary>
            /// Booking Status
            /// </summary>
            public string OrderDate { get; set; } = string.Empty;
            /// <summary>
            /// currency
            /// </summary>
            public string? Currency { get; set; }
            /// <summary>
            /// Penalty Amount
            /// </summary>
            public decimal CancelPenaltyAmount { get; set; }


        }

        [DataContract]
        public class ConfirmBookingInfo
        {
            /// <summary>
            /// Booking ID
            /// </summary>
            public string ReservationId { get; set; } = string.Empty;
            /// <summary>
            /// Golf Club Code
            /// </summary>
            public string GolfclubCode { get; set; } = string.Empty;
            /// <summary>
            /// Play Date
            /// </summary>
            public string PlayDate { get; set; } = string.Empty;
            /// <summary>
            /// Tee-off Time
            /// </summary>
            public string StartTime { get; set; } = string.Empty;
           
            /// <summary>
            /// Booking Status
            /// 1 예약요청
            /// 2 예약확정
            /// 3 예약취소
            /// </summary>
            public int Status { get; set; }
            /// <summary>
            /// reservationDate
            /// </summary>
            public string ReservationDate { get; set; } = string.Empty;


        }

        [DataContract]
        public class CancelBookingInfo
        {
            /// <summary>
            /// cancelDate
            /// </summary>
            public string CancelDate { get; set; } = string.Empty;
            /// <summary>
            /// cancelPenaltyAmount
            /// </summary>
            public decimal CancelPenaltyAmount { get; set; } = 0;
            /// <summary>
            /// currency
            /// </summary>
            public string Currency { get; set; } = string.Empty;

        }
    }
}

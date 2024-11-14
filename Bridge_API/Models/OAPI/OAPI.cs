using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.Serialization;

namespace AGL.Api.Bridge_API.Models.OAPI
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
            public object? playDate { get; set; }
            /// <summary>
            /// 코스 정보
            /// </summary>
            [DataMember]
            [Required(ErrorMessage = "CourseCode is required")]
            public List<string> courseCode { get; set; }
            /// <summary>
            /// 최소인원
            /// </summary>
            [DataMember]
            [Required(ErrorMessage = "MinMembers is required")]
            public int minMembers { get; set; }
            /// <summary>
            /// 최대인원
            /// </summary>
            [DataMember]
            [Required(ErrorMessage = "MaxMembers is required")]
            public int maxMembers { get; set; }
            /// <summary>
            /// 카트 포함 여부 ( true  포함, false  불포함 )
            /// </summary>
            [DataMember]
            [Required(ErrorMessage = "IncludeCart is required")]
            public bool includeCart { get; set; }
            /// <summary>
            /// 캐디 포함 여부 ( true  포함, false  불포함 )
            /// </summary>
            [DataMember]
            [Required(ErrorMessage = "IncludeCaddie is required")]
            public bool includeCaddie { get; set; }
            /// <summary>
            /// 예약 유형 (0: 즉시예약, 1: 대기예약 등)
            /// </summary>
            [DataMember]
            [Required(ErrorMessage = "ReservationType is required")]
            public int reservationType { get; set; }

            /// <summary>
            /// 티타임 정보 리스트
            /// </summary>
            [DataMember]
            [Required(ErrorMessage = "Time is required")]
            public List<TimeInfo> time { get; set; }
            /// <summary>
            /// 가격 정보 리스트
            /// </summary>
            [DataMember]
            [Required(ErrorMessage = "Price is required")]
            public List<PriceInfo> price { get; set; }
            /// <summary>
            /// 환불 정책 리스트
            /// </summary>
            [DataMember]
            public List<RefundPolicy> refundPolicy { get; set; }
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
            [Required(ErrorMessage = "StartTime is required")]
            public string startTime { get; set; }
            /// <summary>
            /// 티타임 코드 리스트 (예: 0600-18-in, 0600-18-out)
            /// </summary>
            [DataMember]
            public List<string> teeTimeCode { get; set; }
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
            [Required(ErrorMessage = "PlayerCount is required")]
            public int? playerCount { get; set; }
            /// <summary>
            /// 그린피
            /// </summary>
            [DataMember]
            [Required(ErrorMessage = "GreenFee is required")]
            public decimal? greenFee { get; set; }
            /// <summary>
            /// 카트피
            /// </summary>
            [DataMember]
            public decimal? cartFee { get; set; }
            /// <summary>
            /// 캐디피
            /// </summary>
            [DataMember]
            public decimal? caddyFee { get; set; }
            /// <summary>
            /// 세금
            /// </summary>
            [DataMember]
            public decimal? tax { get; set; }
            /// <summary>
            /// 추가세금
            /// </summary>
            [DataMember]
            public decimal? additionalTax { get; set; }
            /// <summary>
            /// 1인 총 요금 (greenFee + cartFee + caddyFee + tax + additionalTax)
            /// </summary>
            [DataMember]
            [Required(ErrorMessage = "UnitPrice is required")]
            public decimal? unitPrice { get; set; }
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
            [Required(ErrorMessage = "GolfclubCode is required")]
            public string golfClubCode { get; set; }

            /// <summary>
            /// 골프장 이름
            /// </summary>
            [DataMember]
            [Required(ErrorMessage = "GolfclubName is required")]
            public string golfClubName { get; set; }

            /// <summary>
            /// ISO alpha-2 형식의 국가 코드
            /// </summary>
            [DataMember]
            [Required(ErrorMessage = "CountryCode is required")]
            public string countryCode { get; set; }

            /// <summary>
            /// 언어 코드
            /// </summary>
            [DataMember]
            [Required(ErrorMessage = "Language is required")]
            public string language { get; set; }

            /// <summary>
            /// 통화 코드
            /// </summary>
            [DataMember]
            [Required(ErrorMessage = "Currency is required")]
            public string currency { get; set; }

            /// <summary>
            /// 골프장 설명
            /// </summary>
            [DataMember]
            public string? description { get; set; }

            /// <summary>
            /// 골프장 주소
            /// </summary>
            [DataMember]
            public string? address { get; set; }

            /// <summary>
            /// 골프장 위치의 위도
            /// </summary>
            [DataMember]
            [Required(ErrorMessage = "Latitude is required")]
            public string? latitude { get; set; }

            /// <summary>
            /// 골프장 위치의 경도
            /// </summary>
            [DataMember]
            [Required(ErrorMessage = "Longitude is required")]
            public string? longitude { get; set; }

            /// <summary>
            /// 골프장 전화번호
            /// </summary>
            [DataMember]
            public string? phone { get; set; }

            /// <summary>
            /// 골프장 팩스 번호
            /// </summary>
            [DataMember]
            public string? fax { get; set; }

            /// <summary>
            /// 골프장 이메일 주소
            /// </summary>
            [DataMember]
            public string? email { get; set; }

            /// <summary>
            /// 골프장 홈페이지 URL
            /// </summary>
            [DataMember]
            public string? homepage { get; set; }

            /// <summary>
            /// 골프장의 총 홀 수
            /// </summary>
            [DataMember]
            public int? totalHoleCount { get; set; }

            /// <summary>
            /// 골프장의 총 코스 수
            /// </summary>
            [DataMember]
            public int? totalCourseCount { get; set; }

            /// <summary>
            /// 예약 시 내장객 정보 필수 여부
            /// </summary>
            [DataMember]
            [Required(ErrorMessage = "IsGuestInfoRequired is required")]
            public bool? isGuestInfoRequired { get; set; } 

            /// <summary>
            /// 골프장 이미지 목록
            /// </summary>
            [DataMember]
            public List<Images>? image { get; set; }

            /// <summary>
            /// 골프장의 환불 정책 목록
            /// </summary>
            [DataMember]
            public List<RefundPolicy>? refundPolicy { get; set; }

            /// <summary>
            /// 골프장의 코스 목록
            /// </summary>
            [DataMember]
            [Required(ErrorMessage = "Course is required")]
            public List<Course> course { get; set; }

            /// <summary>
            /// 골프장의 홀 정보 목록
            /// </summary>
            [DataMember]
            public List<HoleInfo>? holeInfo { get; set; }
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
            [Required(ErrorMessage = "RefundDate is required")]
            public int refundDate { get; set; }
            /// <summary>
            /// 환불 가능한 시간
            /// </summary>
            [DataMember]
            public string? refundHour { get; set; }
            /// <summary>
            /// 환불 수수료
            /// </summary>
            [DataMember]
            [Required(ErrorMessage = "RefundFee is required")]
            public decimal refundFee { get; set; }
            /// <summary>
            /// 환불 단위 (1 비율 - 특정 비율에 따라 환불 , 2 고정액 – 정해진 금액으로 환불)
            /// </summary>
            [DataMember]
            [Required(ErrorMessage = "RefundUnit is required")]
            public byte refundUnit { get; set; }
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
            [Required(ErrorMessage = "Images id is required")]
            public int? id { get; set; }
            /// <summary>
            /// 경로
            /// </summary>
            [DataMember]
            [Required(ErrorMessage = "Url is required")]
            public string url { get; set; }
            /// <summary>
            /// 제목
            /// </summary>
            [DataMember]
            public string? title { get; set; }
            /// <summary>
            /// 이미지설명
            /// </summary>
            [DataMember]
            public string? description { get; set; }
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
            [Required(ErrorMessage = "CourseCode is required")]
            public string courseCode { get; set; }
            /// <summary>
            /// 코스 명
            /// </summary>
            [DataMember]
            [Required(ErrorMessage = "CourseName is required")]
            public string courseName { get; set; }
            /// <summary>
            /// 코스 홀 수
            /// </summary>
            [DataMember]
            public int? courseHoleCount { get; set; }
            /// <summary>
            /// 시작 홀
            /// </summary>
            [DataMember]
            public int? startHole { get; set; }
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
            [Required(ErrorMessage = "HoleNumber is required")]
            public int holeNumber { get; set; }
            /// <summary>
            /// 홀 명
            /// </summary>
            [DataMember]
            public string? holeName { get; set; }
            /// <summary>
            /// 파
            /// </summary>
            [DataMember]
            public int? par { get; set; }
            /// <summary>
            /// 거리 단위 ( 1 yards , 2 meters )
            /// </summary>
            [DataMember]
            public int? distanceUnit { get; set; }
            /// <summary>
            /// 거리
            /// </summary>
            [DataMember]
            public int? distance { get; set; }
        }



        [DataContract]
        public class BookingInfo
        {
            /// <summary>
            /// Booking ID
            /// </summary>
            public string reservationId { get; set; } = string.Empty;
            /// <summary>
            /// Golf Club Code
            /// </summary>
            public string golfClubCode { get; set; } = string.Empty;
            /// <summary>
            /// Play Date
            /// </summary>
            public string playDate { get; set; } = string.Empty;
            /// <summary>
            /// Tee-off Time
            /// </summary>
            public string startTime { get; set; } = string.Empty;
            /// <summary>
            /// Number of Players
            /// </summary>
            public int playerCount { get; set; }
            /// <summary>
            /// Booking Status
            /// 1 예약요청
            /// 2 예약확정
            /// 3 예약취소
            /// </summary>
            public int status { get; set; }
            /// <summary>
            /// Booking Status
            /// </summary>
            public string orderDate { get; set; } = string.Empty;
            /// <summary>
            /// currency
            /// </summary>
            public string? currency { get; set; }
            /// <summary>
            /// Penalty Amount
            /// </summary>
            public decimal cancelPenaltyAmount { get; set; }


        }

        [DataContract]
        public class ConfirmBookingInfo
        {
            /// <summary>
            /// Booking ID
            /// </summary>
            public string reservationId { get; set; } = string.Empty;
            /// <summary>
            /// Golf Club Code
            /// </summary>
            public string golfClubCode { get; set; } = string.Empty;
            /// <summary>
            /// Play Date
            /// </summary>
            public string playDate { get; set; } = string.Empty;
            /// <summary>
            /// Tee-off Time
            /// </summary>
            public string startTime { get; set; } = string.Empty;
           
            /// <summary>
            /// Booking Status
            /// 1 예약요청
            /// 2 예약확정
            /// 3 예약취소
            /// </summary>
            public int status { get; set; }
            /// <summary>
            /// reservationDate
            /// </summary>
            public string reservationDate { get; set; } = string.Empty;


        }

        [DataContract]
        public class CancelBookingInfo
        {
            /// <summary>
            /// cancelDate
            /// </summary>
            public string cancelDate { get; set; } = string.Empty;
            /// <summary>
            /// cancelPenaltyAmount
            /// </summary>
            public decimal cancelPenaltyAmount { get; set; } = 0;
            /// <summary>
            /// currency
            /// </summary>
            public string currency { get; set; } = string.Empty;

        }
    }
}

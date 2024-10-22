namespace AGL.Api.API_Template.Models.GolfClub
{
    public class GolfClub
    {
        public class OAPI_GolfClubHole
        {
            public int GolfClubHoleId { get; set; } // 홀정보ID (PK)
            public int? GolfClubId { get; set; } // 골프장ID (FK)
            public int HoleNumber { get; set; } // 홀번호
            public int Par { get; set; } // 타수
            public int DistanceUnit { get; set; } // 거리단위
            public int? Distance { get; set; } // 거리
            public DateTime CreatedDate { get; set; } // 생성일
            public DateTime? UpdatedDate { get; set; } // 수정일
        }

        public class OAPI_GolfClubRefundPolicy
        {
            public int GolfClubRefundPolicyId { get; set; } // 환불정책ID (PK)
            public int? GolfClubId { get; set; } // 골프장ID (FK)
            public string RefundDate { get; set; } // 환불잔여일
            public string? RefundHour { get; set; } // 환불시간
            public decimal? RefundFee { get; set; } // 환불수수료
            public byte? RefundUnit { get; set; } // 환불단위
            public DateTime CreatedDate { get; set; } // 생성일
            public DateTime? UpdatedDate { get; set; } // 수정일
        }

        public class OAPI_GolfClub
        {
            public int GolfClubId { get; set; } // 골프장ID (PK)
            public int? SupplierId { get; set; } // 공급자ID (FK)
            public string GolfClubCode { get; set; } // 골프장코드
            public string GolfClubName { get; set; } // 골프장명
            public string CountryCode { get; set; } // 국가코드
            public string Currency { get; set; } // 통화
            public string? Description { get; set; } // 설명
            public string? Address { get; set; } // 주소
            public string? Latitude { get; set; } // 위도
            public string? Longitude { get; set; } // 경도
            public int? HoleCount { get; set; } // 홀 갯수
            public int? CourseCount { get; set; } // 코스 갯수
            public string? Phone { get; set; } // 전화번호
            public string? Fax { get; set; } // 팩스
            public string? Email { get; set; } // 이메일
            public string? Homepage { get; set; } // 홈페이지
            public byte? IsResidentGuestRequired { get; set; } // 내장객 예약시 필수 여부
            public int ReservationType { get; set; } // 예약종류
            public bool? TeetimeCancelIsActive { get; set; } // 티타임 취소시 활성화여부
            public DateTime CreatedDate { get; set; } // 생성일
            public DateTime? UpdatedDate { get; set; } // 수정일
        }

        public class OAPI_GolfClubRefund
        {
            public int GolfClubRefundId { get; set; } // 취소정책ID (PK)
            public int GolfClubId { get; set; } // 골프장ID (FK)
            public int? RefundDate { get; set; } // 취소일자
            public int RefundFee { get; set; } // 취소수수료
            public byte? RefundUnit { get; set; } // 취소단위
            public DateTime CreatedDate { get; set; } // 생성일
            public DateTime? UpdatedDate { get; set; } // 수정일
        }

        public class OAPI_GolfClubImage
        {
            public int GolfClubImageId { get; set; } // 이미지 번호 (PK)
            public int GolfClubId { get; set; } // 골프장ID (FK)
            public int Idx { get; set; } // 번호
            public string Url { get; set; } // 경로
            public string? Title { get; set; } // 제목
            public string? ImageDescription { get; set; } // 이미지설명
            public DateTime CreatedDate { get; set; } // 생성일
            public DateTime? UpdatedDate { get; set; } // 수정일
        }

        public class OAPI_GolfClubCourse
        {
            public int GolfClubCourseId { get; set; } // 코스ID (PK)
            public int GolfClubId { get; set; } // 골프장ID (FK)
            public string? CourseCode { get; set; } // 코스 코드
            public string? CourseName { get; set; } // 코스명
            public DateTime CreatedDate { get; set; } // 생성일
            public DateTime? UpdatedDate { get; set; } // 수정일
        }

        public class OAPI_Supplier
        {
            public int SupplierId { get; set; } // 공급자ID (PK)
            public string FieldId { get; set; } // 공급자필드ID
            public string SupplierCode { get; set; } // 공급자 코드
            public string TokenSupplierToAgl { get; set; } // 공급자 발급토큰
            public string AglCode { get; set; } // AGL 코드
            public string TokenAglToSupplier { get; set; } // AGL 발급토큰
            public string EndPointDev { get; set; } // 개발 엔드포인트
            public string EndPointProd { get; set; } // 상용 엔드포인트

            public DateTime CreatedDate { get; set; } // 생성일
        }


    }
}

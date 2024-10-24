
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities.OAPI
{
    public class OAPI_GolfClub
    {
        [Key]
        [Required]
        public int GolfClubId { get; set; } // 골프장ID (PK)

        [Required]
        [ForeignKey("OAPI_Supplier")]
        public int? SupplierId { get; set; } // 공급자ID (FK)

        [Required, StringLength(100)]
        public string GolfClubCode { get; set; } // 골프장코드

        [Required, StringLength(512)]
        public string GolfClubName { get; set; } // 골프장명

        [Required, StringLength(3)]
        public string CountryCode { get; set; } // 국가코드

        [Required, StringLength(3)]
        public string Currency { get; set; } // 통화

        [StringLength(2048)]
        public string? Description { get; set; } // 설명

        [StringLength(2048)]
        public string? Address { get; set; } // 주소

        [StringLength(16)]
        public string? Latitude { get; set; } // 위도

        [StringLength(16)]
        public string? Longitude { get; set; } // 경도

        public int? HoleCount { get; set; } // 홀 갯수

        public int? CourseCount { get; set; } // 코스 갯수

        [StringLength(32)]
        public string? Phone { get; set; } // 전화번호

        [StringLength(32)]
        public string? Fax { get; set; } // 팩스

        [StringLength(100)]
        public string? Email { get; set; } // 이메일

        [StringLength(200)]
        public string? Homepage { get; set; } // 홈페이지

        public byte? IsResidentGuestRequired { get; set; } // 내장객 예약시 필수 여부

        [Required]
        public int ReservationType { get; set; } // 예약종류

        public bool? TeetimeCancelIsActive { get; set; } // 티타임 취소시 활성화여부

        [Required]
        public DateTime CreatedDate { get; set; } // 생성일

        public DateTime? UpdatedDate { get; set; } // 수정일

        // 네비게이션 속성
        [JsonIgnore]
        public virtual OAPI_Supplier Supplier { get; set; }

        [JsonIgnore]
        public virtual ICollection<OAPI_GolfClubImage> GolfClubImages { get; set; } = new List<OAPI_GolfClubImage>();

        [JsonIgnore]
        public virtual ICollection<OAPI_GolfClubRefundPolicy> RefundPolicies { get; set; } = new List<OAPI_GolfClubRefundPolicy>();

        [JsonIgnore]
        public virtual ICollection<OAPI_GolfClubCourse> Courses { get; set; } = new List<OAPI_GolfClubCourse>();

        [JsonIgnore]
        public virtual ICollection<OAPI_GolfClubHole> Holes { get; set; } = new List<OAPI_GolfClubHole>();

        [JsonIgnore]
        public virtual ICollection<OAPI_TeetimeRefundPolicy> TeetimeRefundPolicies { get; set; } = new List<OAPI_TeetimeRefundPolicy>();

        [JsonIgnore]
        public virtual ICollection<OAPI_TeetimePriceMapping> TeetimePriceMappings { get; set; } = new List<OAPI_TeetimePriceMapping>();

        [JsonIgnore]
        public virtual ICollection<OAPI_TeeTimeMapping> TeeTimeMappings { get; set; } = new List<OAPI_TeeTimeMapping>();
    }
}

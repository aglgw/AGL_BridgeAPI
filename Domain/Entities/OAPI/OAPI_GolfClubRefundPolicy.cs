using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities.OAPI
{

    public class OAPI_GolfClubRefundPolicy
    {
        [Key]
        [Required]
        public int GolfClubRefundPolicyId { get; set; } // 환불정책ID (PK)

        [Required]
        [ForeignKey("OAPI_GolfClub")]
        public int? GolfClubId { get; set; } // 골프장ID (FK)

        [Required, StringLength(8)]
        public string RefundDate { get; set; } // 환불잔여일

        [StringLength(4)]
        public string? RefundHour { get; set; } // 환불시간

        public decimal? RefundFee { get; set; } // 환불수수료

        public byte? RefundUnit { get; set; } // 환불단위

        [Required]
        public DateTime CreatedDate { get; set; } // 생성일

        public DateTime? UpdatedDate { get; set; } // 수정일

        // 네비게이션 속성
        [JsonIgnore]
        public virtual OAPI_GolfClub GolfClub { get; set; }
    }
}

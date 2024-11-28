using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace AGL.Api.Domain.Entities.OAPI
{
    public class OAPI_TeeTimeMapping
    {
        [Key]
        [Required]
        public int TeetimeMappingid { get; set; }

        [Required]
        [ForeignKey("OAPI_TeeTime")]
        public int TeetimeId { get; set; }

        [Required]
        [ForeignKey("OAPI_DateSlot")]
        public int DateSlotId { get; set; }

        [Required]
        [ForeignKey("OAPI_TimeSlot")]
        public int TimeSlotId { get; set; }

        [Required]
        [ForeignKey("OAPI_TeetimePricePolicy")]
        public int PricePolicyId { get; set; }

        [ForeignKey("OAPI_TeetimeRefundPolicy")]
        public int? RefundPolicyId { get; set; }

        [StringLength(100)]
        public string? SupplierTeetimeCode { get; set; }

        [Required]
        public bool IsAvailable { get; set; } // 판매 여부

        [Required]
        public bool IsDisable { get; set; } // 노출 여부

        [Required]
        public bool IsDeleted { get; set; } // 삭제 여부

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }


        // 새로운 읽기 전용 속성으로 PlayDate 추가
        [NotMapped] // 데이터베이스에 저장하지 않음
        public string? PlayDate => DateSlot?.PlayDate;
        [NotMapped] // 데이터베이스에 저장하지 않음
        public string? StartTime => TimeSlot?.StartTime;

        //네비게이션 설정
        [JsonIgnore]
        public virtual OAPI_TeeTime TeeTime { get; set; }
        [JsonIgnore]
        public virtual OAPI_DateSlot DateSlot { get; set; }
        [JsonIgnore]
        public virtual OAPI_TimeSlot TimeSlot { get; set; }
        [JsonIgnore]
        public virtual OAPI_TeetimePricePolicy TeetimePricePolicy { get; set; }
        [JsonIgnore]
        public virtual OAPI_TeetimeRefundPolicy TeetimeRefundPolicy { get; set; }
    }
}

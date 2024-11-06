using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AGL.Api.Domain.Entities.OAPI
{
    public class OAPI_TeetimeRefundPolicy
    {
        [Key]
        [Required]
        public int RefundPolicyId { get; set; }

        // 첫 번째 환불 조건
        public int? RefundDate_1 { get; set; } // 환불잔여일_1
        [StringLength(4)]
        public string? RefundHour_1 { get; set; } // 환불시간_1
        public decimal? RefundFee_1 { get; set; } // 환불수수료_1
        public byte? RefundUnit_1 { get; set; } // 환불단위_1

        // 두 번째 환불 조건
        public int? RefundDate_2 { get; set; } // 환불잔여일_2
        [StringLength(4)]
        public string? RefundHour_2 { get; set; } // 환불시간_2
        public decimal? RefundFee_2 { get; set; } // 환불수수료_2
        public byte? RefundUnit_2 { get; set; } // 환불단위_2

        // 세 번째 환불 조건
        public int? RefundDate_3 { get; set; } // 환불잔여일_3
        [StringLength(4)]
        public string? RefundHour_3 { get; set; } // 환불시간_3
        public decimal? RefundFee_3 { get; set; } // 환불수수료_3
        public byte? RefundUnit_3 { get; set; } // 환불단위_3

        // 네 번째 환불 조건
        public int? RefundDate_4 { get; set; } // 환불잔여일_4
        [StringLength(4)]
        public string? RefundHour_4 { get; set; } // 환불시간_4
        public decimal? RefundFee_4 { get; set; } // 환불수수료_4
        public byte? RefundUnit_4 { get; set; } // 환불단위_4

        // 다섯 번째 환불 조건
        public int? RefundDate_5 { get; set; } // 환불잔여일_5
        [StringLength(4)]
        public string? RefundHour_5 { get; set; } // 환불시간_5
        public decimal? RefundFee_5 { get; set; } // 환불수수료_5
        public byte? RefundUnit_5 { get; set; } // 환불단위_5

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        //네비게이션 속성
        [JsonIgnore]
        public virtual ICollection<OAPI_TeeTimeMapping> TeeTimeMappings { get; set; } = new List<OAPI_TeeTimeMapping>();
    }
}

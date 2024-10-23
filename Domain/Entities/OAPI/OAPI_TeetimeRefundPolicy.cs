using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AGL.Api.Domain.Entities.OAPI
{
    public class OAPI_TeetimeRefundPolicy
    {
        [Key]
        [Required]
        public int RefundPolicyId { get; set; }

        [Required]
        [StringLength(8)]
        public string RefundDate { get; set; }

        [StringLength(4)]
        public string RefundHour { get; set; }

        [Required]
        public decimal RefundFee { get; set; }

        [Required]
        public byte RefundUnit { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        //네비게이션 속성
        [JsonIgnore]
        public virtual ICollection<OAPI_TeetimeRefundMapping> TeetimeRefundMappings { get; set; } = new List<OAPI_TeetimeRefundMapping>();

    }
}

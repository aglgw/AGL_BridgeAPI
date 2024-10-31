using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AGL.Api.Domain.Entities.OAPI
{
    public class OAPI_PricePolicy
    {
        [Key]
        [Required]
        public int PricePolicyId { get; set; }

        [Required]
        public int PlayerCount { get; set; }

        [Required]
        public decimal GreenFee { get; set; }

        public decimal? CartFee { get; set; }

        public decimal? CaddyFee { get; set; }

        public decimal? Tax { get; set; }

        public decimal? AdditionalTax { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}

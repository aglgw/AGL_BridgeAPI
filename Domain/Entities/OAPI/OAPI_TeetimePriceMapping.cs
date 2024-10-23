using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace AGL.Api.Domain.Entities.OAPI
{
    public class OAPI_TeetimePriceMapping
    {
        [Required]
        [ForeignKey("OAPI_DateTimeMapping")]
        public int DateTimeMappingId { get; set; }

        [Required]
        [ForeignKey("OAPI_PricePolicy")]
        public int? PricePolicyId { get; set; }

        //네비게이션 속성
        [JsonIgnore]
        public virtual OAPI_DateTimeMapping DateTimeMapping { get; set; }

        [JsonIgnore]
        public virtual OAPI_PricePolicy PricePolicy { get; set; }
    }
}

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace AGL.Api.Domain.Entities.OAPI
{
    //[Keyless]
    public class OAPI_TeetimePriceMapping
    {
        [Required]
        [ForeignKey("OAPI_DateTimeMapping")]
        public int TeeTimeMappingId { get; set; }

        [Required]
        [ForeignKey("OAPI_PricePolicy")]
        public int? PricePolicyId { get; set; }

        //네비게이션 속성
        [JsonIgnore]
        public virtual OAPI_TeeTimeMapping TeeTimeMapping { get; set; }

        [JsonIgnore]
        public virtual OAPI_PricePolicy PricePolicy { get; set; }
    }
}

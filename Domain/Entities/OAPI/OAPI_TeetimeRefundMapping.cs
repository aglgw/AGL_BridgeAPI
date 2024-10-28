using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace AGL.Api.Domain.Entities.OAPI
{
    public class OAPI_TeetimeRefundMapping
    {
        [Required]
        [ForeignKey("OAPI_DateTimeMapping")]
        public int TeeTimeMappingId { get; set; }

        [Required]
        [ForeignKey("OAPI_TeetimeRefundPolicy")]
        public int RefundPolicyId { get; set; }

        //네비게이션 속성
        [JsonIgnore]
        public virtual OAPI_TeeTimeMapping TeeTimeMapping { get; set; }

        [JsonIgnore]
        public virtual OAPI_TeetimeRefundPolicy TeetimeRefundPolicy { get; set; }

    }
}

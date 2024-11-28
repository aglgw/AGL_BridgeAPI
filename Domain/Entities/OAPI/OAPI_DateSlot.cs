using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace AGL.Api.Domain.Entities.OAPI
{
    public class OAPI_DateSlot
    {
        [Key]
        [Required]
        public int DateSlotId { get; set; }

        [Required]
        public DateTime StartDate { get; set; } // 검색용

        [Required]
        [StringLength(8)]
        public string PlayDate { get; set; } // 출력용

        [JsonIgnore]
        public virtual ICollection<OAPI_TeeTimeMapping> TeeTimeMappings { get; set; } = new List<OAPI_TeeTimeMapping>();
    }
}

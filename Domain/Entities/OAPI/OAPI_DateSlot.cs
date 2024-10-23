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
        [StringLength(8)]
        public string PlayDate { get; set; }

        [JsonIgnore]
        public virtual ICollection<OAPI_DateTimeMapping> DateTimeMappings { get; set; } = new List<OAPI_DateTimeMapping>();
    }
}

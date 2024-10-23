using System.ComponentModel.DataAnnotations;

namespace AGL.Api.Domain.Entities.OAPI
{
    public class OAPI_TimeSlot
    {
        [Key]
        [Required]
        public int TimeSlotId { get; set; }

        [Required]
        [StringLength(4)]
        public string StartTime { get; set; }

        //네비게이션 속성
        public virtual ICollection<OAPI_DateTimeMapping> DateTimeMappings { get; set; } = new List<OAPI_DateTimeMapping>();
    }
}

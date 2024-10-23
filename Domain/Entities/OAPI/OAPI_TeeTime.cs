using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities.OAPI
{
    public class OAPI_TeeTime
    {
        [Key]
        [Required]
        public int TeetimeId { get; set; }

        [Required]
        [ForeignKey("OAPI_Supplier")]
        public int SupplierId { get; set; }

        [Required]
        [ForeignKey("OAPI_GolfClub")]
        public int GolfClubId { get; set; }

        [Required]
        [ForeignKey("OAPI_GolfClubCourse")]
        public int GolfClubCourseId { get; set; }

        [Required]
        public int MinMembers { get; set; }

        [Required]
        public int MaxMembers { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        //네비게이션 속성
        [JsonIgnore]
        public virtual OAPI_Supplier Supplier { get; set; }

        [JsonIgnore]
        public virtual OAPI_GolfClub GolfClub { get; set; }

        [JsonIgnore]
        public virtual OAPI_GolfClubCourse GolfClubCourse { get; set; }

        [JsonIgnore]
        public virtual ICollection<OAPI_DateTimeMapping> DateTimeMappings { get; set; } = new List<OAPI_DateTimeMapping>();

    }
}

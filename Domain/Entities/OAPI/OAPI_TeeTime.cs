using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

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
        public bool IncludeCart { get; set; } // 카트포함여부

        [Required]
        public bool IncludeCaddie { get; set; } // 캐디포함여부

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
        public virtual ICollection<OAPI_TeeTimeMapping> TeeTimeMappings { get; set; } = new List<OAPI_TeeTimeMapping>();

    }
}

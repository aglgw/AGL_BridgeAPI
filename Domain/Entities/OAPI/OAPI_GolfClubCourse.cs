using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace AGL.Api.Domain.Entities.OAPI
{
    public class OAPI_GolfClubCourse
    {
        [Key]
        [Required]
        public int GolfClubCourseId { get; set; } // 코스ID (PK)

        [Required]
        public int GolfClubId { get; set; } // 골프장ID (FK)

        [StringLength(255)]
        public string CourseCode { get; set; } // 코스 코드

        [StringLength(255)]
        public string CourseName { get; set; } // 코스명

        public int? CourseHoleCount { get; set; }

        public int? StartHole { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } // 생성일

        public DateTime? UpdatedDate { get; set; } // 수정일

        // 네비게이션 속성
        [JsonIgnore]
        public virtual OAPI_GolfClub GolfClub { get; set; }
        [JsonIgnore]
        public virtual ICollection<OAPI_TeeTime> TeeTimes { get; set; } = new List<OAPI_TeeTime>();
    }
}

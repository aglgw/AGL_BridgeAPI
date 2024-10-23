using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AGL.Api.Domain.Entities.OAPI
{
    public class OAPI_GolfClubImage
    {
        [Key]
        [Required]
        public int GolfClubImageId { get; set; } // 이미지 번호 (PK)

        [Required]
        public int GolfClubId { get; set; } // 골프장ID (FK)

        [Required]
        public int Idx { get; set; } // 번호

        [Required, StringLength(255)]
        public string Url { get; set; } // 경로

        [StringLength(255)]
        public string? Title { get; set; } // 제목

        [StringLength(2048)]
        public string? ImageDescription { get; set; } // 이미지설명

        [Required]
        public DateTime CreatedDate { get; set; } // 생성일

        public DateTime? UpdatedDate { get; set; } // 수정일

        // 네비게이션 속성
        [JsonIgnore]
        public virtual OAPI_GolfClub GolfClub { get; set; }
    }
}

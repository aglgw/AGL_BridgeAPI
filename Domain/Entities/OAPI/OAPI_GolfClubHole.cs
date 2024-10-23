using AGL.Api.Domain.Entities.OAPI;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class OAPI_GolfClubHole
{
    [Key]
    [Required]
    public int GolfClubHoleId { get; set; } // 홀정보ID (PK)

    [Required]
    [ForeignKey("OAPI_GolfClub")]
    public int? GolfClubId { get; set; } // 골프장ID (FK)

    [Required]
    public int HoleNumber { get; set; } // 홀번호

    [Required]
    public int Par { get; set; } // 타수

    [Required]
    public int DistanceUnit { get; set; } // 거리단위

    public int? Distance { get; set; } // 거리

    [Required]
    public DateTime CreatedDate { get; set; } // 생성일

    public DateTime? UpdatedDate { get; set; } // 수정일

    // 네비게이션 속성
    [JsonIgnore]
    public virtual OAPI_GolfClub GolfClub { get; set; }
}
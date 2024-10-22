using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities
{
    public class HTT_CODE_NATION
    {
        [Key]
        [Column(TypeName = "tinyint")]
        public byte NAT_SEQ { get; set; }
        [Key]
        public string NAT_CD { get; set; } = string.Empty;
        public string? AREA_CD { get; set; }
        public string? NAT_NM_KR { get; set; }
        public string? NAT_NM_EN { get; set; }
        public string? NAT_NM_JP { get; set; }
        public string? NAT_NM_CN { get; set; }
        public string? CURR_CD { get; set; }
        public string? CURR_CD_NM { get; set; }
        public string? CURR_CD2 { get; set; }
        public string? CURR_CD_NM2 { get; set; }
        [Required]
        public string CD { get; set; } = string.Empty;
        [Required]
        public string VIEW_YN { get; set; } = string.Empty;
        [Required]
        public string USE_YN { get; set; } = string.Empty;
        [Required]
        public string DISPLAY_YN { get; set; } = string.Empty;
        [Required]
        [Column(TypeName = "tinyint")]
        public byte DISPLAY_SEQ { get; set; }
        [Required]
        [Column(TypeName = "tinyint")]
        public byte COL_SEQ { get; set; }
        public string? LANG_CD { get; set; }

        [JsonIgnore]
        [ForeignKey("AREA_CD")]
        [InverseProperty("Nations")]
        public virtual HTT_CODE_AREA Areas { get; set; }
    }
}

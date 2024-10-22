using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities
{
    public class HTT_CODE_CITY
    {
        [Key]
        [Column(TypeName = "smallint")]
        public byte CITY_SEQ { get; set; }
        [Key]
        public string CITY_CODE { get; set; }
        [Key]
        public string NAT_CD { get; set; }
        public string? CITY_NM_KR { get; set; }
        public string? CITY_NM_EN { get; set; }
        public string? CITY_NM_JP { get; set; }
        public string? CITY_NM_CN { get; set; }
        public string? CITY_NM_KEYWORD { get; set; }
        public string? VIEW_YN { get; set; }
        public string? USE_YN { get; set; }
        public DateTime? REGDT { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities
{
    public class HTT_CODE_STATE
    {
        [Key]        
        public int STATE_SEQ { get; set; }        
        public string STATE_CD { get; set; }
        [Required]
        public string STATE_GUBUN { get; set; }
        [Required]
        public string NAT_CD { get; set; }
        [Required]
        public string STATE_NM_KR { get; set; }
        [Required]
        public string STATE_NM_EN { get; set; }
        [Required]
        public string STATE_NM_JP { get; set; }
        [Required]
        public string VIEW_YN { get; set; }
        [Required]
        public string USE_YN { get; set; }        
        public DateTime? REGDT { get; set; }
    }
}

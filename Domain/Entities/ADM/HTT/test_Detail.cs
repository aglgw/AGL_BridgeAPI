using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities
{
    public class test_Detail
    {
        [Key]
        public int idx { get; set; }
        [Display(Name = "마스터 PK"), Required]
        public int master_idx { get; set; }

        [Display(Name = "이름"), Required]
        public string name { get; set; } = string.Empty;
        [Display(Name = "생성일시"), Required]
        public DateTime reg_dt { get; set; }
        [Display(Name = "수정일시")]
        public DateTime? mod_dt { get; set; }


        [JsonIgnore]
        [ForeignKey("master_idx")]
        public virtual test_Master Master { get; set; }

    }
}

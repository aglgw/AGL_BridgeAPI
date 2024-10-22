using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities
{
    public class ht_bbs_notice_lang_files
    {
        [Key]
        [Required]
        public int idx { get; set; }
        public int? bbs_notice_lang_idx { get; set; }        
        public string? ori_name { get; set; }        
        public string? up_name { get; set; }
        public string? file_ext { get; set; }
        public int? file_size { get; set; }
        public DateTime? reg_date { get; set; }

        [JsonIgnore]
        [ForeignKey("bbs_notice_lang_idx")]
        public virtual ht_bbs_notice_lang NoticeLang { get; set; }
    }
}

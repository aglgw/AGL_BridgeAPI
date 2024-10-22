using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace AGL.Api.Domain.Entities
{
    public class ht_bbs_notice
    {
        [Key]
        [Required]
        public int idx { get; set; }
        public string? subject { get; set; }
        public string? name { get; set; }
        public int? view_count { get; set; }
        public byte? is_noti { get; set; }
        public DateTime reg_date { get; set; }
        public DateTime? up_date { get; set; }
        public int? status { get; set; }

        [JsonIgnore]
        public virtual ICollection<ht_bbs_notice_lang> NoticeLang { get; set; }
    }
}

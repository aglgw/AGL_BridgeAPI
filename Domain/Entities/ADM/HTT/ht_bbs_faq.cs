using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AGL.Api.Domain.Entities
{
    public class ht_bbs_faq
    {
        [Key]
        [Required]
        public int idx { get; set; }
        public int bbs_faq_option_idx { get; set; }
        public string? subject { get; set; }
        public string? name { get; set; }
        public int? view_count { get; set; }
        public int? status { get; set; }
        public DateTime? reg_date { get; set; }
        public DateTime? up_date { get; set; }

        [JsonIgnore]
        public virtual ICollection<ht_bbs_faq_lang> FaqLang { get; set; }
    }
}

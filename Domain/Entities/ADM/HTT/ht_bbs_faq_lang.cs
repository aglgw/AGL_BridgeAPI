using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities
{
    public class ht_bbs_faq_lang
    {
        [Key]
        [Required]
        public int idx { get; set; }
        public int? bbs_faq_idx { get; set; }
        public int? language_idx { get; set; }
        public string? subject { get; set; }
        public string? content { get; set; }

        [JsonIgnore]
        [ForeignKey("bbs_faq_idx")]
        public virtual ht_bbs_faq Faq { get; set; }

        [JsonIgnore]
        [ForeignKey("language_idx")]
        public virtual ht_language Language { get; set; }

        [JsonIgnore]
        public virtual ICollection<ht_bbs_faq_lang_files> Files { get; set; }
    }
}

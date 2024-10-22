using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities
{
    public class ht_agree_lang
    {
        [Key]
        [Required]

        public int idx { get; set; }
        [Required]
        public int agree_idx { get; set; }
        [Required]
        public int language_idx { get; set; }
        [Required]
        public string content { get; set; }


        [JsonIgnore]
        [ForeignKey("agree_idx")]
        public virtual ht_user User { get; set; }

        [JsonIgnore]
        [ForeignKey("language_idx")]
        public virtual ht_language Language { get; set; }
    }
}

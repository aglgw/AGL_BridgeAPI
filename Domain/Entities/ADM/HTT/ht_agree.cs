using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities
{
    public class ht_agree
    {
        [Key]
        [Required]
        public int idx { get; set; }
        [Required]
        public int agree_gubun { get; set; }
        [Required]
        public int lang_type { get; set; }
        [Required]
        public string content { get; set; }
        [Required]
        public string version { get; set; }
        [Required]
        public DateTime start_date { get; set; }
        [Required]
        public DateTime reg_date { get; set; }
        [Required]
        public DateTime up_date { get; set; }

        [JsonIgnore]
        public virtual ICollection<ht_agree_lang> Lang { get; set; }
    }
}

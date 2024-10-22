using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGL.Api.Domain.Entities
{
    public class ht_multi_language_name
    {
        [Key]
        [Required]
        public string code_name { get; set; }
        [Key]
        [Required]
        public int language_idx { get; set; }
        public string? name { get; set; }
        
        [JsonIgnore]
        [ForeignKey("code_name")]
        public virtual ht_multi_language_code LangCode { get; set; }

        [JsonIgnore]
        [ForeignKey("language_idx")]
        public virtual ht_language Language { get; set; }
    }
}

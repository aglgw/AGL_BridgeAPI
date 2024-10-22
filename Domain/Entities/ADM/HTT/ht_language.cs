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
    public class ht_language
    {
        [Key]
        [Required]
        public int? idx { get; set; }
        public string? name { get; set; }
        public string? display_name { get; set; }
        public byte? display_order { get; set; }
        public string? code { get; set; }

        [JsonIgnore]
        public virtual ht_language_files LangFile { get; set; }
    }
}

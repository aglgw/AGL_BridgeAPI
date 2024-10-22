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
    public class ht_multi_language
    {
        [Key]
        [Required]
        public int idx { get; set; }
        public string? code_name { get; set; }
        public string? name { get; set; }
        public byte? depth { get; set; }
        public int? p_idx { get; set; }

        [JsonIgnore]
        [ForeignKey("p_idx")]
        public virtual ht_multi_language Parent { get; set; }

    }
}

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
    public class ht_multi_language_code
    {
        [Key]
        [Required]
        public string code_name { get; set; }
        public string? code_name1 { get; set; }
        public string? code_name2 { get; set; }
        public string? code_name3 { get; set; }
        public string? name { get; set; }

        [JsonIgnore]
        public virtual ICollection<ht_multi_language_name> LangNames { get; set; }



    }
}

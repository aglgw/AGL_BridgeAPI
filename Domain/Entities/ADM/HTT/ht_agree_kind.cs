
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
    public class ht_agree_kind
    {
        [Key]
        [Required]

        public int idx { get; set; }
        public string code_name { get; set; }
        public string name { get; set; }
        public DateTime reg_date { get; set; }
        public DateTime up_date { get; set; }

        [JsonIgnore]
        [ForeignKey("idx")]
        public virtual ht_agree Agree { get; set; }
    }
}

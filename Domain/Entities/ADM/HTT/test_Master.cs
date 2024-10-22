using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities
{
    public class test_Master
    {
        [Key]
        public int idx { get; set; }

        [Display(Name = "이름"), Required]
        public string name { get; set; } = string.Empty;
        public byte[]? enc { get; set; }
        public string password { get; set; } = string.Empty;

        [JsonIgnore]
        public virtual ICollection<test_Detail> Detail { get; set; }

    }
}

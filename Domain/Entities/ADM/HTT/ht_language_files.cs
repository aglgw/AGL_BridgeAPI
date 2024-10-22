using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities
{
    public class ht_language_files
    {
        [Key]
        [Required]

        public int idx { get; set; }
        public int? language_idx { get; set; }
        public string? ori_name { get; set; }
        public string? up_name { get; set; }
        public string? file_ext { get; set; }
        public int? file_size { get; set; }
        public DateTime? reg_date { get; set; }

        [JsonIgnore]
        [ForeignKey("language_idx")]
        public virtual ht_language Lang { get; set; }
    }
}

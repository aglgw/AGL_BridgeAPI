using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace AGL.Api.Domain.Entities
{

    public class ht_review_files
    {
        [Key]
        [Required]
        public int idx { get; set; }
        public int? user_idx { get; set; }
        public int? review_idx { get; set; }
        public string? pack_code { get; set; }
        public string? ori_name { get; set; }
        public string? up_name { get; set; }
        public string? file_ext { get; set; }
        public int? file_size { get; set; }
        public DateTime? reg_date { get; set; }

        [JsonIgnore]
        [ForeignKey("user_idx")]
        public virtual ht_user User { get; set; }

        [JsonIgnore]
        [ForeignKey("review_idx")]
        public virtual ht_review Review { get; set; }

    }
}

using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace AGL.Api.Domain.Entities
{

    public class ht_review_like
    {
        [Key]
        [Required]
        public int idx { get; set; }
        public int? user_idx { get; set; }
        public int? review_idx { get; set; }

        public string? pack_code { get; set; }

        public DateTime? reg_date { get; set; }
        
        [JsonIgnore]
        [ForeignKey("user_idx")]
        public virtual ht_user User { get; set; }

        [JsonIgnore]
        [ForeignKey("review_idx")]
        public virtual ht_review Review { get; set; }

    }
}

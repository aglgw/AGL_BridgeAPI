using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities
{

    public class ht_cart
    {
        [Key]
        [Required]
        public int idx { get; set; }
        public int? user_idx { get; set; }

        public string? pack_code { get; set; }
        public int? prod_opt_idx { get; set; }
        public int? hotel_idx { get; set; }
        public DateTime? reg_date { get; set; }

        [JsonIgnore]
        [ForeignKey("user_idx")]
        public virtual ht_user User { get; set; }
    }
}

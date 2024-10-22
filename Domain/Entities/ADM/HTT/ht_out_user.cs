using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities
{
    public class ht_out_user
    {
        [Key]
        [Required]
        public int idx { get; set; }

        public int? user_idx { get; set; }
        public int? social_user_idx { get; set; }
        public string? j_data { get; set; }



        [JsonIgnore]
        [ForeignKey("user_idx")]
        public virtual ht_user User { get; set; }



    }
}

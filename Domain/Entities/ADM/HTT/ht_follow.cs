using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities
{
    public class ht_follow
    {
        [Key]
        [Required]
        public int idx { get; set; }
        public int? user_idx { get; set; }
        public int? target_idx { get; set; }
        public DateTime? reg_date { get; set; }

        [JsonIgnore]
        [ForeignKey("user_idx")]
        [InverseProperty("Follower")]
        public virtual ht_user User { get; set; }


        [JsonIgnore]
        [ForeignKey("target_idx")]
        public virtual ht_user TargetUser { get; set; }


    }
}

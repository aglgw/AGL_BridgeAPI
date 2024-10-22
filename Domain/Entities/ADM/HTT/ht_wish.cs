using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities
{
    public class ht_wish
    {
        [Key]
        [Required]
        public int idx { get; set; }
        public int? user_idx { get; set; }
        public string? prod_id { get; set; }
        public DateTime? reg_date { get; set; }

        [JsonIgnore]
        [ForeignKey("user_idx")]
        public virtual ht_user User { get; set; }



        [JsonIgnore]
        public virtual ICollection<ht_wish_push> Push { get; set; }

        //상품 테이블은 htt20 mssql db에 존재 
        //[JsonIgnore]
        //[ForeignKey("prod_idx")]
        //public virtual ht_user TargetUser { get; set; }


    }
}

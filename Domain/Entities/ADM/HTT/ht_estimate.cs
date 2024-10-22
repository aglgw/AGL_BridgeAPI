using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AGL.Api.Domain.Entities
{
    public class ht_estimate
    {
        [Key]
        [Required]
        public int idx { get; set; }
        public int? user_idx { get; set; }
        public string? company_name { get; set; }
        public string? name { get; set; }
        public string? contact { get; set; }
        public DateOnly? start_date { get; set; }
        public DateOnly? end_date { get; set; }
        public int? person_count { get; set; }
        public int? vip_count { get; set; }
        public string? hope_location { get; set; }
        public int? place_type { get; set; }
        public int? food { get; set; }
        public int? room { get; set; }
        public int? movement { get; set; }
        public int? tour { get; set; }
        public int? budget_type { get; set; }
        public int? budget_price { get; set; }
        public string? content { get; set; }
        public byte? status { get; set; }
        public DateTime? reg_date { get; set; }
        public DateTime? up_date { get; set; }
        
        [JsonIgnore]
        [ForeignKey("user_idx")]
        public virtual ht_user User { get; set; }



        [JsonIgnore]
        public virtual ICollection<ht_estimate_files> Files { get; set; }

        //상품 테이블은 htt20 mssql db에 존재 
        //[JsonIgnore]
        //[ForeignKey("prod_idx")]
        //public virtual ht_user TargetUser { get; set; }


    }
}

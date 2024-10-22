using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities
{
    public enum EnumPointKInd
    {
        상품구입 = 1,
        회원가입,
        이벤트,
        기타

    }
    public class ht_point
    {
        [Key]
        [Required]
        public int idx { get; set; }
        public int? user_idx { get; set; }
        public string? booking_number { get; set; }
        public EnumPointKInd point_kind { get; set; }
        public int? point { get; set; }
        public string? memo { get; set; }
        public byte? status { get; set; }
        public DateTime? reg_date { get; set; }
        public DateTime? up_date { get; set; }
        public DateTime? out_date{ get; set; }


        [JsonIgnore]
        [ForeignKey("user_idx")]
        public virtual ht_user User { get; set; }




        //상품 테이블은 htt20 mssql db에 존재 
        //[JsonIgnore]
        //[ForeignKey("prod_idx")]
        //public virtual ht_user TargetUser { get; set; }


    }
}

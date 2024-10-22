using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities
{
    public class admin_member
    {
        [Key]
        public int adm_seq { get; set; }
        [Display(Name = "로그인ID"), Required]
        public string adm_id { get; set; } = "";
        [Display(Name = "패스워드"), Required]
        public byte[] adm_pwd { get; set; }
        [Display(Name = "이름"), Required]
        public string adm_name { get; set; } = "";
        [Display(Name = ""), Required]
        public int adm_part_seq { get; set; }
        [Display(Name = ""), Required]
        public int cj_seq { get; set; }
        [Display(Name = ""), Required]
        public string email { get; set; } = "";
        [Display(Name = ""), Required]
        public string confirm_yn { get; set; } = "N";
        [Display(Name = ""), Required]
        public string use_yn { get; set; } = "Y";
        [Display(Name = ""), Required]
        public string work_yn { get; set; } = "Y";
        [Display(Name = ""), Required]
        public string? memo { get; set; }
        [Display(Name = ""), Required]
        public DateTime? regdt { get; set; }
        [Display(Name = ""), Required]
        public DateTime? moddt { get; set; }
        [Display(Name = ""), Required]
        public DateTime? outdt { get; set; }
    }
}

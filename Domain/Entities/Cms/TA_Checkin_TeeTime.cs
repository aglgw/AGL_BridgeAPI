using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities
{
    public class TA_Checkin_TeeTime
    {
        [Key]
        [Display(Name = "TimeId")]
        public string TimeId { get; set; }
        [Key]
        [Display(Name = "CheckinId"),Required]
        public string CheckinId { get; set; } 
        [Display(Name = "상태값"), Required]
        public int Status { get; set; }
        [Display(Name = "생성일시"), Required]
        public DateTime CreatedDate { get; set; }
        [Display(Name = "취소일시")]
        public DateTime? CancelDate { get; set; }
        [Display(Name = "수정일시"), Required]
        public string LastUpdate { get; set; }
    }
}

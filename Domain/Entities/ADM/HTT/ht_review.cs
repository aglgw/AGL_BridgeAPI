using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace AGL.Api.Domain.Entities
{

    public class ht_review
    {
        [Key]
        [Required]
        public int idx { get; set; }
        public int? user_idx { get; set; }
        public int? booking_tee_idx { get; set; }
        public string? pack_code { get; set; }
        public string? prod_id { get; set; }
        public string? content { get; set; }
        public byte? grade { get; set; }
        public byte? grade_coursedesign { get; set; }
        public byte? grade_coursecondition { get; set; }
        public byte? grade_facilities { get; set; }
        public byte? grade_accessibility { get; set; }
        public byte? grade_service { get; set; }
        public byte? score { get; set; }
        public string? is_best { get; set; }
        public string? is_worst { get; set; }
        public string? is_keyword { get; set; }
        public DateTime? reg_date { get; set; }
        
        [JsonIgnore]
        [ForeignKey("user_idx")]
        public virtual ht_user User { get; set; }

        [JsonIgnore]
        [ForeignKey("booking_tee_idx")]
        public virtual ht_booking_tee BookingTee { get; set; }

        public virtual ICollection<ht_review_like> Likes { get; set; }
        public virtual ICollection<ht_review_files> Files { get; set; }
    }
}

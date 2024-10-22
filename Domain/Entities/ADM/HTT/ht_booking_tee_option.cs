using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities
{

    public class ht_booking_tee_option
    {
        [Key]
        [Required]
        public int idx { get; set; }
        public int? booking_tee_idx { get; set; }

        public string? pack_code { get; set; }
        public int? prod_opt_idx { get; set; }
        public int? person_count { get; set; }
        public int? price { get; set; }

        [JsonIgnore]
        [ForeignKey("booking_tee_idx")]
        public virtual ht_booking_tee BookingTee { get; set; }
    }
}

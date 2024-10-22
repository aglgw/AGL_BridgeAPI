using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AGL.Api.Domain.Entities
{
    public class ht_booking_tee
    {
        [Key]
        [Required]
        public int idx { get; set; }
        public int? user_idx { get; set; }
        public string? booking_number { get; set; }
        public string? pack_code { get; set; }
        public string? prod_id { get; set; }
        public int? prod_gubun { get; set; }
        public string? location1 { get; set; }
        public string? location2 { get; set; }
        public string? location3 { get; set; }
        public string? location4 { get; set; }
        public string? prod_name { get; set; }
        public int? regular_price { get; set; }
        public int? sale_price { get; set; }
        public int? channel_price { get; set; }
        public int? staff_price { get; set; }
        public byte? is_air { get; set; }
        public byte? is_hotel { get; set; }
        public byte? is_sending { get; set; }
        public int? status { get; set; }
        public string? course { get; set; }
        public int? partner_count { get; set; }
        public int one_price { get; set; }
        public DateOnly? use_date { get; set; }

        [JsonIgnore]
        [ForeignKey("user_idx")]
        public virtual ht_user User { get; set; }


        [JsonIgnore]
        [ForeignKey("booking_number")]
        [InverseProperty("BookingTees")]
        public virtual ht_booking Booking { get; set; }


        [JsonIgnore]
        public virtual ICollection<ht_booking_tee_option> Option { get; set; }


    }
}

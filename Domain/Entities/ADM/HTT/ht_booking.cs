using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities
{
    public enum EnumBookingGubun
    { 
        티타임 = 1,
        패키지

    }



    public class ht_booking
    {
        [Key]
        [Required]
        public int idx { get; set; }
        public int? user_idx { get; set; }
        
        public string? booking_number { get; set; }
        public string? first_name { get; set; }
        public string? last_name { get; set; }
        public EnumBookingGubun? booking_gubun { get; set; }
        public byte? status { get; set; }
        public DateTime? fixed_date { get; set; }
        public DateTime? cancel_date { get; set; }
        public DateTime? reg_date { get; set; }
        public DateTime? up_date { get; set; }

        [JsonIgnore]
        [ForeignKey("user_idx")]
        public virtual ht_user User { get; set; }
        [JsonIgnore]
        [InverseProperty("Booking")]
        public virtual ICollection<ht_booking_tee>BookingTees { get; set; }
    }
}

using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities
{
    public enum EnumSex
    {
        Male=1,
        Female
    }
    public class ht_user
    {
        [Key]
        [Required]
        public int idx { get; set; }

        public int? social_user_idx { get; set; }

        public string? account { get; set; }

        public string? passwd { get; set; }

        public byte? member_type { get; set; }

        public byte? level { get; set; }

        public byte[]? name_kr { get; set; }

        public byte[]? nick { get; set; }

        public byte[]? birth { get; set; }

        public byte[]? nationality_type { get; set; }

        public byte[]? country_code { get; set; }
        public byte[]? phone_number { get; set; }

        public byte[]? hp_number { get; set; }

        public byte[]? sex { get; set; }

        public int? point { get; set; }

        public string? channel { get; set; }

        public byte? status { get; set; }

        public string? device_token { get; set; }

        public string? profile_image { get; set; }
        
        public DateTime? reg_date { get; set; }

        public DateTime? up_date { get; set; }

        public DateTime? out_date { get; set; }

        [JsonIgnore]
        [ForeignKey("social_user_idx")]
        public virtual ht_social_user Social{ get; set; }


        [JsonIgnore]
        public virtual ICollection<ht_addr_book> AddrBooks { get; set; }

        [JsonIgnore]
        [InverseProperty("User")]
        public virtual ICollection<ht_follow> Follower { get; set; }

        [JsonIgnore]
        public virtual ICollection<ht_wish> WishProucts { get; set; }

        [JsonIgnore]
        public virtual ICollection<ht_point> Points { get; set; }

        [JsonIgnore]
        public virtual ICollection<ht_inquery> Inqueries { get; set; }

        [JsonIgnore]
        public virtual ICollection<ht_estimate> Estimates { get; set; }

        [JsonIgnore]
        public virtual ICollection<ht_booking> Bookings { get; set; }

        [JsonIgnore]
        public virtual ICollection<ht_cart> Carts { get; set; }

        [JsonIgnore]
        public virtual ICollection<ht_review> Reviews { get; set; }

    }
}

using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AGL.Api.Domain.Entities
{

    public class ht_sendemail
    {
        [Key]
        [Required]
        public int idx { get; set; }
        public string account { get; set; }
        public string verificationCode { get; set; }
        public DateTime reg_date { get; set; }
    }
}


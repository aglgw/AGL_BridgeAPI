using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGL.Api.Domain.Entities.ADM.HTT
{
    public class ht_currency
    {
        [Key]
        [Required]
        public int idx { get; set; }
        public string? currency_code { get; set; }
        public string? currency_sign { get; set; }
        public byte? display_order { get; set; }
        public DateTime? reg_date { get; set; }
    }
}

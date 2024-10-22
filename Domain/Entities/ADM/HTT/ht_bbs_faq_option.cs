using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities
{
    public class ht_bbs_faq_option
    {
        [Key]
        [Required]
        public int idx { get; set; }
        public string? code_name { get; set; }
        public string? name { get; set; }
        public DateTime? reg_date { get; set; }
        public DateTime? up_date { get; set; }
    }
}

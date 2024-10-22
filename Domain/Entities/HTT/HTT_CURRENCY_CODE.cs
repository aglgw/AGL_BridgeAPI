using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities
{
    public class HTT_CURRENCY_CODE
    {
        [Key]
        [Display(Name = "CURRENCY_CODE")]
        public string CURRENCY_CODE { get; set; }
        [Display(Name = "COUNTRY"), Required]
        public string COUNTRY { get; set; }
    }
}

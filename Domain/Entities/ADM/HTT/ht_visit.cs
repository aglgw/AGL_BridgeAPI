using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities
{
    public class ht_visit
    {
        [Key]
        [Required]
        public int idx { get; set; }

        public int user_idx { get; set; }

        public string? referer { get; set; }

        public string? ip { get; set; }

        public DateTime? reg_date { get; set; }


    }
}

using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGL.Api.Domain.Entities
{
    public class ht_social_user
    {
        [Key]
        [Required]
        public int idx { get; set; }
        public string? social_id { get; set; }
        public string? social_name { get; set; }

    }
}

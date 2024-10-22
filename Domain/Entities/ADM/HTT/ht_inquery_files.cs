using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace AGL.Api.Domain.Entities
{
    public class ht_inquery_files
    {
        [Key]
        [Required]
        public int idx { get; set; }
        public int? inquery_idx { get; set; }
        public string? gubun { get; set; }
        public string? ori_name { get; set; }
        public string? up_name { get; set; }
        public string? file_ext { get; set; }
        public int? file_size { get; set; }
        public DateTime reg_date { get; set; }

        [JsonIgnore]
        [ForeignKey("inquery_idx")]
        public virtual ht_inquery Inquery { get; set; }



    }
}

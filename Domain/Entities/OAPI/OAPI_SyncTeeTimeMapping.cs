using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AGL.Api.Domain.Entities.OAPI
{
    public class OAPI_SyncTeeTimeMapping
    {
        [Key]
        [Required]
        public int SyncTeeTimeMappingId { get; set; }

        [Required]
        [ForeignKey("OAPI_TeeTimeMapping")]
        public int TeetimeMappingId { get; set; }

        [JsonIgnore]
        public virtual OAPI_TeeTimeMapping TeeTimeMapping { get; set; } = new OAPI_TeeTimeMapping();
    }
}

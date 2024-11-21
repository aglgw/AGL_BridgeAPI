using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AGL.Api.Domain.Entities.OAPI
{
    public class OAPI_SyncClient
    {
        [Key]
        [Required]
        public int SyncClientId { get; set; }

        [Required]
        public string ClientName { get; set; }

        public string? ClientEndpoint { get; set; }

        public int? LastSyncTeeTimeMappingId { get; set; }

        [JsonIgnore]
        public virtual OAPI_Authentication Authentication { get; set; }
    }
}

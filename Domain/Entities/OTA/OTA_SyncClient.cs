using AGL.Api.Domain.Entities.OAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AGL.Api.Domain.Entities.OTA
{
    public class OTA_SyncClient
    {
        [Key]
        [Required]
        public int SyncClientId { get; set; }

        [Required]
        public string ClientCode { get; set; }

        [StringLength(50)]
        public string? ClientName { get; set; } // 사용자 명

        public string? ClientEndpoint { get; set; }

        public int? LastSyncTeeTimeMappingId { get; set; }

        [Required]
        public bool IsSyncEnabled { get; set; }

        [JsonIgnore]
        public virtual OTA_Authentication Authentication { get; set; }
    }
}

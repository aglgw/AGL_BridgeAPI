using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AGL.Api.Domain.Entities.OAPI
{
    public class OAPI_Authentication
    {
        [Key]
        [Required]
        public int AuthenticationId { get; set; } // 인증ID (PK)

        public int? SupplierId { get; set; } // 공급자ID 

        public int? SyncClientId { get; set; } // 클라이언트ID

        // AGL 에서 받는 토큰
        [StringLength(50)]
        public string TokenSupplier { get; set; } // supplier에서 AGL 전송 용 발급토큰

        // AGL 에서 받는 토큰
        [StringLength(50)]
        public string TokenClient { get; set; } // 클라이언트(현재는 내부용 GDS1.0 , BO 등등 ) 발급토큰

        // AGL 에서 보내는 코드
        [StringLength(50)]
        public string AglCode { get; set; } // AGL에서 supplier 전송 용 코드

        // AGL 에서 보내는 토큰
        [StringLength(50)]
        public string TokenAgl { get; set; } // AGL에서 supplier 전송 용 발급토큰


        public bool Deleted { get; set; } // 삭제여부

        [Required]
        public DateTime CreatedDate { get; set; } // 생성일

        // 네비게이션 속성
        [JsonIgnore]
        public virtual OAPI_Supplier Supplier { get; set; }

        [JsonIgnore]
        public virtual OAPI_SyncClient SyncClient { get; set; }
    }
}

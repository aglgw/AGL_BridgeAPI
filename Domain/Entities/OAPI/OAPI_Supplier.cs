using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AGL.Api.Domain.Entities.OAPI
{
    public class OAPI_Supplier
    {
        [Key]
        [Required]
        public int SupplierId { get; set; } // 공급자ID (PK)

        [Required, StringLength(6)]
        public string FieldId { get; set; } // 공급자필드ID

        [Required, StringLength(50)]
        public string SupplierCode { get; set; } // 공급자 코드

        [Required, StringLength(50)]
        public string TokenSupplierToAgl { get; set; } // 공급자 발급토큰

        [Required, StringLength(50)]
        public string AglCode { get; set; } // AGL 코드

        [Required, StringLength(50)]
        public string TokenAglToSupplier { get; set; } // AGL 발급토큰

        [StringLength(50)]
        public string EndPointDev { get; set; } // 개발 엔드포인트

        [StringLength(50)]
        public string EndPointProd { get; set; } // 상용 엔드포인트

        [Required]
        public DateTime CreatedDate { get; set; } // 생성일

        // 네비게이션 속성
        [JsonIgnore]
        public virtual ICollection<OAPI_GolfClub> GolfClubs { get; set; } = new List<OAPI_GolfClub>();

    }
}

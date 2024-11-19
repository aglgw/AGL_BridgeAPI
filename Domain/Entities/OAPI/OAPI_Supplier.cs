using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AGL.Api.Domain.Entities.OAPI
{
    public class OAPI_Supplier
    {
        [Key]
        [Required]
        public int SupplierId { get; set; } // 공급자ID (PK)

        [Required, StringLength(3)]
        public string DaemonId { get; set; } // 공급자 데몬ID - 연동용

        [Required, StringLength(50)]
        public string SupplierCode { get; set; } // 공급자 코드

        [Required, StringLength(50)]
        public string TokenSupplierToAgl { get; set; } // 공급자 발급토큰

        [Required, StringLength(50)]
        public string AglCode { get; set; } // AGL 코드

        [Required, StringLength(50)]
        public string TokenAglToSupplier { get; set; } // AGL 발급토큰

        [StringLength(50)]
        public string EndPoint { get; set; } // 엔드포인트

        [Required]
        public DateTime CreatedDate { get; set; } // 생성일

        // 네비게이션 속성
        [JsonIgnore]
        public virtual ICollection<OAPI_GolfClub> GolfClubs { get; set; } = new List<OAPI_GolfClub>();
        [JsonIgnore]
        public virtual ICollection<OAPI_TeeTime> TeeTimes { get; set; } = new List<OAPI_TeeTime>();
        [JsonIgnore]
        public virtual ICollection<OAPI_ReservationManagement> ReservationManagements { get; set; } = new List<OAPI_ReservationManagement>();
    }
}

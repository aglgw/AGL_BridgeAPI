using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AGL.Api.Domain.Entities.OAPI
{
    public class OAPI_Supplier
    {
        [Key]
        [Required]
        public int SupplierId { get; set; } // 공급자ID (PK)

        [Required, StringLength(50)]
        public string SupplierCode { get; set; } // 공급자 코드

        [StringLength(50)]
        public string? SupplierName { get; set; } // 공급자 명

        [StringLength(50)]
        public string? EndPoint { get; set; } // 엔드포인트

        [Required]
        public DateTime CreatedDate { get; set; } // 생성일

        // 네비게이션 속성
        [JsonIgnore]
        public virtual ICollection<OAPI_GolfClub> GolfClubs { get; set; } = new List<OAPI_GolfClub>();
        [JsonIgnore]
        public virtual ICollection<OAPI_TeeTime> TeeTimes { get; set; } = new List<OAPI_TeeTime>();
        [JsonIgnore]
        public virtual ICollection<OAPI_ReservationManagement> ReservationManagements { get; set; } = new List<OAPI_ReservationManagement>();
        [JsonIgnore]
        public virtual OAPI_Authentication Authentication { get; set; }
    }
}

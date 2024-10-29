using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGL.Api.Domain.Entities.OAPI
{
    public class OAPI_ReservationManagement // 예약 관리 테이블 (임시)
    {
        [Key]
        [Required]
        public int ReservationManagementId { get; set; }

        [Required]
        [ForeignKey("OAPI_Supplier")]
        public int SupplierId { get; set; }

        [Required, StringLength(100)]
        public string ReservationId { get; set; } // 공급사 예약 번호

        [StringLength(50)]
        public string SalesChannel { get; set; } // 판매 채널

        [StringLength(100)]
        public string Endpoint { get; set; } // 판매 채널 경로?

        public byte? ReservationStatus { get; set; } // 1: 예약요청, 2: 예약확정, 3: 예약취소

        [Required]
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public virtual OAPI_Supplier Supplier { get; set; }
    }
}

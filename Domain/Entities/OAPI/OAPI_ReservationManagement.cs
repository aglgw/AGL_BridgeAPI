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
        public string? ReservationId { get; set; } // 공급사 예약 번호

        [StringLength(50)]
        public string? SalesChannel { get; set; } // 판매 채널

        [StringLength(100)]
        public string? Endpoint { get; set; } // 판매 채널 경로?

        [Required]
        public byte ReservationStatus { get; set; } // 1: 예약요청, 2: 예약확정, 3: 예약취소

        [MaxLength(64)]
        public string GolfClubCode { get; set; }

        [MaxLength(255)]
        public string CourseCode { get; set; }

        [MaxLength(8)]
        public string ReservationDate { get; set; }

        [MaxLength(4)]
        public string ReservationStartTime { get; set; }

        public byte? ReservationMembers { get; set; }

        [MaxLength(3)]
        public string Currency { get; set; }

        public decimal? TotalPrice { get; set; }

        [MaxLength(255)]
        public string HolderName { get; set; }

        [MaxLength(32)]
        public string ReservationPhone { get; set; }

        [MaxLength(100)]
        public string ReservationEmail { get; set; }

        [MaxLength(3)]
        public string ReservationCountry { get; set; }

        public DateTime? cancelDate { get; set; }

        public decimal? cancelPenaltyAmount { get; set; }

        [MaxLength(3)]
        public string? cancelCurrency { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public virtual OAPI_Supplier Supplier { get; set; }

        // Navigation property
        public virtual ICollection<OAPI_ReservationmanagementGuest> Guests { get; set; } = new List<OAPI_ReservationmanagementGuest>();
    }
}

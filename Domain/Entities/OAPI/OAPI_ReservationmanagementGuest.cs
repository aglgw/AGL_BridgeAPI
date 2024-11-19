using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGL.Api.Domain.Entities.OAPI
{
    public class OAPI_ReservationmanagementGuest
    {
        [Key]
        [Required]
        public int ReservationManagementGuestId { get; set; }

        [Required]
        [ForeignKey("OAPI_ReservationManagement")]
        public int ReservationManagementId { get; set; } // 예약관리 (FK)

        [Required]
        public byte Idx { get; set; }

        [Required]
        [MaxLength(100)]
        public string? GuestName { get; set; }

        [MaxLength(32)]
        public string? GuestPhone { get; set; }

        [MaxLength(1)]
        public string? GuestGender { get; set; }

        [MaxLength(3)]
        public string? GuestCountry { get; set; }

        // Navigation property
        public virtual OAPI_ReservationManagement ReservationManagement { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event_Management_System.Models
{
    public class Booking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BookingID { get; set; }

        [Required]
        [StringLength(20)]
        public string BookingReferenceNumber { get; set; } = string.Empty;

        [Required]
        public int MemberID { get; set; }

        [Required]
        public int EventID { get; set; }

        public int? TicketID { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.Now;

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [StringLength(20)]
        public string BookingStatus { get; set; } = "Confirmed";

        [Column(TypeName = "text")]
        public string? SpecialRequests { get; set; }

        [StringLength(50)]
        public string? SeatType { get; set; }

        [StringLength(20)]
        public string PaymentMethod { get; set; } = "On-Time";

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal ServiceFee { get; set; } = 0;

        // QR Code for E-Ticket
        public byte[]? QRCode { get; set; }

        // Navigation properties
        [ForeignKey("MemberID")]
        public virtual Member? Member { get; set; }

        [ForeignKey("EventID")]
        public virtual Event? Event { get; set; }

        [ForeignKey("TicketID")]
        public virtual Ticket? Ticket { get; set; }
    }
}

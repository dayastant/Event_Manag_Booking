using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event_Management_System.Models
{
    public class Ticket
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TicketID { get; set; }

        [Required]
        public int EventID { get; set; }

        [Required]
        [StringLength(50)]
        public string SeatType { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        public int TotalQuantity { get; set; }

        [Required]
        public int AvailableQuantity { get; set; }

        // Navigation properties
        [ForeignKey("EventID")]
        public virtual Event? Event { get; set; }
    }
}

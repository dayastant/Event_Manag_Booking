using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event_Management_System.Models
{
    public class Event
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EventID { get; set; }

        [Required]
        [StringLength(100)]
        public string EventName { get; set; } = string.Empty;

        [Column(TypeName = "text")]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        public int TotalSeats { get; set; }

        [Required]
        public int AvailableSeats { get; set; }

        [Required]
        public int VenueID { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Upcoming";

        // Event Image (Binary storage)
        public byte[]? EventImage { get; set; }

        [StringLength(100)]
        public string? EventImageContentType { get; set; }

        // Computed property for Base64 image display
        [NotMapped]
        public string? EventImageBase64
        {
            get => EventImage != null && EventImageContentType != null
                ? $"data:{EventImageContentType};base64,{Convert.ToBase64String(EventImage)}"
                : null;
        }

        // Navigation properties
        [ForeignKey("VenueID")]
        public virtual Venue? Venue { get; set; }
        
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}

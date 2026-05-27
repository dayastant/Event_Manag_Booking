using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event_Management_System.Models
{
    public class Venue
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VenueID { get; set; }

        [Required]
        [StringLength(100)]
        public string VenueName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string City { get; set; } = string.Empty;

        [Required]
        public int Capacity { get; set; }

        // Navigation properties
        public virtual ICollection<Event> Events { get; set; } = new List<Event>();
    }
}

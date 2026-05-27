using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event_Management_System.Models
{
    public class Guest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GuestID { get; set; }

        [StringLength(100)]
        public string? Name { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();
    }
}

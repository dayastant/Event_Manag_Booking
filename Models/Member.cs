using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event_Management_System.Models
{
    public class Member
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MemberID { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(50)]
        public string? PreferredCategory { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string Status { get; set; } = "Active";

        [Column(TypeName = "text")]
        public string? Preferences { get; set; }

        // Profile Photo (Binary storage)
        public byte[]? ProfilePhoto { get; set; }

        [StringLength(100)]
        public string? ProfilePhotoContentType { get; set; }

        // Computed property for Base64 image display
        [NotMapped]
        public string? ProfilePhotoBase64
        {
            get => ProfilePhoto != null && ProfilePhotoContentType != null
                ? $"data:{ProfilePhotoContentType};base64,{Convert.ToBase64String(ProfilePhoto)}"
                : null;
        }

        // Computed property for full name
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        // Navigation properties
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();
    }
}

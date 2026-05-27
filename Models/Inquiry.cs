using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event_Management_System.Models
{
    public class Inquiry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InquiryID { get; set; }

        public int? GuestID { get; set; }

        public int? MemberID { get; set; }

        [Required]
        [Column(TypeName = "text")]
        public string Message { get; set; } = string.Empty;

        public DateTime InquiryDate { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        // Navigation properties
        [ForeignKey("GuestID")]
        public virtual Guest? Guest { get; set; }

        [ForeignKey("MemberID")]
        public virtual Member? Member { get; set; }
    }
}

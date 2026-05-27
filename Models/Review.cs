using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event_Management_System.Models
{
    public class Review
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReviewID { get; set; }

        [Required]
        public int MemberID { get; set; }

        [Required]
        public int EventID { get; set; }

        [Range(1, 5)]
        public int? Rating { get; set; }

        [StringLength(200)]
        public string? Title { get; set; }

        [Column(TypeName = "text")]
        public string? Comment { get; set; }

        public DateTime ReviewDate { get; set; } = DateTime.Now;

        public bool IsRecommended { get; set; } = false;

        // Navigation properties
        [ForeignKey("MemberID")]
        public virtual Member? Member { get; set; }

        [ForeignKey("EventID")]
        public virtual Event? Event { get; set; }
    }
}

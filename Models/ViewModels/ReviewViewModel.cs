using System.ComponentModel.DataAnnotations;

namespace Event_Management_System.Models.ViewModels
{
    public class ReviewViewModel
    {
        [Required(ErrorMessage = "Event ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Event ID")]
        public int EventID { get; set; }

        public string EventName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please provide a rating")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Please share your feedback")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Comment must be between 10 and 500 characters")]
        public string Comment { get; set; } = string.Empty;
    }
}

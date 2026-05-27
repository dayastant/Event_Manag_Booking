using System.ComponentModel.DataAnnotations;

namespace Event_Management_System.Models.ViewModels
{
    public class ReviewCreateViewModel
    {
        public Event Event { get; set; } = null!;

        [Required(ErrorMessage = "Please select a rating")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Please provide a title")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please write a review")]
        [StringLength(2000, MinimumLength = 50, ErrorMessage = "Review must be between 50 and 2000 characters")]
        public string Comment { get; set; } = string.Empty;

        public bool IsRecommended { get; set; }
    }
}

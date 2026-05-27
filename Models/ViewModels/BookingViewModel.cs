using System.ComponentModel.DataAnnotations;

namespace Event_Management_System.Models.ViewModels
{
    public class BookingViewModel
    {
        [Required(ErrorMessage = "Event ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Event ID")]
        public int EventID { get; set; }

        public string EventName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seat type is required")]
        [RegularExpression("^(standard|vip|Standard|VIP)$", ErrorMessage = "Invalid seat type. Must be 'standard' or 'vip'")]
        public string SeatType { get; set; } = "standard";

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 10, ErrorMessage = "Quantity must be between 1 and 10")]
        public int Quantity { get; set; } = 1;

        public decimal BasePrice { get; set; }

        public decimal TotalAmount { get; set; }
    }
}

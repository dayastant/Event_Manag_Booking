using System.ComponentModel.DataAnnotations;

namespace Event_Management_System.Models.ViewModels
{
    public class BookingCreateViewModel
    {
        public Event Event { get; set; } = null!;
        public Venue Venue { get; set; } = null!;
        public List<Ticket> TicketTypes { get; set; } = new List<Ticket>();

        [Required(ErrorMessage = "Please select a ticket type")]
        public int TicketID { get; set; }

        [Required(ErrorMessage = "Please specify quantity")]
        [Range(1, 10, ErrorMessage = "Quantity must be between 1 and 10")]
        public int Quantity { get; set; } = 1;

        public string? SpecialRequests { get; set; }

        [Required(ErrorMessage = "You must agree to the terms and conditions")]
        public bool AgreeToTerms { get; set; }

        // Calculated properties
        public decimal TicketPrice { get; set; }
        public decimal ServiceFee { get; set; } = 5.00m;
        public decimal Subtotal => TicketPrice * Quantity;
        public decimal TotalAmount => Subtotal + ServiceFee;
    }
}

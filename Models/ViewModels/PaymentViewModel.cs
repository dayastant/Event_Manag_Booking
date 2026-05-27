using Event_Management_System.Models;

namespace Event_Management_System.Models.ViewModels
{
    public class PaymentViewModel
    {
        public int BookingID { get; set; }
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string SeatType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = "On-Time";
        
        // Card payment details
        public string? CardNumber { get; set; }
        public string? CardHolderName { get; set; }
        public string? ExpiryDate { get; set; }
        public string? CVV { get; set; }
        public bool AgreeToTerms { get; set; }
    }
}

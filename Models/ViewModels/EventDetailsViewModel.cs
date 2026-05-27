using System.ComponentModel.DataAnnotations;

namespace Event_Management_System.Models.ViewModels
{
    public class EventDetailsViewModel
    {
        public Event Event { get; set; } = null!;
        public Venue Venue { get; set; } = null!;
        public List<Ticket> TicketTypes { get; set; } = new List<Ticket>();
        public List<Review> Reviews { get; set; } = new List<Review>();
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public bool UserHasBooked { get; set; }
        public bool UserHasReviewed { get; set; }
    }
}

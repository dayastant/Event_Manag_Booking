using Event_Management_System.Models;

namespace Event_Management_System.Models.ViewModels
{
    public class DashboardOverviewViewModel
    {
        public int TotalBookings { get; set; }
        public int UpcomingEvents { get; set; }
        public decimal TotalSpent { get; set; }
        public List<Booking> RecentBookings { get; set; } = new List<Booking>();
        public List<Review> RecentReviews { get; set; } = new List<Review>();
    }
}

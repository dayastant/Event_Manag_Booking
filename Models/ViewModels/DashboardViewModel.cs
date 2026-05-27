namespace Event_Management_System.Models.ViewModels
{
    public class DashboardViewModel
    {
        public Member? Member { get; set; }
        public int TotalBookings { get; set; }
        public int TotalReviews { get; set; }
        public int UpcomingEvents { get; set; }
        public List<Booking> RecentBookings { get; set; } = new List<Booking>();
        public List<Review> RecentReviews { get; set; } = new List<Review>();
    }
}

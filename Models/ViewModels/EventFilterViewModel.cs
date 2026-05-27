namespace Event_Management_System.Models.ViewModels
{
    public class EventFilterViewModel
    {
        public string? SearchText { get; set; }
        public string? Category { get; set; }
        public DateTime? EventDate { get; set; }
        public List<Event> Events { get; set; } = new List<Event>();
    }
}

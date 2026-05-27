using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Event_Management_System.Data;
using Event_Management_System.Models;

namespace Event_Management_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Home/Index
        public async Task<IActionResult> Index()
        {
            // Get comprehensive statistics for the About section with AsNoTracking
            ViewBag.TotalEvents = await _context.Events.AsNoTracking().CountAsync();
            
            ViewBag.TotalMembers = await _context.Members.AsNoTracking().CountAsync();
            
            ViewBag.TotalTicketsSold = await _context.Bookings
                .AsNoTracking()
                .Where(b => b.BookingStatus == "Confirmed")
                .SumAsync(b => (int?)b.Quantity) ?? 0;

            // Get 3 most recent added events (used for both floating and upcoming sections)
            var allRecentEvents = await _context.Events
                .AsNoTracking()
                .Include(e => e.Venue)
                .Include(e => e.Tickets)
                .Where(e => e.EventDate >= DateTime.Today)
                .OrderByDescending(e => e.EventID)
                .Take(3)
                .Select(e => new
                {
                    Event = e,
                    MinPrice = e.Tickets.Any() ? e.Tickets.Min(t => t.Price) : 0
                })
                .AsSplitQuery()
                .ToListAsync();
                
            // Set minimum prices on events
            var upcomingEvents = allRecentEvents.Select(x => 
            {
                x.Event.Price = x.MinPrice;
                return x.Event;
            }).ToList();

            // Use first 2 for floating section
            ViewBag.FloatingEvents = upcomingEvents.Take(2).ToList();

            return View(upcomingEvents);
        }

        // GET: Home/About
        public IActionResult About()
        {
            return View();
        }

        // GET: Home/Contact
        public IActionResult Contact()
        {
            return View();
        }

        // POST: Home/Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(string name, string email, string message)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "All fields are required.";
                return View();
            }

            var memberId = HttpContext.Session.GetInt32("MemberID");
            
            if (memberId.HasValue)
            {
                // Logged in member inquiry
                var inquiry = new Inquiry
                {
                    MemberID = memberId.Value,
                    Message = message,
                    InquiryDate = DateTime.Now,
                    Status = "Pending"
                };
                _context.Inquiries.Add(inquiry);
            }
            else
            {
                // Guest inquiry - create or find guest
                var guest = await _context.Guests.FirstOrDefaultAsync(g => g.Email == email);
                
                if (guest == null)
                {
                    guest = new Guest
                    {
                        Name = name,
                        Email = email
                    };
                    _context.Guests.Add(guest);
                    await _context.SaveChangesAsync();
                }

                var inquiry = new Inquiry
                {
                    GuestID = guest.GuestID,
                    Message = message,
                    InquiryDate = DateTime.Now,
                    Status = "Pending"
                };
                _context.Inquiries.Add(inquiry);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Your inquiry has been submitted successfully!";
            return RedirectToAction("Contact");
        }
    }
}

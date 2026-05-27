using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Event_Management_System.Data;
using Event_Management_System.Models;
using Event_Management_System.Models.ViewModels;
using Event_Management_System.Helpers;
using System.Runtime.Versioning;

namespace Event_Management_System.Controllers
{
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings/Create?eventId=1
        public async Task<IActionResult> Create(int? eventId)
        {
            if (eventId == null)
            {
                return NotFound();
            }

            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("MemberID");
            if (userId == null)
            {
                TempData["Error"] = "Please log in to book tickets.";
                return RedirectToAction("Login", "Account");
            }

            var eventItem = await _context.Events
                .AsNoTracking()
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.EventID == eventId);

            if (eventItem == null)
            {
                return NotFound();
            }

            // Load tickets separately
            eventItem.Tickets = await _context.Tickets
                .Where(t => t.EventID == eventId && t.AvailableQuantity > 0)
                .ToListAsync();

            return View(eventItem);
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SupportedOSPlatform("windows")]
        public async Task<IActionResult> Create(int EventID, int MemberID, string SeatType, int Quantity, decimal TotalAmount, string PaymentMethod)
        {
            var userId = HttpContext.Session.GetInt32("MemberID");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get event
            var eventItem = await _context.Events
                .Include(e => e.Tickets)
                .FirstOrDefaultAsync(e => e.EventID == EventID);

            if (eventItem == null)
            {
                TempData["Error"] = "Event not found.";
                return RedirectToAction("Index", "Events");
            }

            // Check if event is bookable
            if (eventItem.Status == "Cancelled" || eventItem.Status == "Completed" || eventItem.EventDate < DateTime.Today)
            {
                TempData["Error"] = "Booking is closed for this event.";
                return RedirectToAction("Details", "Events", new { id = EventID });
            }

            // Get ticket for the selected seat type
            var ticket = eventItem.Tickets.FirstOrDefault(t => t.SeatType == SeatType);
            if (ticket == null || ticket.AvailableQuantity < Quantity)
            {
                TempData["Error"] = "Selected tickets are not available.";
                return RedirectToAction("Details", "Events", new { id = EventID });
            }

            // Calculate service fee
            decimal subtotal = ticket.Price * Quantity;
            decimal serviceFee = subtotal * 0.05m; // 5%

            // Create booking
            var booking = new Booking
            {
                BookingReferenceNumber = GenerateBookingReference(),
                MemberID = userId.Value,
                EventID = EventID,
                TicketID = ticket.TicketID,
                SeatType = SeatType,
                Quantity = Quantity,
                TotalAmount = TotalAmount,
                ServiceFee = serviceFee,
                PaymentMethod = PaymentMethod,
                BookingStatus = "Confirmed",
                BookingDate = DateTime.Now
            };

            // Generate QR Code for e-ticket
            var qrService = new QRCodeService();
            var qrData = $"BOOKING:{booking.BookingReferenceNumber}|EVENT:{eventItem.EventName}|DATE:{eventItem.EventDate:yyyy-MM-dd}|SEATS:{Quantity}";
            booking.QRCode = qrService.GenerateQRCode(qrData);

            _context.Bookings.Add(booking);

            // Update ticket availability
            ticket.AvailableQuantity -= Quantity;

            // Update event available seats
            eventItem.AvailableSeats -= Quantity;

            await _context.SaveChangesAsync();

            // TempData["Success"] removed to prevent duplicate popup on next page (Confirmation page handles its own logic)
            return RedirectToAction(nameof(Confirmation), new { id = booking.BookingID });
        }

        // GET: Bookings/Confirmation/5
        public async Task<IActionResult> Confirmation(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("MemberID");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var booking = await _context.Bookings
                .AsNoTracking()
                .Include(b => b.Event)
                    .ThenInclude(e => e!.Venue)
                .Include(b => b.Ticket)
                .FirstOrDefaultAsync(b => b.BookingID == id && b.MemberID == userId);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // Helper method to generate unique booking reference
        private string GenerateBookingReference()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd");
            var random = new Random().Next(1000, 9999);
            return $"BK-{timestamp}-{random}";
        }

        // POST: Bookings/Cancel/5
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = HttpContext.Session.GetInt32("MemberID");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Ticket)
                .FirstOrDefaultAsync(b => b.BookingID == id && b.MemberID == userId);

            if (booking == null)
            {
                return NotFound();
            }

            if (booking.BookingStatus == "Cancelled")
            {
                return Ok(); // Already cancelled
            }

            // Update status
            booking.BookingStatus = "Cancelled";

            // Restore inventory
            if (booking.Event != null)
            {
                booking.Event.AvailableSeats += booking.Quantity;
            }

            if (booking.Ticket != null)
            {
                booking.Ticket.AvailableQuantity += booking.Quantity;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        // GET: Bookings/MyBookings
        public async Task<IActionResult> MyBookings()
        {
            var userId = HttpContext.Session.GetInt32("MemberID");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var bookings = await _context.Bookings
                .AsNoTracking()
                .Include(b => b.Event)
                    .ThenInclude(e => e!.Venue)
                .Include(b => b.Ticket)
                .Where(b => b.MemberID == userId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return View(bookings);
        }
    }
}

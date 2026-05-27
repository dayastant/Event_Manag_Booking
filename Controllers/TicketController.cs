using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Event_Management_System.Data;
using Event_Management_System.Helpers;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace Event_Management_System.Controllers
{
    public class TicketController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ticket/Print/5
        [SupportedOSPlatform("windows")]
        public async Task<IActionResult> Print(int id)
        {
            var memberId = HttpContext.Session.GetInt32("MemberID");
            if (!memberId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                    .ThenInclude(e => e!.Venue)
                .Include(b => b.Member)
                .FirstOrDefaultAsync(b => b.BookingID == id && b.MemberID == memberId.Value);

            if (booking == null)
            {
                return NotFound();
            }

            if (booking.BookingStatus != "Confirmed")
            {
                TempData["Error"] = "Ticket is not confirmed yet.";
                return RedirectToAction("MyBookings", "Dashboard");
            }

            // Generate comprehensive QR Code content
            string qrContent = $"BookingID:{booking.BookingID}|" +
                             $"Ref:{booking.BookingReferenceNumber}|" +
                             $"Name:{booking.Member?.FirstName} {booking.Member?.LastName}|" +
                             $"Email:{booking.Member?.Email}|" +
                             $"Event:{booking.Event?.EventName}|" +
                             $"Date:{booking.Event?.EventDate:yyyy-MM-dd}|" +
                             $"Time:{booking.Event?.StartTime:hh\\:mm}|" +
                             $"Venue:{booking.Event?.Venue?.VenueName}|" +
                             $"Seat:{booking.SeatType}|" +
                             $"Qty:{booking.Quantity}|" +
                             $"Amount:${booking.TotalAmount}|" +
                             $"Status:{booking.BookingStatus}";
            
            // Generate QR Code
            var qrService = new QRCodeService();
            ViewBag.QRCodeImage = qrService.GenerateQRCodeBase64(qrContent);

            return View(booking);
        }
        // GET: Ticket/GetTicketPartial/5
        [HttpGet]
        [SupportedOSPlatform("windows")]
        public async Task<IActionResult> GetTicketPartial(int id)
        {
            var memberId = HttpContext.Session.GetInt32("MemberID");
            if (!memberId.HasValue) return Content("Not authenticated");

            var booking = await _context.Bookings
                .Include(b => b.Event)
                    .ThenInclude(e => e!.Venue)
                .Include(b => b.Member)
                .FirstOrDefaultAsync(b => b.BookingID == id && b.MemberID == memberId.Value);

            if (booking == null) return Content("Booking not found");

            // Generate comprehensive QR Code
            var qrService = new QRCodeService();
            string qrContent = $"BookingID:{booking.BookingID}|" +
                             $"Ref:{booking.BookingReferenceNumber}|" +
                             $"Name:{booking.Member?.FirstName} {booking.Member?.LastName}|" +
                             $"Email:{booking.Member?.Email}|" +
                             $"Event:{booking.Event?.EventName}|" +
                             $"Date:{booking.Event?.EventDate:yyyy-MM-dd}|" +
                             $"Time:{booking.Event?.StartTime:hh\\:mm}|" +
                             $"Venue:{booking.Event?.Venue?.VenueName}|" +
                             $"Seat:{booking.SeatType}|" +
                             $"Qty:{booking.Quantity}|" +
                             $"Amount:${booking.TotalAmount}|" +
                             $"Status:{booking.BookingStatus}";
            ViewBag.QRCodeImage = qrService.GenerateQRCodeBase64(qrContent);

            return PartialView("_TicketPartial", booking);
        }
    }
}

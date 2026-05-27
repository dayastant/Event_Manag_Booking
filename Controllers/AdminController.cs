using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Event_Management_System.Data;
using Event_Management_System.Models;
using Event_Management_System.Helpers;

namespace Event_Management_System.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================== AUTHENTICATION ====================
        [HttpGet]
        public IActionResult Login()
        {
            // If already logged in, redirect to dashboard
            if (HttpContext.Session.GetInt32("AdminID").HasValue)
            {
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter both email and password";
                return View();
            }

            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Email == email && a.Status == "Active");

if (admin == null || string.IsNullOrEmpty(admin.PasswordHash))
{
    ViewBag.Error = "Invalid email or password";
    return View();
}

bool isValid = false;

try
{
    isValid = BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash);
}
catch
{
    ViewBag.Error = "Password format error in database";
    return View();
}

if (!isValid)
{
    ViewBag.Error = "Invalid email or password";
    return View();
}

            // Set session
            HttpContext.Session.SetInt32("AdminID", admin.AdminID);
            HttpContext.Session.SetString("AdminName", admin.FullName);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AdminID");
            HttpContext.Session.Remove("AdminName");
            return RedirectToAction(nameof(Login));
        }

        // ==================== DASHBOARD ====================

        [AdminAuthorize]
        public async Task<IActionResult> Index()
        {
            // Execute queries sequentially since DbContext is not thread-safe
            ViewBag.TotalEvents = await _context.Events.AsNoTracking().CountAsync();
            ViewBag.TotalUsers = await _context.Members.AsNoTracking().CountAsync();
            ViewBag.TotalBookings = await _context.Bookings.AsNoTracking().CountAsync();
            ViewBag.TotalReviews = await _context.Reviews.AsNoTracking().CountAsync();
            ViewBag.TotalVenues = await _context.Venues.AsNoTracking().CountAsync();
            
            // Recent events
            ViewBag.RecentEvents = await _context.Events
                .AsNoTracking()
                .Include(e => e.Venue)
                .OrderByDescending(e => e.EventDate)
                .Take(5)
                .ToListAsync();

            return View();
        }

        // ==================== EVENT MANAGEMENT ====================

        [AdminAuthorize]
        public async Task<IActionResult> Events()
        {
            // Optimized Status Updates using Bulk Operations (ExecuteUpdateAsync)
            // 1. Mark as Completed (EndDate passed OR EndDate is today but EndTime passed)
            await _context.Events
                .Where(e => e.Status != "Cancelled" && e.Status != "Completed" && 
                       (e.EndDate < DateTime.Today || (e.EndDate == DateTime.Today && e.EndTime < DateTime.Now.TimeOfDay)))
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.Status, "Completed"));

            // 2. Mark as Ongoing (StartDate <= Today <= EndDate)
            await _context.Events
                .Where(e => e.Status != "Cancelled" && e.Status != "Ongoing" &&
                       e.EventDate <= DateTime.Today && e.EndDate >= DateTime.Today &&
                       !(e.EndDate == DateTime.Today && e.EndTime < DateTime.Now.TimeOfDay))
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.Status, "Ongoing"));

            // 3. Mark as Upcoming (StartDate > Today)
            await _context.Events
                .Where(e => e.Status != "Cancelled" && e.Status != "Upcoming" && e.EventDate > DateTime.Today)
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.Status, "Upcoming"));


            var events = await _context.Events
                .AsNoTracking()
                .Include(e => e.Venue)
                .OrderByDescending(e => e.EventDate)
                .AsSplitQuery()
                .ToListAsync();

            return View(events);
        }

        [AdminAuthorize]
        [HttpGet]
        public async Task<IActionResult> CreateEvent()
        {
            ViewBag.Venues = await _context.Venues.ToListAsync();
            return View();
        }

        [AdminAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(Event model, IFormFile? eventImage)
        {
            // Auto-calculate seats from tickets if provided
            if (model.Tickets != null && model.Tickets.Any())
            {
                model.TotalSeats = model.Tickets.Sum(t => t.TotalQuantity);
                model.AvailableSeats = model.Tickets.Sum(t => t.AvailableQuantity);
                
                // Clear validation errors for these since we just calculated them
                ModelState.Remove("TotalSeats");
                ModelState.Remove("AvailableSeats");
            }

            // Backend Validation: Check Venue Capacity
            var venue = await _context.Venues.FindAsync(model.VenueID);
            if (venue != null && model.TotalSeats > venue.Capacity)
            {
                ModelState.AddModelError("TotalSeats", $"Total seats ({model.TotalSeats}) cannot exceed venue capacity ({venue.Capacity})");
            }

            if (ModelState.IsValid)
            {
                // Handle image upload
                if (eventImage != null && eventImage.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await eventImage.CopyToAsync(memoryStream);
                        model.EventImage = memoryStream.ToArray();
                        model.EventImageContentType = eventImage.ContentType;
                    }
                }

                _context.Events.Add(model);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Event created successfully";
                return RedirectToAction(nameof(Events));
            }

            ViewBag.Venues = await _context.Venues.ToListAsync();
            return View(model);
        }

        [AdminAuthorize]
        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var eventItem = await _context.Events
                .Include(e => e.Tickets)
                .FirstOrDefaultAsync(e => e.EventID == id);
            
            if (eventItem == null)
            {
                return NotFound();
            }

            ViewBag.Venues = await _context.Venues.ToListAsync();
            return View(eventItem);
        }

        [AdminAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(int id, Event model, IFormFile? eventImage)
        {
            if (id != model.EventID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingEvent = await _context.Events
                        .Include(e => e.Tickets)
                        .FirstOrDefaultAsync(e => e.EventID == id);

                    if (existingEvent == null)
                    {
                        return NotFound();
                    }

                    // Update properties
                    existingEvent.EventName = model.EventName;
                    existingEvent.Description = model.Description;
                    existingEvent.Category = model.Category;
                    existingEvent.EventDate = model.EventDate;
                    existingEvent.EndDate = model.EndDate;
                    existingEvent.StartTime = model.StartTime;
                    existingEvent.EndTime = model.EndTime;
                    existingEvent.Price = model.Price;
                    existingEvent.TotalSeats = model.TotalSeats;
                    existingEvent.AvailableSeats = model.AvailableSeats;
                    existingEvent.VenueID = model.VenueID;
                    existingEvent.Status = model.Status;

                    // Handle Ticket Sync (using existing Ticket table)
                    if (model.Tickets != null)
                    {
                        // 1. Update existing and Add new
                        foreach (var t in model.Tickets)
                        {
                            var existingTicket = existingEvent.Tickets
                                .FirstOrDefault(et => et.TicketID == t.TicketID && t.TicketID != 0);

                            if (existingTicket != null)
                            {
                                // Update properties
                                existingTicket.SeatType = t.SeatType;
                                existingTicket.Price = t.Price;
                                existingTicket.TotalQuantity = t.TotalQuantity;
                                
                                // Use admin's availability input (with validation to not exceed total)
                                existingTicket.AvailableQuantity = Math.Min(t.AvailableQuantity, t.TotalQuantity);
                            }
                            else
                            {
                                // Add new ticket
                                // For new tickets, ensure Available doesn't exceed Total
                                t.AvailableQuantity = Math.Min(t.AvailableQuantity, t.TotalQuantity);
                                existingEvent.Tickets.Add(t);
                            }
                        }

                        // 2. Delete removed
                        var formIds = model.Tickets.Select(t => t.TicketID).ToList();
                        var toRemove = existingEvent.Tickets
                            .Where(t => !formIds.Contains(t.TicketID) && t.TicketID != 0)
                            .ToList();

                        foreach (var remove in toRemove)
                        {
                            // Remove from context and in-memory list so Sum() is correct
                            _context.Tickets.Remove(remove);
                            existingEvent.Tickets.Remove(remove);
                        }

                        // 3. Update Event Total/Available Seats based on tickets
                        // We use existingEvent.Tickets which includes updates and new items (EF Core tracking)
                        if (existingEvent.Tickets.Any())
                        {
                            existingEvent.TotalSeats = existingEvent.Tickets.Sum(t => t.TotalQuantity);
                            existingEvent.AvailableSeats = existingEvent.Tickets.Sum(t => t.AvailableQuantity);
                        }
                    }

                    // Backend Validation: Check Venue Capacity
                    var venue = await _context.Venues.FindAsync(existingEvent.VenueID);
                    if (venue != null && existingEvent.TotalSeats > venue.Capacity)
                    {
                        ModelState.AddModelError("TotalSeats", $"Total seats ({existingEvent.TotalSeats}) cannot exceed venue capacity ({venue.Capacity})");
                        ViewBag.Venues = await _context.Venues.ToListAsync();
                        return View(model);
                    }

                    // Handle image upload if new image provided
                    if (eventImage != null && eventImage.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await eventImage.CopyToAsync(memoryStream);
                            existingEvent.EventImage = memoryStream.ToArray();
                            existingEvent.EventImageContentType = eventImage.ContentType;
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Event updated successfully";
                    return RedirectToAction(nameof(Events));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await EventExists(id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            ViewBag.Venues = await _context.Venues.ToListAsync();
            return View(model);
        }

        [AdminAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventItem = await _context.Events
                .Include(e => e.Tickets)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            // 1. Delete associated Bookings first
            var bookings = await _context.Bookings.Where(b => b.EventID == id).ToListAsync();
            if (bookings.Any())
            {
                _context.Bookings.RemoveRange(bookings);
            }

            // 2. Delete associated Tickets
            if (eventItem.Tickets != null && eventItem.Tickets.Any())
            {
                _context.Tickets.RemoveRange(eventItem.Tickets);
            }

            // 3. Delete Event
            _context.Events.Remove(eventItem);
            
            await _context.SaveChangesAsync();

            TempData["Success"] = "Event and associated data deleted successfully";
            return RedirectToAction(nameof(Events));
        }

        // ==================== USER MANAGEMENT ====================

        [AdminAuthorize]
        public async Task<IActionResult> Users()
        {
            var users = await _context.Members
                .AsNoTracking()
                .OrderByDescending(m => m.RegistrationDate)
                .ToListAsync();

            return View(users);
        }

        [AdminAuthorize]
        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _context.Members.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [AdminAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, Member model)
        {
            if (id != model.MemberID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Members.FindAsync(id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    // Update editable properties
                    existingUser.FirstName = model.FirstName;
                    existingUser.LastName = model.LastName;
                    existingUser.Email = model.Email;
                    existingUser.PhoneNumber = model.PhoneNumber;
                    existingUser.PreferredCategory = model.PreferredCategory;
                    existingUser.Status = model.Status;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "User updated successfully";
                    return RedirectToAction(nameof(Users));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await UserExists(id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            return View(model);
        }

        [AdminAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Members
                .Include(m => m.Bookings)
                .FirstOrDefaultAsync(m => m.MemberID == id);

            if (user == null)
            {
                return NotFound();
            }

            // Check if user has bookings
            if (user.Bookings != null && user.Bookings.Any())
            {
                TempData["Error"] = "Cannot delete user with existing bookings";
                return RedirectToAction(nameof(Users));
            }

            _context.Members.Remove(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "User deleted successfully";
            return RedirectToAction(nameof(Users));
        }

        [AdminAuthorize]
        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var user = await _context.Members.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Status = user.Status == "Active" ? "Inactive" : "Active";
            await _context.SaveChangesAsync();

            return Json(new { success = true, newStatus = user.Status });
        }

        // ==================== REVIEW MANAGEMENT ====================

        [AdminAuthorize]
        public async Task<IActionResult> Reviews()
        {
            var reviews = await _context.Reviews
                .AsNoTracking()
                .Include(r => r.Event)
                .Include(r => r.Member)
                .OrderByDescending(r => r.ReviewDate)
                .AsSplitQuery()
                .ToListAsync();

            return View(reviews);
        }

        [AdminAuthorize]
        [HttpGet]
        public async Task<IActionResult> EditReview(int id)
        {
            var review = await _context.Reviews
                .Include(r => r.Event)
                .Include(r => r.Member)
                .FirstOrDefaultAsync(r => r.ReviewID == id);

            if (review == null)
            {
                return NotFound();
            }

            ViewBag.Members = await _context.Members
                .Select(m => new { m.MemberID, m.FullName, m.Email })
                .ToListAsync();

            return View(review);
        }

        [AdminAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReview(int id, Review model)
        {
            if (id != model.ReviewID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingReview = await _context.Reviews.FindAsync(id);
                    if (existingReview == null)
                    {
                        return NotFound();
                    }

                    existingReview.Rating = model.Rating;
                    existingReview.Comment = model.Comment;
                    existingReview.MemberID = model.MemberID; // Update User

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Review updated successfully";
                    return RedirectToAction(nameof(Reviews));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ReviewExists(id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            return View(model);
        }

        [AdminAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Review deleted successfully";
            return RedirectToAction(nameof(Reviews));
        }

        // ==================== VENUE MANAGEMENT ====================

        [AdminAuthorize]
        public async Task<IActionResult> Venues()
        {
            var venues = await _context.Venues.AsNoTracking().ToListAsync();
            return View(venues);
        }

        // ==================== INQUIRIES MANAGEMENT ====================
        [AdminAuthorize]
        public async Task<IActionResult> Inquiries()
        {
            var inquiries = await _context.Inquiries
                .AsNoTracking()
                .Include(i => i.Member)
                .Include(i => i.Guest)
                .AsSplitQuery()
                .ToListAsync();
            return View(inquiries);
        }

        [AdminAuthorize]
        [HttpPost]
        public async Task<IActionResult> ApproveInquiry(int id)
        {
            try
            {
                var inquiry = await _context.Inquiries.FindAsync(id);
                if (inquiry == null)
                {
                    return Json(new { success = false, message = "Inquiry not found" });
                }

                inquiry.Status = "Resolved";
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [AdminAuthorize]
        [HttpPost]
        public async Task<IActionResult> DeleteInquiry(int id)
        {
            try
            {
                var inquiry = await _context.Inquiries.FindAsync(id);
                if (inquiry == null)
                {
                    return Json(new { success = false, message = "Inquiry not found" });
                }

                _context.Inquiries.Remove(inquiry);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [AdminAuthorize]
        [HttpGet]
        public IActionResult CreateVenue()
        {
            return View();
        }

        [AdminAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVenue(Venue model)
        {
            if (ModelState.IsValid)
            {
                _context.Venues.Add(model);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Venue created successfully";
                return RedirectToAction(nameof(Venues));
            }

            return View(model);
        }

        [AdminAuthorize]
        [HttpGet]
        public async Task<IActionResult> EditVenue(int id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        [AdminAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVenue(int id, Venue model)
        {
            if (id != model.VenueID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Venue updated successfully";
                    return RedirectToAction(nameof(Venues));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await VenueExists(id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            return View(model);
        }

        [AdminAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVenue(int id)
        {
            var venue = await _context.Venues
                .Include(v => v.Events)
                .FirstOrDefaultAsync(v => v.VenueID == id);

            if (venue == null)
            {
                return NotFound();
            }

            // Check if venue has events
            if (venue.Events != null && venue.Events.Any())
            {
                TempData["Error"] = "Cannot delete venue with existing events";
                return RedirectToAction(nameof(Venues));
            }

            _context.Venues.Remove(venue);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Venue deleted successfully";
            return RedirectToAction(nameof(Venues));
        }

        // ==================== HELPER METHODS ====================

        private async Task<bool> EventExists(int id)
        {
            return await _context.Events.AnyAsync(e => e.EventID == id);
        }

        private async Task<bool> UserExists(int id)
        {
            return await _context.Members.AnyAsync(m => m.MemberID == id);
        }

        private async Task<bool> ReviewExists(int id)
        {
            return await _context.Reviews.AnyAsync(r => r.ReviewID == id);
        }

        private async Task<bool> VenueExists(int id)
        {
            return await _context.Venues.AnyAsync(v => v.VenueID == id);
        }
    }
}

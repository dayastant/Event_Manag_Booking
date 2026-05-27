using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Event_Management_System.Data;
using Event_Management_System.Models;
using Event_Management_System.Models.ViewModels;
using Event_Management_System.Helpers;

namespace Event_Management_System.Controllers
{
    [AuthorizeSession]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Dashboard/Index
        public async Task<IActionResult> Index()
        {
            var memberId = HttpContext.Session.GetInt32("MemberID");
            
            if (!memberId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var member = await _context.Members.FindAsync(memberId.Value);
            
            if (member == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = new DashboardViewModel
            {
                Member = member,
                TotalBookings = await _context.Bookings
                    .AsNoTracking()
                    .Where(b => b.MemberID == memberId.Value && b.BookingStatus == "Confirmed")
                    .CountAsync(),
                TotalReviews = await _context.Reviews
                    .AsNoTracking()
                    .Where(r => r.MemberID == memberId.Value)
                    .CountAsync(),
                UpcomingEvents = await _context.Bookings
                    .AsNoTracking()
                    .Include(b => b.Event)
                    .Where(b => b.MemberID == memberId.Value && 
                               b.BookingStatus == "Confirmed" &&
                               b.Event!.EventDate >= DateTime.Today)
                    .CountAsync(),
                RecentBookings = await _context.Bookings
                    .AsNoTracking()
                    .Include(b => b.Event)
                        .ThenInclude(e => e!.Venue)
                    .Where(b => b.MemberID == memberId.Value)
                    .OrderByDescending(b => b.BookingDate)
                    .Take(5)
                    .AsSplitQuery()
                    .ToListAsync(),
                RecentReviews = await _context.Reviews
                    .AsNoTracking()
                    .Include(r => r.Event)
                    .Where(r => r.MemberID == memberId.Value)
                    .OrderByDescending(r => r.ReviewDate)
                    .Take(5)
                    .AsSplitQuery()
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // GET: Dashboard/MyBookings
        public async Task<IActionResult> MyBookings()
        {
            var memberId = HttpContext.Session.GetInt32("MemberID");
            
            if (!memberId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var bookings = await _context.Bookings
                    .AsNoTracking()
                    .Include(b => b.Event)
                        .ThenInclude(e => e!.Venue)
                .Where(b => b.MemberID == memberId.Value)
                .OrderByDescending(b => b.BookingDate)
                .AsSplitQuery()
                .ToListAsync();

            return View(bookings);
        }

        // GET: Dashboard/MyReviews
        public async Task<IActionResult> MyReviews()
        {
            var memberId = HttpContext.Session.GetInt32("MemberID");
            
            if (!memberId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var reviews = await _context.Reviews
                .AsNoTracking()
                .Include(r => r.Event)
                .Where(r => r.MemberID == memberId.Value)
                .OrderByDescending(r => r.ReviewDate)
                .AsSplitQuery()
                .ToListAsync();

            return View(reviews);
        }

        // GET: Dashboard/Settings
        public async Task<IActionResult> Settings()
        {
            var memberId = HttpContext.Session.GetInt32("MemberID");
            
            if (!memberId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var member = await _context.Members.FindAsync(memberId.Value);
            
            if (member == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(member);
        }

        // POST: Dashboard/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([FromForm] Member model)
        {
            try
            {
                var memberId = HttpContext.Session.GetInt32("MemberID");
                
                if (!memberId.HasValue)
                {
                    return Json(new { success = false, message = "Session expired. Please login again." });
                }

                var member = await _context.Members.FindAsync(memberId.Value);
                
                if (member == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                // Update only allowed fields
                // Note: Email is not updated as it is uniquely identifying/readonly
                member.FirstName = model.FirstName;
                member.LastName = model.LastName;
                member.PhoneNumber = model.PhoneNumber;
                member.PreferredCategory = model.PreferredCategory;

                _context.Members.Update(member);
                await _context.SaveChangesAsync();

                // Update session name if changed
                HttpContext.Session.SetString("MemberName", $"{member.FirstName} {member.LastName}");

                return Json(new { success = true, message = "Profile updated successfully!" });
            }
            catch (Exception ex)
            {
                // Log the exception (if logger is available)
                return Json(new { success = false, message = "An error occurred while updating profile: " + ex.Message });
            }
        }


        // POST: Dashboard/UploadProfilePhoto (AJAX)
        [HttpPost]
        public async Task<IActionResult> UploadProfilePhoto(IFormFile profilePhoto)
        {
            var memberId = HttpContext.Session.GetInt32("MemberID");
            
            if (!memberId.HasValue)
            {
                return Json(new { success = false, message = "Not authenticated" });
            }

            if (profilePhoto == null || profilePhoto.Length == 0)
            {
                return Json(new { success = false, message = "No file uploaded" });
            }

            // Validate file size (max 2MB)
            if (profilePhoto.Length > 2 * 1024 * 1024)
            {
                return Json(new { success = false, message = "File size must be less than 2MB" });
            }

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(profilePhoto.ContentType.ToLower()))
            {
                return Json(new { success = false, message = "Only image files are allowed" });
            }

            var member = await _context.Members.FindAsync(memberId.Value);
            if (member == null)
            {
                return Json(new { success = false, message = "Member not found" });
            }

            // Convert to byte array
            using (var memoryStream = new MemoryStream())
            {
                await profilePhoto.CopyToAsync(memoryStream);
                member.ProfilePhoto = memoryStream.ToArray();
                member.ProfilePhotoContentType = profilePhoto.ContentType;
            }

            _context.Members.Update(member);
            await _context.SaveChangesAsync();

            // Return base64 image for preview
            var base64Image = member.ProfilePhotoBase64;
            
            // Update session
            HttpContext.Session.SetString("ProfilePhotoUrl", base64Image ?? "/images/prf.png");

            return Json(new { success = true, message = "Profile photo updated successfully", imageUrl = base64Image });
        }

        // POST: Dashboard/ChangePassword (AJAX)
        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            var memberId = HttpContext.Session.GetInt32("MemberID");
            
            if (!memberId.HasValue)
            {
                return Json(new { success = false, message = "Not authenticated" });
            }

            // Validate inputs
            if (string.IsNullOrWhiteSpace(model.CurrentPassword) || 
                string.IsNullOrWhiteSpace(model.NewPassword) || 
                string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                return Json(new { success = false, message = "All fields are required" });
            }

            // Check if new password matches confirm password
            if (model.NewPassword != model.ConfirmPassword)
            {
                return Json(new { success = false, message = "New password and confirmation do not match" });
            }

            // Validate new password strength
            if (model.NewPassword.Length < 8)
            {
                return Json(new { success = false, message = "Password must be at least 8 characters long" });
            }

            if (!model.NewPassword.Any(char.IsUpper))
            {
                return Json(new { success = false, message = "Password must contain at least one uppercase letter" });
            }

            if (!model.NewPassword.Any(char.IsLower))
            {
                return Json(new { success = false, message = "Password must contain at least one lowercase letter" });
            }

            if (!model.NewPassword.Any(char.IsDigit))
            {
                return Json(new { success = false, message = "Password must contain at least one number" });
            }

            if (!model.NewPassword.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                return Json(new { success = false, message = "Password must contain at least one special character" });
            }

            var member = await _context.Members.FindAsync(memberId.Value);
            if (member == null)
            {
                return Json(new { success = false, message = "Member not found" });
            }

            // Verify current password using AuthHelper
            if (!AuthHelper.VerifyPassword(model.CurrentPassword, member.PasswordHash))
            {
                return Json(new { success = false, message = "Current password is incorrect" });
            }

            // Update password with proper hashing
            member.PasswordHash = AuthHelper.HashPassword(model.NewPassword);
            
            _context.Members.Update(member);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Password changed successfully!" });
        }


        // GET: Dashboard/GetBookings (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetBookings()
        {
            var memberId = HttpContext.Session.GetInt32("MemberID");
            
            if (!memberId.HasValue)
            {
                return Json(new { success = false });
            }

            var bookings = await _context.Bookings
                .AsNoTracking()
                .Include(b => b.Event)
                .Where(b => b.MemberID == memberId.Value)
                .OrderByDescending(b => b.BookingDate)
                .Select(b => new
                {
                    bookingId = b.BookingID,
                    eventName = b.Event!.EventName,
                    eventDate = b.Event.EventDate.ToString("MMM dd, yyyy"),
                    quantity = b.Quantity,
                    totalAmount = b.TotalAmount,
                    reference = $"BK{b.BookingID:D6}",
                    status = b.BookingStatus
                })
                .ToListAsync();

            return Json(new { success = true, bookings });
        }
        
        // POST: Dashboard/DeleteBooking (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBooking([FromForm] int BookingId)
        {
            try
            {
                var memberId = HttpContext.Session.GetInt32("MemberID");
                
                if (!memberId.HasValue)
                {
                    return Json(new { success = false, message = "Not authenticated" });
                }

                if (BookingId <= 0)
                {
                    return Json(new { success = false, message = "Invalid booking ID" });
                }

                var booking = await _context.Bookings
                    .Include(b => b.Event)
                    .ThenInclude(e => e!.Venue)
                    .FirstOrDefaultAsync(b => b.BookingID == BookingId && b.MemberID == memberId.Value);

                if (booking == null)
                {
                    // It might be already deleted, so just say success to clear UI
                    return Json(new { success = true, message = "Booking already removed." });
                }

                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Booking deleted successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting booking: {ex}");
                return Json(new { success = false, message = "Server error: " + ex.Message });
            }
        }

        public class DeleteBookingRequest
        {
            public int BookingId { get; set; }
        }

        // POST: Dashboard/DeleteReview (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview([FromForm] int ReviewId)
        {
            try
            {
                var memberId = HttpContext.Session.GetInt32("MemberID");
                
                if (!memberId.HasValue)
                {
                    return Json(new { success = false, message = "Not authenticated" });
                }

                var review = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.ReviewID == ReviewId && r.MemberID == memberId.Value);

                if (review == null)
                {
                     return Json(new { success = true, message = "Review already removed." });
                }

                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Review deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting review: " + ex.Message });
            }
        }
        
        public class DeleteReviewRequest
        {
            public int ReviewId { get; set; }
        }

        // GET: Dashboard/GetReviews (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetReviews()
        {
            var memberId = HttpContext.Session.GetInt32("MemberID");
            
            if (!memberId.HasValue)
            {
                return Json(new { success = false });
            }

            var reviews = await _context.Reviews
                .AsNoTracking()
                .Include(r => r.Event)
                .Where(r => r.MemberID == memberId.Value)
                .OrderByDescending(r => r.ReviewDate)
                .Select(r => new
                {
                    reviewId = r.ReviewID,
                    eventName = r.Event!.EventName,
                    rating = r.Rating,
                    comment = r.Comment,
                    reviewDate = r.ReviewDate.ToString("MMM dd, yyyy")
                })
                .ToListAsync();

            return Json(new { success = true, reviews });
        }
    }
}

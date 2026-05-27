using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Event_Management_System.Data;
using Event_Management_System.Models;
using Event_Management_System.Models.ViewModels;
using Event_Management_System.Helpers;

namespace Event_Management_System.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: Review/Create (AJAX)
        [HttpPost]
        [AuthorizeSession]
        public async Task<IActionResult> Create([FromBody] ReviewViewModel model)
        {
            var memberId = HttpContext.Session.GetInt32("MemberID");
            
            if (!memberId.HasValue)
            {
                return Json(new { success = false, message = "Please login to submit a review." });
            }

            // Server-side validation - check ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return Json(new { 
                    success = false, 
                    message = errors.Any() ? errors.First() : "Invalid review data." 
                });
            }

            // Additional validation: Rating range check (server-side)
            if (model.Rating < 1 || model.Rating > 5)
            {
                return Json(new { success = false, message = "Rating must be between 1 and 5 stars." });
            }

            // Additional validation: Comment length check (server-side)
            var comment = model.Comment?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(comment))
            {
                return Json(new { success = false, message = "Please share your feedback." });
            }

            if (comment.Length < 10)
            {
                return Json(new { success = false, message = "Review must be at least 10 characters long." });
            }

            if (comment.Length > 500)
            {
                return Json(new { success = false, message = "Review cannot exceed 500 characters." });
            }

            // Sanitize comment (basic XSS protection)
            model.Comment = System.Net.WebUtility.HtmlEncode(comment);

            // Validate event exists
            var eventExists = await _context.Events.AnyAsync(e => e.EventID == model.EventID);
            if (!eventExists)
            {
                return Json(new { success = false, message = "Event not found." });
            }

            // Check if user already reviewed this event
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.EventID == model.EventID && r.MemberID == memberId.Value);

            if (existingReview != null)
            {
                return Json(new { success = false, message = "You have already reviewed this event." });
            }

            var review = new Review
            {
                MemberID = memberId.Value,
                EventID = model.EventID,
                Rating = model.Rating,
                Comment = model.Comment,
                ReviewDate = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                message = "Thank you! Your review has been submitted successfully.",
                reviewId = review.ReviewID
            });
        }

        // GET: Review/GetReviews/5 (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetReviews(int eventId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.Member)
                .Where(r => r.EventID == eventId)
                .OrderByDescending(r => r.ReviewDate)
                .Select(r => new
                {
                    reviewId = r.ReviewID,
                    memberName = r.Member!.FirstName + " " + r.Member.LastName,
                    rating = r.Rating,
                    comment = r.Comment,
                    reviewDate = r.ReviewDate.ToString("MMM dd, yyyy")
                })
                .ToListAsync();

            var averageRating = reviews.Any() ? reviews.Average(r => r.rating ?? 0) : 0;

            return Json(new { 
                success = true, 
                reviews,
                averageRating = Math.Round(averageRating, 1)
            });
        }

        // POST: Review/Edit/5 (AJAX)
        [HttpPost]
        [AuthorizeSession]
        public async Task<IActionResult> Edit(int id, [FromBody] ReviewViewModel model)
        {
            var memberId = HttpContext.Session.GetInt32("MemberID");
            
            if (!memberId.HasValue)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewID == id && r.MemberID == memberId.Value);

            if (review == null)
            {
                return Json(new { success = false, message = "Review not found." });
            }

            review.Rating = model.Rating;
            review.Comment = model.Comment;
            review.ReviewDate = DateTime.Now;

            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Review updated successfully!" });
        }

        // POST: Review/Delete/5
        // POST: Review/Delete/5
        [HttpPost]
        [AuthorizeSession]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try 
            {
                var memberId = HttpContext.Session.GetInt32("MemberID");
                
                if (!memberId.HasValue)
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var review = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.ReviewID == id && r.MemberID == memberId.Value);

                if (review == null)
                {
                    return Json(new { success = false, message = "Review not found or could not be deleted." });
                }

                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Review deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting review: " + ex.Message });
            }
        }
    }
}

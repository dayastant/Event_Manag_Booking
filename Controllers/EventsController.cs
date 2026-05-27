using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Event_Management_System.Data;
using Event_Management_System.Models;
using Event_Management_System.Models.ViewModels;
using Event_Management_System.Helpers;
using System.Linq;
using System.Collections.Generic;

namespace Event_Management_System.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Events/Index
public async Task<IActionResult> Index(string? search, string? category, string? status, int? rating, 
    decimal? minPrice, decimal? maxPrice, DateTime? startDate, DateTime? endDate, string? availability)
{
    // Build query with AsNoTracking for better performance (read-only)
    var query = _context.Events
        .AsNoTracking()
        .Include(e => e.Venue)
        .Include(e => e.Tickets)
        .Include(e => e.Reviews)
        .AsQueryable();

    // Predefined category and status options (matching Admin page)
    var allCategories = new List<string> { "Concert", "Play", "Exhibition", "Workshop", "Festival", "Other" };
    var allStatuses = new List<string> { "Upcoming", "Ongoing", "Completed", "Cancelled" };

    ViewBag.Categories = allCategories;
    ViewBag.Statuses = allStatuses;
    ViewBag.CurrentSearch = search;
    ViewBag.CurrentCategory = category;
    ViewBag.CurrentStatus = status;
    ViewBag.CurrentRating = rating;
    ViewBag.MinPrice = minPrice;
    ViewBag.MaxPrice = maxPrice;
    ViewBag.StartDate = startDate;
    ViewBag.EndDate = endDate;
    ViewBag.Availability = availability;

    // Status filter
    if (!string.IsNullOrWhiteSpace(status))
    {
       query = query.Where(e => e.Status == status);
    }

    // Search filter - Enhanced for incremental search
    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(e => e.EventName.Contains(search) || 
                                (e.Description != null && e.Description.Contains(search)));
    }

    // Category filter
    if (!string.IsNullOrWhiteSpace(category))
    {
        query = query.Where(e => e.Category == category);
    }

    // Rating filter - filter by minimum average rating
    if (rating.HasValue && rating.Value > 0)
    {
        query = query.Where(e => e.Reviews != null && e.Reviews.Any() && 
                                e.Reviews.Average(r => r.Rating ?? 0) >= rating.Value);
    }

    // Availability filter
    if (!string.IsNullOrWhiteSpace(availability))
    {
        if (availability.ToLower() == "available")
        {
            query = query.Where(e => e.AvailableSeats > 0);
        }
        else if (availability.ToLower() == "full")
        {
            query = query.Where(e => e.AvailableSeats == 0);
        }
    }

    // Date range filter
    if (startDate.HasValue)
    {
        query = query.Where(e => e.EventDate >= startDate.Value);
    }
    if (endDate.HasValue)
    {
        query = query.Where(e => e.EventDate <= endDate.Value);
    }

    // Price range filter - applied at database level
    if (minPrice.HasValue || maxPrice.HasValue)
    {
        // Filter events that have tickets within the price range
        query = query.Where(e => e.Tickets.Any(t => 
            (!minPrice.HasValue || t.Price >= minPrice.Value) &&
            (!maxPrice.HasValue || t.Price <= maxPrice.Value)
        ));
    }

    // Sorting: Recent Created First (EventID Descending) as requested
    var events = await query.OrderByDescending(e => e.EventID)
                            .Take(50) // Limit to 50 for performance
                            .AsSplitQuery()
                            .ToListAsync();
    
    // Calculate minimum price for each event
    foreach (var eventItem in events)
    {
        if (eventItem.Tickets != null && eventItem.Tickets.Any())
        {
            eventItem.Price = eventItem.Tickets.Min(t => t.Price);
        }
    }

    return View(events);
}
        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventItem = await _context.Events
                .AsNoTracking()
                .Include(e => e.Venue)
                .Include(e => e.Tickets)
                .Include(e => e.Reviews)
                    .ThenInclude(r => r.Member)
                .AsSplitQuery()
                .FirstOrDefaultAsync(m => m.EventID == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            // Ensure Ticket and Reviews lists are initialized
            if (eventItem.Tickets == null) eventItem.Tickets = new List<Ticket>();
            if (eventItem.Reviews == null) eventItem.Reviews = new List<Review>();
            
            // Calculate minimum price
            if (eventItem.Tickets.Any())
            {
                eventItem.Price = eventItem.Tickets.Min(t => t.Price);
            }

            return View(eventItem);
        }

        // POST: Events/Filter (AJAX)
        [HttpPost]
        public async Task<IActionResult> Filter([FromBody] EventFilterViewModel filter)
        {
            var query = _context.Events
                .Include(e => e.Venue)
                .Where(e => e.Status == "Upcoming" && e.EventDate >= DateTime.Today)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                query = query.Where(e => e.EventName.Contains(filter.SearchText) || 
                                        e.Description!.Contains(filter.SearchText));
            }

            if (!string.IsNullOrWhiteSpace(filter.Category) && filter.Category != "all")
            {
                query = query.Where(e => e.Category.ToLower() == filter.Category.ToLower());
            }

            if (filter.EventDate.HasValue)
            {
                query = query.Where(e => e.EventDate.Date == filter.EventDate.Value.Date);
            }

            var events = await query.OrderBy(e => e.EventDate).ToListAsync();
            
            return Json(new { success = true, events });
        }
        
        // GET: Events/GetReviews (AJAX)
        public async Task<IActionResult> GetReviews(int eventId)
        {
            var reviews = await _context.Reviews
                .AsNoTracking()
                .Include(r => r.Member)
                .Where(r => r.EventID == eventId)
                .OrderByDescending(r => r.ReviewDate)
                .Select(r => new
                {
                    r.ReviewID,
                    r.Rating,
                    r.Title,
                    r.Comment,
                    r.ReviewDate,
                    r.IsRecommended,
                    MemberName = r.Member!.FirstName + " " + r.Member.LastName
                })
                .ToListAsync();

            return Json(reviews);
        }
    }
}

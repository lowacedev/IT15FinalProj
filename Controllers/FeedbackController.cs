using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ITSMS.Data;
using ITSMS.Models;

namespace ITSMS.Controllers
{
    /// <summary>
    /// Feedback Controller - Handles customer feedback on service requests
    /// Authorization: Client (provide feedback on own requests), Admin/Technician (view feedback)
    /// </summary>
    [Authorize]
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeedbackController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Feedback/Create/5
        [HttpGet]
        [Authorize(Roles = "Client")]
        public IActionResult Create(int requestId)
        {
            var request = _context.ServiceRequests
                .Include(sr => sr.Requestor)
                .FirstOrDefault(sr => sr.RequestId == requestId);

            if (request == null)
                return NotFound();

            // Check if feedback already exists
            var existingFeedback = _context.Feedbacks.FirstOrDefault(f => f.RequestId == requestId);
            if (existingFeedback != null)
            {
                TempData["Warning"] = "Feedback for this request already exists.";
                return RedirectToAction("Details", "ServiceRequests", new { id = requestId });
            }

            // Check if user is the requestor
            var userId = GetCurrentUserId();
            if (request.RequestorId != userId)
                return Forbid();

            // Check if request is resolved or closed
            if (request.Status != ServiceRequestStatus.Resolved && request.Status != ServiceRequestStatus.Closed)
            {
                TempData["Warning"] = "Feedback can only be provided for resolved or closed requests.";
                return RedirectToAction("Details", "ServiceRequests", new { id = requestId });
            }

            var model = new Feedback { RequestId = requestId };
            return View(model);
        }

        // POST: Feedback/Create/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Create(Feedback feedback)
        {
            var userId = GetCurrentUserId();
            var request = _context.ServiceRequests.FirstOrDefault(sr => sr.RequestId == feedback.RequestId);

            if (request == null || request.RequestorId != userId)
                return Forbid();

            feedback.ProvidedBy = userId;
            feedback.ProvidedAt = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                _context.Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thank you for your feedback!";
                return RedirectToAction("Details", "ServiceRequests", new { id = feedback.RequestId });
            }

            return View(feedback);
        }

        // GET: Feedback/Edit/5
        [HttpGet]
        [Authorize(Roles = "Client")]
        public IActionResult Edit(int id)
        {
            var feedback = _context.Feedbacks.FirstOrDefault(f => f.FeedbackId == id);
            if (feedback == null)
                return NotFound();

            var userId = GetCurrentUserId();
            if (feedback.ProvidedBy != userId)
                return Forbid();

            return View(feedback);
        }

        // POST: Feedback/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Edit(int id, Feedback feedback)
        {
            if (id != feedback.FeedbackId)
                return NotFound();

            var existingFeedback = _context.Feedbacks.FirstOrDefault(f => f.FeedbackId == id);
            if (existingFeedback == null)
                return NotFound();

            var userId = GetCurrentUserId();
            if (existingFeedback.ProvidedBy != userId)
                return Forbid();

            existingFeedback.Rating = feedback.Rating;
            existingFeedback.Comments = feedback.Comments;

            try
            {
                _context.Feedbacks.Update(existingFeedback);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Feedback updated successfully.";
                return RedirectToAction("Details", "ServiceRequests", new { id = existingFeedback.RequestId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating feedback: {ex.Message}");
            }

            return View(feedback);
        }

        // GET: Feedback/Statistics (Admin only)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Statistics()
        {
            var feedbacks = _context.Feedbacks
                .Include(f => f.Request)
                .Include(f => f.User)
                .ToList();

            var statistics = new
            {
                TotalFeedback = feedbacks.Count,
                AverageRating = feedbacks.Count > 0 ? feedbacks.Average(f => f.Rating) : 0,
                RatingDistribution = feedbacks.GroupBy(f => f.Rating)
                    .ToDictionary(g => g.Key, g => g.Count()),
                RecentFeedback = feedbacks.OrderByDescending(f => f.ProvidedAt).Take(10).ToList()
            };

            ViewData["Statistics"] = statistics;
            return View(feedbacks);
        }

        // ==================== HELPER METHODS ====================

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim?.Value, out var userId) ? userId : 0;
        }
    }
}

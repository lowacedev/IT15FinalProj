using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ITSMS.Data;
using ITSMS.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ITSMS.Controllers
{
    /// <summary>
    /// TechnicianDashboard Controller - Dedicated dashboard for technicians showing assigned tasks
    /// Authorization: Technician role only
    /// </summary>
    [Route("[controller]")]
    [Authorize(Roles = "Technician")]
    public class TechnicianDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ITSMS.Services.NotificationService _notificationService;

        public TechnicianDashboardController(ApplicationDbContext context, ITSMS.Services.NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // GET: TechnicianDashboard/Index
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            var userId = GetCurrentUserId();

            // Get all assigned service requests for this technician
            var assignedRequests = _context.ServiceRequests
                .Include(sr => sr.Category)
                .Include(sr => sr.Requestor)
                .Where(sr => sr.AssignedTechnicianId == userId)
                .OrderByDescending(sr => sr.CreatedAt)
                .ToList();

            // Calculate summary statistics
            var totalAssignedCount = assignedRequests.Count;
            var ongoingCount = assignedRequests.Count(sr => sr.Status == ServiceRequestStatus.InProgress);
            var completedCount = assignedRequests.Count(sr => sr.Status == ServiceRequestStatus.Resolved || sr.Status == ServiceRequestStatus.Closed);

            // Fetch recent feedbacks for this technician
            var recentFeedbacks = _context.Feedbacks
                .Include(f => f.Request)
                .Where(f => f.Request != null && f.Request.AssignedTechnicianId == userId)
                .OrderByDescending(f => f.ProvidedAt)
                .Take(5)
                .ToList();

            ViewData["AssignedCount"] = totalAssignedCount;
            ViewData["OngoingCount"] = ongoingCount;
            ViewData["CompletedCount"] = completedCount;
            ViewData["AssignedRequests"] = assignedRequests;
            ViewData["RecentFeedbacks"] = recentFeedbacks;

            return View();
        }

        // GET: TechnicianDashboard/UpdateStatus/5
        [HttpGet("UpdateStatus/{requestId}")]
        public IActionResult UpdateStatus(int requestId)
        {
            var userId = GetCurrentUserId();
            var request = _context.ServiceRequests
                .FirstOrDefault(sr => sr.RequestId == requestId && sr.AssignedTechnicianId == userId);

            if (request == null)
                return NotFound();

            return View(request);
        }

        // POST: TechnicianDashboard/UpdateStatus/5
        [HttpPost("UpdateStatus/{requestId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int requestId, ServiceRequestStatus status)
        {
            var userId = GetCurrentUserId();
            var request = _context.ServiceRequests
                .FirstOrDefault(sr => sr.RequestId == requestId && sr.AssignedTechnicianId == userId);

            if (request == null)
                return NotFound();

            // Update the status
            var previousStatus = request.Status;
            request.Status = status;
            request.UpdatedAt = DateTime.Now;

            // Set resolved/closed timestamps
            if (status == ServiceRequestStatus.Resolved && request.ResolvedAt == null)
                request.ResolvedAt = DateTime.Now;

            if (status == ServiceRequestStatus.Closed && request.ClosedAt == null)
                request.ClosedAt = DateTime.Now;

            // Log activity
            var activityLog = new ActivityLog
            {
                UserId = userId,
                Action = $"Updated service request status from {previousStatus} to {status}",
                Entity = "ServiceRequest",
                EntityId = requestId,
                LoggedAt = DateTime.Now
            };

            _context.ServiceRequests.Update(request);
            _context.ActivityLogs.Add(activityLog);
            await _context.SaveChangesAsync();

            // Notify requestor via SignalR
            await _notificationService.SendStatusNotification(
                request.RequestorId.ToString(),
                request.RequestNumber ?? request.RequestId.ToString(),
                status.ToString(),
                $"Status updated to {status} by technician."
            );

            return RedirectToAction("Index", new { success = "Status updated successfully" });
        }

        // Helper method to get current user ID
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }
}

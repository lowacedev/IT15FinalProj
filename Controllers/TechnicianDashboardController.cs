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

        public TechnicianDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TechnicianDashboard/Index
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            var userId = GetCurrentUserId();

            // Get all assigned service requests for this technician
            var assignedRequests = _context.ServiceRequests
                .Where(sr => sr.AssignedTechnicianId == userId)
                .ToList();

            // Calculate summary statistics
            var pendingCount = assignedRequests.Count(sr => sr.Status == ServiceRequestStatus.Pending);
            var ongoingCount = assignedRequests.Count(sr => sr.Status == ServiceRequestStatus.InProgress);
            var completedCount = assignedRequests.Count(sr => sr.Status == ServiceRequestStatus.Resolved || sr.Status == ServiceRequestStatus.Closed);

            ViewData["PendingCount"] = pendingCount;
            ViewData["OngoingCount"] = ongoingCount;
            ViewData["CompletedCount"] = completedCount;

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
            request.UpdatedAt = DateTime.UtcNow;

            // Set resolved/closed timestamps
            if (status == ServiceRequestStatus.Resolved && request.ResolvedAt == null)
                request.ResolvedAt = DateTime.UtcNow;

            if (status == ServiceRequestStatus.Closed && request.ClosedAt == null)
                request.ClosedAt = DateTime.UtcNow;

            // Log activity
            var activityLog = new ActivityLog
            {
                UserId = userId,
                Action = $"Updated service request status from {previousStatus} to {status}",
                Entity = "ServiceRequest",
                EntityId = requestId,
                LoggedAt = DateTime.UtcNow
            };

            _context.ServiceRequests.Update(request);
            _context.ActivityLogs.Add(activityLog);
            await _context.SaveChangesAsync();

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

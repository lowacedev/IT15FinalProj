using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ITSMS.Data;
using ITSMS.Models;

namespace ITSMS.Controllers
{
    /// <summary>
    /// Assignments Controller - Handles assignment of service requests to technicians
    /// Authorization: Admin only
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AssignmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AssignmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Assignments/Assign/5
        [HttpGet]
        public IActionResult Assign(int requestId)
        {
            var request = _context.ServiceRequests
                .Include(sr => sr.Requestor)
                .Include(sr => sr.Category)
                .FirstOrDefault(sr => sr.RequestId == requestId);

            if (request == null)
                return NotFound();

            // Get list of available technicians
            var technicians = _context.Users
                .Where(u => u.IsActive && u.Role.RoleName == "Technician")
                .ToList();

            ViewData["Technicians"] = technicians;
            return View(request);
        }

        // POST: Assignments/Assign/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int requestId, int technicianId, string notes)
        {
            var request = _context.ServiceRequests.FirstOrDefault(sr => sr.RequestId == requestId);
            if (request == null)
                return NotFound();

            var technician = _context.Users.FirstOrDefault(u => u.UserId == technicianId && u.IsActive);
            if (technician == null)
            {
                ModelState.AddModelError("", "Invalid technician selected.");
                return BadRequest();
            }

            var userId = GetCurrentUserId();

            // If request was previously assigned, mark old assignment as inactive
            var previousAssignment = _context.Assignments
                .FirstOrDefault(a => a.RequestId == requestId && a.IsActive);

            if (previousAssignment != null)
            {
                previousAssignment.IsActive = false;
                _context.Assignments.Update(previousAssignment);
            }

            // Create new assignment
            var newAssignment = new Assignment
            {
                RequestId = requestId,
                TechnicianId = technicianId,
                AssignedBy = userId,
                AssignedAt = DateTime.UtcNow,
                IsActive = true,
                Notes = notes
            };

            request.AssignedTechnicianId = technicianId;
            request.Status = ServiceRequestStatus.InProgress;
            request.UpdatedAt = DateTime.UtcNow;

            _context.Assignments.Add(newAssignment);
            _context.ServiceRequests.Update(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Service request assigned to {technician.FullName}.";
            return RedirectToAction("Details", "ServiceRequests", new { id = requestId });
        }

        // GET: Assignments/WorkLoad
        [HttpGet]
        public IActionResult Workload()
        {
            var technicians = _context.Users
                .Where(u => u.IsActive && u.Role.RoleName == "Technician")
                .Include(u => u.RequestsAssigned)
                .ToList();

            var workloadData = new Dictionary<string, int>();

            foreach (var tech in technicians)
            {
                var openRequests = _context.ServiceRequests
                    .Count(sr => sr.AssignedTechnicianId == tech.UserId && 
                           (sr.Status == ServiceRequestStatus.Open || sr.Status == ServiceRequestStatus.InProgress));
                workloadData[tech.FullName] = openRequests;
            }

            return View(workloadData);
        }

        // GET: Assignments/History/5
        [HttpGet]
        public IActionResult History(int requestId)
        {
            var request = _context.ServiceRequests.FirstOrDefault(sr => sr.RequestId == requestId);
            if (request == null)
                return NotFound();

            var assignments = _context.Assignments
                .Where(a => a.RequestId == requestId)
                .Include(a => a.Technician)
                .Include(a => a.AssignedByUser)
                .OrderByDescending(a => a.AssignedAt)
                .ToList();

            ViewData["Request"] = request;
            return View(assignments);
        }

        // ==================== HELPER METHODS ====================

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim?.Value, out var userId) ? userId : 0;
        }
    }
}

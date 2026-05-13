using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ITSMS.Data;
using ITSMS.Models;
using Microsoft.EntityFrameworkCore;

namespace ITSMS.Controllers
{
    /// <summary>
    /// Assignments Controller - Handles assignment of service requests to technicians
    /// Authorization: Admin for assignments, Technician for viewing own workload
    /// </summary>
    [Route("[controller]")]
    [Authorize]
    public class AssignmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ITSMS.Services.AuditService _auditService;

        public AssignmentsController(ApplicationDbContext context, ITSMS.Services.AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // GET: Assignments/Assign/5
        [HttpGet("Assign/{requestId}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
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
                .Where(u => u.IsActive && u.Role != null && u.Role.RoleName == "Technician")
                .ToList();

            ViewData["Technicians"] = technicians;
            return View(request);
        }

        // POST: Assignments/Assign/5
        [HttpPost("Assign/{requestId}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperAdmin")]
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
                AssignedAt = DateTime.Now,
                IsActive = true,
                Notes = notes
            };

            request.AssignedTechnicianId = technicianId;
            request.Status = ServiceRequestStatus.InProgress;
            request.UpdatedAt = DateTime.Now;

            _context.Assignments.Add(newAssignment);
            _context.ServiceRequests.Update(request);
            await _context.SaveChangesAsync();

            _auditService.Log(userId, "ASSIGN", "ServiceRequest", $"Assigned to technician {technician.FullName}");

            TempData["Success"] = $"Service request assigned to {technician.FullName}.";
            return RedirectToAction("Details", "ServiceRequests", new { id = requestId });
        }

        // POST: Assignments/AssignTechnicianQuick
        [HttpPost("AssignTechnicianQuick")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> AssignTechnicianQuick(int requestId, int technicianId)
        {
            var request = _context.ServiceRequests.FirstOrDefault(sr => sr.RequestId == requestId);
            if (request == null)
                return BadRequest("Request not found");

            var technician = _context.Users.FirstOrDefault(u => u.UserId == technicianId && u.IsActive);
            if (technician == null)
                return BadRequest("Invalid technician selected");

            var userId = GetCurrentUserId();

            // If request was previously assigned, mark old assignment as inactive
            var previousAssignment = _context.Assignments
                .FirstOrDefault(a => a.RequestId == requestId && a.IsActive);

            string? previousTechnicianName = null;
            if (previousAssignment != null)
            {
                var previousTech = _context.Users.FirstOrDefault(u => u.UserId == previousAssignment.TechnicianId);
                previousTechnicianName = previousTech?.FullName;
                previousAssignment.IsActive = false;
                _context.Assignments.Update(previousAssignment);
            }

            // Create new assignment
            var newAssignment = new Assignment
            {
                RequestId = requestId,
                TechnicianId = technicianId,
                AssignedBy = userId,
                AssignedAt = DateTime.Now,
                IsActive = true,
                Notes = "Quick assignment from dashboard"
            };

            request.AssignedTechnicianId = technicianId;
            request.Status = ServiceRequestStatus.InProgress;
            request.UpdatedAt = DateTime.Now;

            _context.Assignments.Add(newAssignment);
            _context.ServiceRequests.Update(request);
            await _context.SaveChangesAsync();

            _auditService.Log(userId, "ASSIGN", "ServiceRequest", $"Assigned to technician {technician.FullName}");

            return Ok(new { 
                success = true,
                message = previousTechnicianName != null ? 
                    $"Request reassigned from {previousTechnicianName} to {technician.FullName}" : 
                    $"Request assigned to {technician.FullName}",
                technicianName = technician.FullName,
                previousTechnicianName = previousTechnicianName
            });
        }

        // GET: Assignments/Workload
        [HttpGet("Workload")]
        [Authorize(Roles = "Admin,SuperAdmin,Technician")]
        public IActionResult Workload()
        {
            var technicians = _context.Users
                .Where(u => u.IsActive && u.Role != null && u.Role.RoleName == "Technician")
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToList();

            var allServiceRequests = _context.ServiceRequests.ToList();

            var workloadData = technicians.Select(tech => new
            {
                Technician = tech.FullName,
                OpenRequests = allServiceRequests
                    .Count(sr => sr.AssignedTechnicianId == tech.UserId && sr.Status == ServiceRequestStatus.Pending),
                InProgressRequests = allServiceRequests
                    .Count(sr => sr.AssignedTechnicianId == tech.UserId && sr.Status == ServiceRequestStatus.InProgress),
                ResolvedRequests = allServiceRequests
                    .Count(sr => sr.AssignedTechnicianId == tech.UserId && sr.Status == ServiceRequestStatus.Resolved),
                TotalAssigned = allServiceRequests
                    .Count(sr => sr.AssignedTechnicianId == tech.UserId && 
                           (sr.Status == ServiceRequestStatus.Pending || sr.Status == ServiceRequestStatus.InProgress))
            }).ToList();

            return View(workloadData);
        }

        // GET: Assignments/History/5
        [HttpGet("History/{requestId}")]
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
                .AsNoTracking()
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

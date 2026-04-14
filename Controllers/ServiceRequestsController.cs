using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ITSMS.Data;
using ITSMS.Models;

namespace ITSMS.Controllers
{
    /// <summary>
    /// ServiceRequests Controller - Handles service request ticketing operations
    /// Authorization: Client (create), Technician (update/view), Admin (view all)
    /// </summary>
    [Authorize]
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string AdminRole = "Admin";
        private const string TechnicianRole = "Technician";
        private const string ClientRole = "Client";

        public ServiceRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ServiceRequests/Index (List all requests - filtered by role)
        [HttpGet]
        public IActionResult Index()
        {
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            IQueryable<ServiceRequest> requests = _context.ServiceRequests
                .Include(sr => sr.Category)
                .Include(sr => sr.Requestor)
                .Include(sr => sr.AssignedTechnician)
                .AsQueryable();

            // Filter based on role
            if (userRole == ClientRole)
            {
                // Clients see only their own requests
                requests = requests.Where(sr => sr.RequestorId == userId);
            }
            else if (userRole == TechnicianRole)
            {
                // Technicians see requests assigned to them + unassigned requests
                requests = requests.Where(sr => sr.AssignedTechnicianId == userId || sr.AssignedTechnicianId == null);
            }
            // Admin sees all requests

            var serviceRequests = requests.OrderByDescending(sr => sr.CreatedAt).ToList();
            return View(serviceRequests);
        }

        // GET: ServiceRequests/Details/5
        [HttpGet]
        public IActionResult Details(int id)
        {
            var request = _context.ServiceRequests
                .Include(sr => sr.Category)
                .Include(sr => sr.Requestor)
                .Include(sr => sr.AssignedTechnician)
                .Include(sr => sr.Assignments)
                .Include(sr => sr.Feedback)
                .FirstOrDefault(sr => sr.RequestId == id);

            if (request == null)
                return NotFound();

            // Authorization check
            if (!CanViewRequest(request))
                return Forbid();

            return View(request);
        }

        // GET: ServiceRequests/Create
        [HttpGet]
        [Authorize(Roles = "Client,Admin")]
        public IActionResult Create()
        {
            ViewData["Categories"] = _context.Categories.Where(c => c.IsActive).ToList();
            return View();
        }

        // POST: ServiceRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client,Admin")]
        public async Task<IActionResult> Create(ServiceRequest serviceRequest)
        {
            var userId = GetCurrentUserId();

            // Generate unique request number
            var lastRequest = _context.ServiceRequests.OrderByDescending(sr => sr.RequestId).FirstOrDefault();
            var nextNumber = (lastRequest?.RequestId ?? 0) + 1;
            serviceRequest.RequestNumber = $"REQ-{nextNumber:000000}";

            serviceRequest.RequestorId = userId;
            serviceRequest.Status = ServiceRequestStatus.Open;
            serviceRequest.CreatedAt = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                _context.ServiceRequests.Add(serviceRequest);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Service request {serviceRequest.RequestNumber} created successfully.";
                return RedirectToAction(nameof(Details), new { id = serviceRequest.RequestId });
            }

            ViewData["Categories"] = _context.Categories.Where(c => c.IsActive).ToList();
            return View(serviceRequest);
        }

        // GET: ServiceRequests/Edit/5
        [HttpGet]
        [Authorize(Roles = "Technician,Admin")]
        public IActionResult Edit(int id)
        {
            var request = _context.ServiceRequests.FirstOrDefault(sr => sr.RequestId == id);
            if (request == null)
                return NotFound();

            ViewData["Categories"] = _context.Categories.Where(c => c.IsActive).ToList();
            return View(request);
        }

        // POST: ServiceRequests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Technician,Admin")]
        public async Task<IActionResult> Edit(int id, ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.RequestId)
                return NotFound();

            var existingRequest = _context.ServiceRequests.FirstOrDefault(sr => sr.RequestId == id);
            if (existingRequest == null)
                return NotFound();

            try
            {
                existingRequest.Title = serviceRequest.Title;
                existingRequest.Description = serviceRequest.Description;
                existingRequest.Status = serviceRequest.Status;
                existingRequest.Priority = serviceRequest.Priority;
                existingRequest.UpdatedAt = DateTime.UtcNow;

                _context.ServiceRequests.Update(existingRequest);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Service request updated successfully.";
                return RedirectToAction(nameof(Details), new { id = existingRequest.RequestId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating request: {ex.Message}");
            }

            ViewData["Categories"] = _context.Categories.Where(c => c.IsActive).ToList();
            return View(serviceRequest);
        }

        // GET: ServiceRequests/Close/5
        [HttpGet]
        [Authorize(Roles = "Technician,Admin")]
        public IActionResult Close(int id)
        {
            var request = _context.ServiceRequests.FirstOrDefault(sr => sr.RequestId == id);
            if (request == null)
                return NotFound();

            return View(request);
        }

        // POST: ServiceRequests/Close/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Technician,Admin")]
        public async Task<IActionResult> Close(int id)
        {
            var request = _context.ServiceRequests.FirstOrDefault(sr => sr.RequestId == id);
            if (request == null)
                return NotFound();

            request.Status = ServiceRequestStatus.Closed;
            request.ClosedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;

            _context.ServiceRequests.Update(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Service request closed successfully.";
            return RedirectToAction(nameof(Details), new { id = request.RequestId });
        }

        // ==================== HELPER METHODS ====================

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim?.Value, out var userId) ? userId : 0;
        }

        private bool CanViewRequest(ServiceRequest request)
        {
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (userRole == AdminRole)
                return true;

            if (userRole == ClientRole && request.RequestorId == userId)
                return true;

            if (userRole == TechnicianRole && (request.AssignedTechnicianId == userId || request.AssignedTechnicianId == null))
                return true;

            return false;
        }
    }
}

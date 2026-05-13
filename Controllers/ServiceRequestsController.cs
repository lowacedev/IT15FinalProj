using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ITSMS.Data;
using ITSMS.Models;
using Microsoft.EntityFrameworkCore;

namespace ITSMS.Controllers
{
    /// <summary>
    /// ServiceRequests Controller - Handles service request ticketing operations
    /// Authorization: Client (create), Technician (update/view), Admin (view all)
    /// </summary>
    [Route("[controller]")]
    [Authorize]
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ITSMS.Services.NotificationService _notificationService;
        private readonly ITSMS.Services.AuditService _auditService;
        private const string AdminRole = "Admin";
        private const string SuperAdminRole = "SuperAdmin";
        private const string TechnicianRole = "Technician";
        private const string EmployeeRole = "Employee";

        public ServiceRequestsController(ApplicationDbContext context, ITSMS.Services.NotificationService notificationService, ITSMS.Services.AuditService auditService)
        {
            _context = context;
            _notificationService = notificationService;
            _auditService = auditService;
        }

        // GET: ServiceRequests/Index (List all requests - filtered by role)
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index(int page = 1, int pageSize = 10)
        {
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            IQueryable<ServiceRequest> requests = _context.ServiceRequests
                .Include(sr => sr.Category)
                .Include(sr => sr.Requestor)
                .Include(sr => sr.AssignedTechnician)
                .Include(sr => sr.Employee)
                    .ThenInclude(e => e.User)
                .AsQueryable();

            // Filter based on role
            if (userRole == EmployeeRole)
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

            // Get total count before pagination
            var totalCount = requests.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Ensure page is valid
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            // Apply pagination
            var serviceRequests = requests.OrderByDescending(sr => sr.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Pass pagination info via ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;

            return View(serviceRequests);
        }

        // GET: ServiceRequests/Details/5
        [HttpGet("Details/{id}")]
        public IActionResult Details(int id)
        {
            var request = _context.ServiceRequests
                .Include(sr => sr.Category)
                .Include(sr => sr.Requestor)
                .Include(sr => sr.AssignedTechnician)
                .Include(sr => sr.Employee)
                    .ThenInclude(e => e.User)
                .Include(sr => sr.Assignments)
                .Include(sr => sr.Feedback)
                .Include(sr => sr.Comments.OrderByDescending(c => c.CreatedAt))
                    .ThenInclude(c => c.Author)
                .FirstOrDefault(sr => sr.RequestId == id);

            if (request == null)
                return NotFound();

            // Authorization check
            if (!CanViewRequest(request))
                return Forbid();

            return View(request);
        }

        // GET: ServiceRequests/Create
        [HttpGet("Create")]
        [Authorize(Roles = "Employee,Admin,SuperAdmin")]
        public IActionResult Create()
        {
            var userId = GetCurrentUserId();
            var viewModel = new ServiceRequestCreateViewModel();

            // Get categories
            ViewData["Categories"] = _context.Categories.Where(c => c.IsActive).ToList();

            // Get current logged-in employee
            var employee = _context.Employees.FirstOrDefault(e => e.UserId == userId);
            if (employee != null)
            {
                // Get assets assigned to this employee (only active assignments - no return date)
                var employeeAssets = _context.AssetAssignments
                    .Where(a => a.EmployeeId == employee.Id && a.ReturnedDate == null)
                    .Include(a => a.Asset)
                    .Select(a => new SelectListItem
                    {
                        Value = a.AssetId.ToString(),
                        Text = $"{a.Asset.AssetTag} - {a.Asset.AssetName}"
                    })
                    .ToList();

                viewModel.EmployeeAssets = employeeAssets;

                // Smart UX: Auto-select if only one asset
                if (employeeAssets.Count == 1)
                {
                    viewModel.AssetId = int.Parse(employeeAssets[0].Value);
                }
            }

            return View(viewModel);
        }

        // POST: ServiceRequests/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Employee,Admin")]
        public async Task<IActionResult> Create(ServiceRequestCreateViewModel model)
        {
            var userId = GetCurrentUserId();

            // Map ViewModel to ServiceRequest
            var serviceRequest = new ServiceRequest
            {
                Title = model.Title,
                Description = model.Description,
                CategoryId = model.CategoryId,
                Priority = model.Priority,
                AssetId = model.AssetId // Optional asset from dropdown
            };

            // Generate unique request number
            var lastRequest = _context.ServiceRequests.OrderByDescending(sr => sr.RequestId).FirstOrDefault();
            var nextNumber = (lastRequest?.RequestId ?? 0) + 1;
            serviceRequest.RequestNumber = $"REQ-{nextNumber:000000}";

            serviceRequest.RequestorId = userId;
            serviceRequest.Status = ServiceRequestStatus.Pending;
            serviceRequest.CreatedAt = DateTime.Now;

            // Auto-link EmployeeId from User's Employee record
            var employee = _context.Employees.FirstOrDefault(e => e.UserId == userId);
            if (employee != null)
            {
                serviceRequest.EmployeeId = employee.Id;
            }

            if (ModelState.IsValid)
            {
                _context.ServiceRequests.Add(serviceRequest);
                await _context.SaveChangesAsync();
                
                _auditService.Log(userId, "CREATE", "ServiceRequest", $"Created request {serviceRequest.RequestNumber}");
                
                TempData["Success"] = $"Service request {serviceRequest.RequestNumber} created successfully.";
                return RedirectToAction(nameof(Details), new { id = serviceRequest.RequestId });
            }

            // Repopulate form with employee assets on validation error
            var viewModel = new ServiceRequestCreateViewModel
            {
                Title = model.Title,
                Description = model.Description,
                CategoryId = model.CategoryId,
                Priority = model.Priority,
                AssetId = model.AssetId
            };

            if (employee != null)
            {
                var employeeAssets = _context.AssetAssignments
                    .Where(a => a.EmployeeId == employee.Id && a.ReturnedDate == null)
                    .Include(a => a.Asset)
                    .Select(a => new SelectListItem
                    {
                        Value = a.AssetId.ToString(),
                        Text = $"{a.Asset.AssetTag} - {a.Asset.AssetName}"
                    })
                    .ToList();
                viewModel.EmployeeAssets = employeeAssets;
            }

            ViewData["Categories"] = _context.Categories.Where(c => c.IsActive).ToList();
            return View(viewModel);
        }

        // GET: ServiceRequests/Edit/5
        [HttpGet("Edit/{id}")]
        [Authorize(Roles = "Technician,Admin,SuperAdmin")]
        public IActionResult Edit(int id)
        {
            try
            {
                var request = _context.ServiceRequests
                    .Include(sr => sr.AssignedTechnician)
                    .FirstOrDefault(sr => sr.RequestId == id);
                
                if (request == null)
                {
                    return NotFound($"Service request with ID {id} not found");
                }

                // Authorization check - ensure technician can only edit their assigned requests
                var userId = GetCurrentUserId();
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                
                if (userRole == TechnicianRole && request.AssignedTechnicianId != userId)
                    return Forbid();

                ViewData["Categories"] = _context.Categories.Where(c => c.IsActive).ToList();
                return View(request);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        // POST: ServiceRequests/Edit/5
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Technician,Admin,SuperAdmin")]
        public async Task<IActionResult> Edit(int id, ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.RequestId)
                return NotFound();

            var existingRequest = _context.ServiceRequests
                .Include(sr => sr.AssignedTechnician)
                .FirstOrDefault(sr => sr.RequestId == id);
            if (existingRequest == null)
                return NotFound();

            // Authorization check - ensure technician can only edit their assigned requests
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            
            if (userRole == TechnicianRole && existingRequest.AssignedTechnicianId != userId)
                return Forbid();

            try
            {
                var previousStatus = existingRequest.Status;
                
                // Admins and SuperAdmins can update all fields; Technicians can only update Status and Priority
                if (userRole == AdminRole || userRole == SuperAdminRole)
                {
                    existingRequest.Title = serviceRequest.Title;
                    existingRequest.Description = serviceRequest.Description;
                }

                existingRequest.Status = serviceRequest.Status;
                existingRequest.Priority = serviceRequest.Priority;
                existingRequest.UpdatedAt = DateTime.Now;

                // Set ResolvedAt timestamp when status changes to Resolved
                if (serviceRequest.Status == ServiceRequestStatus.Resolved && previousStatus != ServiceRequestStatus.Resolved)
                {
                    existingRequest.ResolvedAt = DateTime.Now;
                }

                _context.ServiceRequests.Update(existingRequest);
                await _context.SaveChangesAsync();

                if (previousStatus != existingRequest.Status)
                {
                    _auditService.Log(userId, "UPDATE", "ServiceRequest", $"Changed status to {existingRequest.Status}");

                    await _notificationService.SendStatusNotification(
                        existingRequest.RequestorId.ToString(),
                        existingRequest.RequestNumber ?? existingRequest.RequestId.ToString(),
                        existingRequest.Status.ToString(),
                        $"Status updated to {existingRequest.Status}."
                    );
                }

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

        // GET: ServiceRequests/CloseConfirm/5
        [HttpGet("CloseConfirm/{id}")]
        [Authorize(Roles = "Technician,Admin,SuperAdmin")]
        public IActionResult CloseConfirm(int id)
        {
            var request = _context.ServiceRequests.FirstOrDefault(sr => sr.RequestId == id);
            if (request == null)
                return NotFound();

            // Authorization check - ensure technician can only close their assigned requests
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            
            if (userRole == TechnicianRole && request.AssignedTechnicianId != userId)
                return Forbid();

            return View("Close", request);
        }

        // POST: ServiceRequests/Close/5
        [HttpPost("Close/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Technician,Admin,SuperAdmin")]
        public async Task<IActionResult> Close(int id)
        {
            var request = _context.ServiceRequests.FirstOrDefault(sr => sr.RequestId == id);
            if (request == null)
                return NotFound();

            // Authorization check - ensure technician can only close their assigned requests
            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            
            if (userRole == TechnicianRole && request.AssignedTechnicianId != userId)
                return Forbid();

            request.Status = ServiceRequestStatus.Closed;
            request.ClosedAt = DateTime.Now;
            request.UpdatedAt = DateTime.Now;

            _context.ServiceRequests.Update(request);
            await _context.SaveChangesAsync();

            await _notificationService.SendStatusNotification(
                request.RequestorId.ToString(),
                request.RequestNumber ?? request.RequestId.ToString(),
                request.Status.ToString(),
                $"Service request was closed."
            );

            TempData["Success"] = "Service request closed successfully.";
            return RedirectToAction(nameof(Details), new { id = request.RequestId });
        }

        // POST: ServiceRequests/LogCost
        [HttpPost("LogCost")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Technician,Admin,SuperAdmin")]
        public async Task<IActionResult> LogCost(int requestId, decimal amount, FinanceTransactionType transactionType, string description)
        {
            var request = _context.ServiceRequests
                .Include(sr => sr.Employee)
                .FirstOrDefault(sr => sr.RequestId == requestId);
            if (request == null)
                return NotFound();

            var userId = GetCurrentUserId();

            var transaction = new FinanceTransaction
            {
                ServiceRequestId = requestId,
                Amount = amount,
                TransactionType = transactionType,
                Description = description,
                TransactionDate = DateTime.Now,
                CreatedByUserId = userId,
                DepartmentId = request.Employee?.DepartmentId // Attempt to auto-tag department if available
            };

            _context.FinanceTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cost logged successfully to Finance.";
            return RedirectToAction(nameof(Details), new { id = requestId });
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

            if (userRole == AdminRole || userRole == SuperAdminRole)
                return true;

            if (userRole == EmployeeRole && request.RequestorId == userId)
                return true;

            if (userRole == TechnicianRole && (request.AssignedTechnicianId == userId || request.AssignedTechnicianId == null))
                return true;

            return false;
        }
    }
}

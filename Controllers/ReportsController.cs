using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ITSMS.Data;
using ITSMS.Models;
using Microsoft.EntityFrameworkCore;

namespace ITSMS.Controllers
{
    /// <summary>
    /// Reports Controller - System dashboards and analytics
    /// Authorization: Admin/Technician for reports
    /// </summary>
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reports/Dashboard
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public IActionResult Dashboard()
        {
            // Materialize all data first to avoid connection reuse issues
            var recentRequests = _context.ServiceRequests
                .Include(sr => sr.Category)
                .Include(sr => sr.Requestor)
                .Include(sr => sr.AssignedTechnician)
                .OrderByDescending(sr => sr.CreatedAt)
                .Take(5)
                .ToList();

            var allServiceRequests = _context.ServiceRequests.ToList();
            var allUsers = _context.Users.Include(u => u.Role).ToList();

            // Calculate statistics from materialized data
            var technicians = allUsers.Where(u => u.IsActive && u.Role?.RoleName == "Technician").ToList();
            
            var topTechnicians = technicians
                .Select(t => new
                {
                    Technician = t.FullName,
                    AssignedCount = allServiceRequests.Count(sr => sr.AssignedTechnicianId == t.UserId),
                    CompletedCount = allServiceRequests.Count(sr => sr.AssignedTechnicianId == t.UserId && sr.Status == ServiceRequestStatus.Resolved)
                })
                .OrderByDescending(x => x.AssignedCount)
                .Take(5)
                .ToList();

            var dashboard = new
            {
                TotalRequests = allServiceRequests.Count,
                PendingRequests = allServiceRequests.Count(sr => sr.Status == ServiceRequestStatus.Pending),
                InProgressRequests = allServiceRequests.Count(sr => sr.Status == ServiceRequestStatus.InProgress),
                ResolvedRequests = allServiceRequests.Count(sr => sr.Status == ServiceRequestStatus.Resolved),
                ClosedRequests = allServiceRequests.Count(sr => sr.Status == ServiceRequestStatus.Closed),

                CriticalRequests = allServiceRequests.Count(sr => sr.Priority == ServiceRequestPriority.Critical && sr.Status != ServiceRequestStatus.Closed),
                AverageResolutionTime = CalculateAverageResolutionTime(allServiceRequests),
                
                RequestsByCategory = allServiceRequests
                    .GroupBy(sr => sr.Category != null ? sr.Category.CategoryName : "Uncategorized")
                    .Select(g => new { Category = g.Key, Count = g.Count() })
                    .ToDictionary(x => x.Category, x => x.Count),

                RequestsByPriority = allServiceRequests
                    .Where(sr => sr.Status != ServiceRequestStatus.Closed)
                    .GroupBy(sr => sr.Priority)
                    .Select(g => new { Priority = g.Key.ToString(), Count = g.Count() })
                    .ToDictionary(x => x.Priority, x => x.Count),

                TotalUsers = allUsers.Count(u => u.IsActive),
                TotalTechnicians = technicians.Count,
                TotalClients = allUsers.Count(u => u.IsActive && u.Role?.RoleName == "Employee"),
                
                RecentRequests = recentRequests,
                TopTechnicians = topTechnicians
            };

            ViewData["Dashboard"] = dashboard;
            return View();
        }

        // GET: Reports/TechnicianWorkload
        [HttpGet]
        [Authorize(Roles = "Admin,Technician,SuperAdmin")]
        public IActionResult TechnicianWorkload()
        {
            var technicians = _context.Users
                .Where(u => u.IsActive && u.Role.RoleName == "Technician")
                .ToList();

            var allServiceRequests = _context.ServiceRequests.ToList();

            var workload = technicians.Select(t => new
            {
                Technician = t.FullName,
                PendingRequests = allServiceRequests.Count(sr => sr.AssignedTechnicianId == t.UserId && sr.Status == ServiceRequestStatus.Pending),
                InProgressRequests = allServiceRequests.Count(sr => sr.AssignedTechnicianId == t.UserId && sr.Status == ServiceRequestStatus.InProgress),
                ResolvedRequests = allServiceRequests.Count(sr => sr.AssignedTechnicianId == t.UserId && sr.Status == ServiceRequestStatus.Resolved),
                TotalAssigned = allServiceRequests.Count(sr => sr.AssignedTechnicianId == t.UserId && (sr.Status == ServiceRequestStatus.Pending || sr.Status == ServiceRequestStatus.InProgress || sr.Status == ServiceRequestStatus.Resolved))
            }).ToList();

            return View(workload);
        }

        // GET: Reports/CategoryAnalysis
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public IActionResult CategoryAnalysis()
        {
            var analysis = _context.ServiceRequests
                .GroupBy(sr => sr.Category != null ? sr.Category.CategoryName : "Uncategorized")
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Count(),
                    Pending = g.Count(sr => sr.Status == ServiceRequestStatus.Pending),
                    InProgress = g.Count(sr => sr.Status == ServiceRequestStatus.InProgress),
                    Resolved = g.Count(sr => sr.Status == ServiceRequestStatus.Resolved),
                    Closed = g.Count(sr => sr.Status == ServiceRequestStatus.Closed),
                    AvgResolutionTime = g.Where(sr => sr.ResolvedAt.HasValue)
                        .Average(sr => sr.ResolvedAt.HasValue ? (sr.ResolvedAt.Value - sr.CreatedAt).TotalHours : 0)
                })
                .ToList();

            return View(analysis);
        }

        // GET: Reports/PriorityAnalysis
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public IActionResult PriorityAnalysis()
        {
            var analysis = _context.ServiceRequests
                .GroupBy(sr => sr.Priority)
                .Select(g => new
                {
                    Priority = g.Key.ToString(),
                    Total = g.Count(),
                    Pending = g.Count(sr => sr.Status == ServiceRequestStatus.Pending),
                    InProgress = g.Count(sr => sr.Status == ServiceRequestStatus.InProgress),
                    Resolved = g.Count(sr => sr.Status == ServiceRequestStatus.Resolved),
                    AvgResolutionTime = g.Where(sr => sr.ResolvedAt.HasValue)
                        .Average(sr => sr.ResolvedAt.HasValue ? (sr.ResolvedAt.Value - sr.CreatedAt).TotalHours : 0)
                })
                .OrderByDescending(x => x.Priority)
                .ToList();

            return View(analysis);
        }

        // GET: Reports/CustomerSatisfaction
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public IActionResult CustomerSatisfaction()
        {
            var feedbacks = _context.Feedbacks
                .Include(f => f.Request)
                .Include(f => f.Request.Category)
                .ToList();

            var satisfaction = new
            {
                TotalFeedback = feedbacks.Count,
                AverageRating = feedbacks.Count > 0 ? Math.Round(feedbacks.Average(f => f.Rating), 2) : 0,
                RatingDistribution = feedbacks.GroupBy(f => f.Rating)
                    .Select(g => new { Stars = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Stars)
                    .ToList(),
                ByCategoryAverage = feedbacks
                    .Where(f => f.Request != null && f.Request.Category != null)
                    .GroupBy(f => f.Request.Category.CategoryName)
                    .Select(g => new { Category = g.Key, AvgRating = Math.Round(g.Average(f => f.Rating), 2), Count = g.Count() })
                    .OrderByDescending(x => x.AvgRating)
                    .ToList()
            };

            ViewData["Satisfaction"] = satisfaction;
            return View(feedbacks);
        }



        // GET: Reports/GetCriticalRequests
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public IActionResult GetCriticalRequests()
        {
            var criticalRequests = _context.ServiceRequests
                .Include(sr => sr.Category)
                .Include(sr => sr.Requestor)
                .Include(sr => sr.AssignedTechnician)
                .Where(sr => sr.Priority == ServiceRequestPriority.Critical && sr.Status != ServiceRequestStatus.Closed)
                .OrderByDescending(sr => sr.CreatedAt)
                .ToList();

            var technicians = _context.Users
                .Where(u => u.IsActive && u.Role.RoleName == "Technician")
                .ToList();

            ViewBag.Technicians = technicians;
            return PartialView("_CriticalRequests", criticalRequests);
        }

        // GET: Reports/GetPendingRequests
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public IActionResult GetPendingRequests()
        {
            var pendingRequests = _context.ServiceRequests
                .Include(sr => sr.Category)
                .Include(sr => sr.Requestor)
                .Include(sr => sr.AssignedTechnician)
                .Where(sr => sr.Status == ServiceRequestStatus.Pending)
                .OrderByDescending(sr => sr.CreatedAt)
                .ToList();

            var technicians = _context.Users
                .Where(u => u.IsActive && u.Role.RoleName == "Technician")
                .ToList();

            ViewBag.Technicians = technicians;
            return PartialView("_PendingRequests", pendingRequests);
        }

        // GET: API/Reports/GetDashboardData (JSON endpoint for auto-refresh)
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public IActionResult GetDashboardData()
        {
            var allServiceRequests = _context.ServiceRequests.ToList();
            var allUsers = _context.Users.Include(u => u.Role).ToList();

            var technicians = allUsers.Where(u => u.IsActive && u.Role?.RoleName == "Technician").ToList();

            var recentRequests = _context.ServiceRequests
                .Include(sr => sr.Category)
                .Include(sr => sr.Requestor)
                .Include(sr => sr.AssignedTechnician)
                .OrderByDescending(sr => sr.CreatedAt)
                .Take(5)
                .ToList();

            var topTechnicians = technicians
                .Select(t => new
                {
                    Technician = t.FullName,
                    AssignedCount = allServiceRequests.Count(sr => sr.AssignedTechnicianId == t.UserId),
                    CompletedCount = allServiceRequests.Count(sr => sr.AssignedTechnicianId == t.UserId && sr.Status == ServiceRequestStatus.Resolved)
                })
                .OrderByDescending(x => x.AssignedCount)
                .Take(5)
                .ToList();

            var dashboardData = new
            {
                TotalRequests = allServiceRequests.Count,
                PendingRequests = allServiceRequests.Count(sr => sr.Status == ServiceRequestStatus.Pending),
                InProgressRequests = allServiceRequests.Count(sr => sr.Status == ServiceRequestStatus.InProgress),
                ResolvedRequests = allServiceRequests.Count(sr => sr.Status == ServiceRequestStatus.Resolved),
                ClosedRequests = allServiceRequests.Count(sr => sr.Status == ServiceRequestStatus.Closed),
                CriticalRequests = allServiceRequests.Count(sr => sr.Priority == ServiceRequestPriority.Critical && sr.Status != ServiceRequestStatus.Closed),
                AverageResolutionTime = CalculateAverageResolutionTime(allServiceRequests),
                TotalTechnicians = technicians.Count,
                TopTechnicians = topTechnicians,
                RecentRequests = recentRequests.Select(r => new
                {
                    r.RequestId,
                    r.RequestNumber,
                    r.Title,
                    r.Priority,
                    r.Status,
                    AssignedTechnicianName = r.AssignedTechnician?.FullName ?? "Unassigned",
                    RequestorName = r.Requestor?.FullName,
                    r.CreatedAt
                }).ToList()
            };

            return Json(dashboardData);
        }

        // GET: Reports/Analytics
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public IActionResult Analytics()
        {
            var viewModel = new ReportsViewModel();

            // Materialize all data first
            var allServiceRequests = _context.ServiceRequests
                .Include(sr => sr.Category)
                .Include(sr => sr.Requestor)
                .Include(sr => sr.AssignedTechnician)
                .Include(sr => sr.Asset)
                .Include(sr => sr.Employee)
                    .ThenInclude(e => e.Department)
                .ToList();

            // ===== REQUESTS BY PRIORITY (FOR BAR CHART) =====
            var priorityColors = new Dictionary<string, string>
            {
                { "Low", "#0ea5e9" },
                { "Medium", "#fbbf24" },
                { "High", "#f87171" },
                { "Critical", "#dc2626" }
            };

            viewModel.RequestsByPriority = allServiceRequests
                .GroupBy(sr => sr.Priority)
                .Select(g => new PriorityData
                {
                    Priority = g.Key.ToString(),
                    Count = g.Count(),
                    Color = priorityColors.ContainsKey(g.Key.ToString()) ? priorityColors[g.Key.ToString()] : "#6b7280"
                })
                .OrderBy(x => x.Priority)
                .ToList();

            // ===== REQUESTS BY CATEGORY (FOR PIE CHART) =====
            var categoryColors = new[] { "#667eea", "#764ba2", "#f093fb", "#4facfe", "#00f2fe", "#43e97b", "#fa709a", "#fee140" };
            
            var categoryList = allServiceRequests
                .GroupBy(sr => sr.Category != null ? sr.Category.CategoryName : "Uncategorized")
                .Select(g => new CategoryData
                {
                    Category = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            // Assign colors to categories
            for (int i = 0; i < categoryList.Count; i++)
            {
                categoryList[i].Color = categoryColors[i % categoryColors.Length];
            }

            viewModel.RequestsByCategory = categoryList;

            // ===== SUMMARY STATISTICS =====
            viewModel.TotalRequests = allServiceRequests.Count;
            viewModel.PendingRequests = allServiceRequests.Count(sr => sr.Status == ServiceRequestStatus.Pending);
            viewModel.InProgressRequests = allServiceRequests.Count(sr => sr.Status == ServiceRequestStatus.InProgress);
            viewModel.ResolvedRequests = allServiceRequests.Count(sr => sr.Status == ServiceRequestStatus.Resolved);
            viewModel.ClosedRequests = allServiceRequests.Count(sr => sr.Status == ServiceRequestStatus.Closed);
            viewModel.CriticalRequests = allServiceRequests.Count(sr => sr.Priority == ServiceRequestPriority.Critical && sr.Status != ServiceRequestStatus.Closed);
            viewModel.AverageResolutionTime = CalculateAverageResolutionTime(allServiceRequests);

            // ===== ERP DATA =====
            viewModel.RequestsPerAsset = allServiceRequests
                .Where(sr => sr.AssetId.HasValue && sr.Asset != null)
                .GroupBy(sr => sr.Asset)
                .Select(g => new RequestsPerAssetData
                {
                    AssetId = g.Key.Id,
                    AssetTag = g.Key.AssetTag,
                    AssetName = g.Key.AssetName,
                    RequestCount = g.Count()
                })
                .OrderByDescending(x => x.RequestCount)
                .Take(10)
                .ToList();

            viewModel.RequestsPerDepartment = allServiceRequests
                .Where(sr => sr.EmployeeId.HasValue && sr.Employee != null && sr.Employee.Department != null)
                .GroupBy(sr => sr.Employee.Department)
                .Select(g => new RequestsPerDepartmentData
                {
                    DepartmentId = g.Key.Id,
                    DepartmentName = g.Key.Name,
                    RequestCount = g.Count()
                })
                .OrderByDescending(x => x.RequestCount)
                .ToList();

            var assetColors = new Dictionary<AssetStatus, string>
            {
                { AssetStatus.Working, "#43e97b" },
                { AssetStatus.Defective, "#fa709a" },
                { AssetStatus.UnderRepair, "#fbbf24" },
                { AssetStatus.Retired, "#6b7280" }
            };

            var allAssets = _context.Assets.ToList();
            viewModel.AssetStatusSummary = allAssets
                .GroupBy(a => a.Status)
                .Select(g => new AssetStatusSummaryData
                {
                    Status = g.Key.ToString(),
                    Count = g.Count(),
                    Color = assetColors.ContainsKey(g.Key) ? assetColors[g.Key] : "#6b7280"
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            return View(viewModel);
        }

        // GET: Reports/TechnicianPerformance
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public IActionResult TechnicianPerformance()
        {
            var viewModel = new ReportsViewModel();

            // Get all active technicians
            var technicians = _context.Users
                .Where(u => u.IsActive && u.Role.RoleName == "Technician")
                .ToList();

            var allServiceRequests = _context.ServiceRequests.ToList();

            // Calculate performance metrics for each technician
            viewModel.TechnicianPerformances = technicians.Select(t => new TechnicianPerformance
            {
                TechnicianId = t.UserId,
                TechnicianName = t.FullName,
                AssignedTickets = allServiceRequests.Count(sr => sr.AssignedTechnicianId == t.UserId),
                CompletedTickets = allServiceRequests.Count(sr => sr.AssignedTechnicianId == t.UserId && (sr.Status == ServiceRequestStatus.Resolved || sr.Status == ServiceRequestStatus.Closed)),
                InProgressTickets = allServiceRequests.Count(sr => sr.AssignedTechnicianId == t.UserId && sr.Status == ServiceRequestStatus.InProgress),
                PendingTickets = allServiceRequests.Count(sr => sr.AssignedTechnicianId == t.UserId && sr.Status == ServiceRequestStatus.Pending),
                LastActive = allServiceRequests
                    .Where(sr => sr.AssignedTechnicianId == t.UserId)
                    .OrderByDescending(sr => sr.UpdatedAt)
                    .Select(sr => sr.UpdatedAt)
                    .FirstOrDefault(),
                IsActive = t.IsActive
            })
            .OrderByDescending(x => x.AssignedTickets)
            .ToList();

            // Add summary statistics
            var allServiceRequestsList = _context.ServiceRequests.ToList();
            viewModel.TotalRequests = allServiceRequestsList.Count;
            viewModel.PendingRequests = allServiceRequestsList.Count(sr => sr.Status == ServiceRequestStatus.Pending);
            viewModel.InProgressRequests = allServiceRequestsList.Count(sr => sr.Status == ServiceRequestStatus.InProgress);
            viewModel.ResolvedRequests = allServiceRequestsList.Count(sr => sr.Status == ServiceRequestStatus.Resolved || sr.Status == ServiceRequestStatus.Closed);
            viewModel.ClosedRequests = allServiceRequestsList.Count(sr => sr.Status == ServiceRequestStatus.Closed);

            return View(viewModel);
        }

        // GET: Reports/ServiceRequestsDetails
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public IActionResult ServiceRequestsDetails(string status = "", string priority = "", string search = "")
        {
            var viewModel = new ReportsViewModel();

            // Get all service requests with related data
            var allRequests = _context.ServiceRequests
                .Include(sr => sr.Category)
                .Include(sr => sr.Requestor)
                .Include(sr => sr.AssignedTechnician)
                .ToList();

            // ===== APPLY FILTERS =====
            var filteredRequests = allRequests;

            // Filter by Status
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<ServiceRequestStatus>(status, out var statusEnum))
                {
                    filteredRequests = filteredRequests.Where(sr => sr.Status == statusEnum).ToList();
                }
                viewModel.SelectedStatus = status;
            }

            // Filter by Priority
            if (!string.IsNullOrEmpty(priority))
            {
                if (Enum.TryParse<ServiceRequestPriority>(priority, out var priorityEnum))
                {
                    filteredRequests = filteredRequests.Where(sr => sr.Priority == priorityEnum).ToList();
                }
                viewModel.SelectedPriority = priority;
            }

            // Search by Title or Requestor
            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                filteredRequests = filteredRequests
                    .Where(sr => sr.Title.ToLower().Contains(searchLower) || 
                                 (sr.Requestor != null && sr.Requestor.FullName.ToLower().Contains(searchLower)))
                    .ToList();
                viewModel.SearchQuery = search;
            }

            // ===== MAP TO DETAIL VIEW MODEL =====
            viewModel.ServiceRequestDetails = filteredRequests
                .Select(sr => new ServiceRequestDetail
                {
                    RequestId = sr.RequestId,
                    RequestNumber = sr.RequestNumber,
                    Title = sr.Title,
                    Description = sr.Description,
                    Category = sr.Category?.CategoryName ?? "Uncategorized",
                    Requestor = sr.Requestor?.FullName ?? "N/A",
                    RequestorEmail = sr.Requestor?.Email ?? "N/A",
                    AssignedTechnician = sr.AssignedTechnician?.FullName ?? "Unassigned",
                    Status = sr.Status.ToString(),
                    Priority = sr.Priority.ToString(),
                    CreatedAt = sr.CreatedAt,
                    UpdatedAt = sr.UpdatedAt,
                    ResolvedAt = sr.ResolvedAt,
                    ClosedAt = sr.ClosedAt
                })
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            // ===== SUMMARY STATISTICS =====
            viewModel.TotalRequests = allRequests.Count;
            viewModel.PendingRequests = allRequests.Count(sr => sr.Status == ServiceRequestStatus.Pending);
            viewModel.InProgressRequests = allRequests.Count(sr => sr.Status == ServiceRequestStatus.InProgress);
            viewModel.ResolvedRequests = allRequests.Count(sr => sr.Status == ServiceRequestStatus.Resolved);
            viewModel.ClosedRequests = allRequests.Count(sr => sr.Status == ServiceRequestStatus.Closed);
            viewModel.CriticalRequests = allRequests.Count(sr => sr.Priority == ServiceRequestPriority.Critical && sr.Status != ServiceRequestStatus.Closed);

            return View(viewModel);
        }

        // ==================== HELPER METHODS ====================

        private double CalculateAverageResolutionTime(List<ServiceRequest> resolvedRequests = null)
        {
            var requests = resolvedRequests?.Where(sr => sr.ResolvedAt.HasValue).ToList() 
                ?? _context.ServiceRequests
                    .Where(sr => sr.ResolvedAt.HasValue)
                    .ToList();

            if (requests.Count == 0)
                return 0;

            var totalHours = requests.Sum(sr => (sr.ResolvedAt.Value - sr.CreatedAt).TotalHours);
            return Math.Round(totalHours / requests.Count, 2);
        }
    }
}

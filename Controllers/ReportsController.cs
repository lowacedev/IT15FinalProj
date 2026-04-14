using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ITSMS.Data;
using ITSMS.Models;

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
        [Authorize(Roles = "Admin")]
        public IActionResult Dashboard()
        {
            var dashboard = new
            {
                TotalRequests = _context.ServiceRequests.Count(),
                OpenRequests = _context.ServiceRequests.Count(sr => sr.Status == ServiceRequestStatus.Open),
                InProgressRequests = _context.ServiceRequests.Count(sr => sr.Status == ServiceRequestStatus.InProgress),
                ResolvedRequests = _context.ServiceRequests.Count(sr => sr.Status == ServiceRequestStatus.Resolved),
                ClosedRequests = _context.ServiceRequests.Count(sr => sr.Status == ServiceRequestStatus.Closed),

                CriticalRequests = _context.ServiceRequests.Count(sr => sr.Priority == ServiceRequestPriority.Critical && sr.Status != ServiceRequestStatus.Closed),
                AverageResolutionTime = CalculateAverageResolutionTime(),
                
                RequestsByCategory = _context.ServiceRequests
                    .GroupBy(sr => sr.Category.CategoryName)
                    .Select(g => new { Category = g.Key, Count = g.Count() })
                    .ToDictionary(x => x.Category, x => x.Count),

                RequestsByPriority = _context.ServiceRequests
                    .Where(sr => sr.Status != ServiceRequestStatus.Closed)
                    .GroupBy(sr => sr.Priority)
                    .Select(g => new { Priority = g.Key.ToString(), Count = g.Count() })
                    .ToDictionary(x => x.Priority, x => x.Count),

                TotalUsers = _context.Users.Count(u => u.IsActive),
                TotalTechnicians = _context.Users.Count(u => u.IsActive && u.Role.RoleName == "Technician"),
                TotalClients = _context.Users.Count(u => u.IsActive && u.Role.RoleName == "Client")
            };

            return View(dashboard);
        }

        // GET: Reports/TechnicianWorkload
        [HttpGet]
        [Authorize(Roles = "Admin,Technician")]
        public IActionResult TechnicianWorkload()
        {
            var technicians = _context.Users
                .Where(u => u.IsActive && u.Role.RoleName == "Technician")
                .ToList();

            var workload = technicians.Select(t => new
            {
                Technician = t.FullName,
                OpenRequests = _context.ServiceRequests.Count(sr => sr.AssignedTechnicianId == t.UserId && sr.Status == ServiceRequestStatus.Open),
                InProgressRequests = _context.ServiceRequests.Count(sr => sr.AssignedTechnicianId == t.UserId && sr.Status == ServiceRequestStatus.InProgress),
                ResolvedRequests = _context.ServiceRequests.Count(sr => sr.AssignedTechnicianId == t.UserId && sr.Status == ServiceRequestStatus.Resolved),
                TotalAssigned = _context.ServiceRequests.Count(sr => sr.AssignedTechnicianId == t.UserId && sr.Status != ServiceRequestStatus.Closed)
            }).ToList();

            return View(workload);
        }

        // GET: Reports/CategoryAnalysis
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult CategoryAnalysis()
        {
            var analysis = _context.ServiceRequests
                .GroupBy(sr => sr.Category.CategoryName)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Count(),
                    Open = g.Count(sr => sr.Status == ServiceRequestStatus.Open),
                    InProgress = g.Count(sr => sr.Status == ServiceRequestStatus.InProgress),
                    Resolved = g.Count(sr => sr.Status == ServiceRequestStatus.Resolved),
                    Closed = g.Count(sr => sr.Status == ServiceRequestStatus.Closed),
                    AvgResolutionTime = g.Where(sr => sr.ResolvedAt.HasValue)
                        .Average(sr => (sr.ResolvedAt.Value - sr.CreatedAt).TotalHours)
                })
                .ToList();

            return View(analysis);
        }

        // GET: Reports/PriorityAnalysis
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult PriorityAnalysis()
        {
            var analysis = _context.ServiceRequests
                .GroupBy(sr => sr.Priority)
                .Select(g => new
                {
                    Priority = g.Key.ToString(),
                    Total = g.Count(),
                    Open = g.Count(sr => sr.Status == ServiceRequestStatus.Open),
                    InProgress = g.Count(sr => sr.Status == ServiceRequestStatus.InProgress),
                    Resolved = g.Count(sr => sr.Status == ServiceRequestStatus.Resolved),
                    AvgResolutionTime = g.Where(sr => sr.ResolvedAt.HasValue)
                        .Average(sr => (sr.ResolvedAt.Value - sr.CreatedAt).TotalHours)
                })
                .OrderByDescending(x => x.Priority)
                .ToList();

            return View(analysis);
        }

        // GET: Reports/CustomerSatisfaction
        [HttpGet]
        [Authorize(Roles = "Admin")]
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
                ByCategoryAverage = feedbacks.GroupBy(f => f.Request.Category.CategoryName)
                    .Select(g => new { Category = g.Key, AvgRating = Math.Round(g.Average(f => f.Rating), 2), Count = g.Count() })
                    .OrderByDescending(x => x.AvgRating)
                    .ToList()
            };

            ViewData["Satisfaction"] = satisfaction;
            return View(feedbacks);
        }

        // GET: Reports/ResponseTimeAnalysis
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult ResponseTimeAnalysis()
        {
            var requests = _context.ServiceRequests
                .Where(sr => sr.ResolvedAt.HasValue)
                .ToList();

            var analysis = requests.Select(sr => new
            {
                RequestNumber = sr.RequestNumber,
                Title = sr.Title,
                Category = sr.Category.CategoryName,
                Priority = sr.Priority.ToString(),
                ResolutionTimeHours = (sr.ResolvedAt.Value - sr.CreatedAt).TotalHours,
                CreatedAt = sr.CreatedAt,
                ResolvedAt = sr.ResolvedAt
            }).OrderByDescending(x => x.ResolutionTimeHours).ToList();

            return View(analysis);
        }

        // ==================== HELPER METHODS ====================

        private double CalculateAverageResolutionTime()
        {
            var resolvedRequests = _context.ServiceRequests
                .Where(sr => sr.ResolvedAt.HasValue)
                .ToList();

            if (resolvedRequests.Count == 0)
                return 0;

            var totalHours = resolvedRequests.Sum(sr => (sr.ResolvedAt.Value - sr.CreatedAt).TotalHours);
            return Math.Round(totalHours / resolvedRequests.Count, 2);
        }
    }
}

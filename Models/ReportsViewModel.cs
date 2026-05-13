using System.ComponentModel.DataAnnotations;

namespace ITSMS.Models
{
    /// <summary>
    /// ViewModel for Reports module - Contains organized data for analytics, charts, and tables
    /// </summary>
    public class ReportsViewModel
    {
        // ========== PRIORITY & CATEGORY DATA ==========
        public List<PriorityData> RequestsByPriority { get; set; } = new();
        public List<CategoryData> RequestsByCategory { get; set; } = new();

        // ========== ERP DATA ==========
        public List<RequestsPerAssetData> RequestsPerAsset { get; set; } = new();
        public List<RequestsPerDepartmentData> RequestsPerDepartment { get; set; } = new();
        public List<AssetStatusSummaryData> AssetStatusSummary { get; set; } = new();

        // ========== TECHNICIAN PERFORMANCE ==========
        public List<TechnicianPerformance> TechnicianPerformances { get; set; } = new();

        // ========== SERVICE REQUESTS (DETAILED) ==========
        public List<ServiceRequestDetail> ServiceRequestDetails { get; set; } = new();

        // ========== FILTERS (FOR UI) ==========
        public string? SelectedStatus { get; set; }
        public string? SelectedPriority { get; set; }
        public string? SearchQuery { get; set; }

        // ========== SUMMARY STATISTICS ==========
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int InProgressRequests { get; set; }
        public int ResolvedRequests { get; set; }
        public int ClosedRequests { get; set; }
        public int CriticalRequests { get; set; }
        public double AverageResolutionTime { get; set; }

        // ========== PAGINATION ==========
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// Represents grouped data for requests by priority (Low, Medium, High, Critical)
    /// </summary>
    public class PriorityData
    {
        public string? Priority { get; set; }
        public int Count { get; set; }
        public string? Color { get; set; } // For chart colors
    }

    /// <summary>
    /// Represents grouped data for requests by category
    /// </summary>
    public class CategoryData
    {
        public string? Category { get; set; }
        public int Count { get; set; }
        public string? Color { get; set; } // For chart colors
    }

    /// <summary>
    /// Technician performance metrics
    /// </summary>
    public class TechnicianPerformance
    {
        public int TechnicianId { get; set; }
        public string? TechnicianName { get; set; }
        public int AssignedTickets { get; set; }
        public int CompletedTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int PendingTickets { get; set; }
        
        [Display(Name = "Completion Rate")]
        public decimal CompletionRate
        {
            get
            {
                if (AssignedTickets == 0) return 0;
                return Math.Round((decimal)CompletedTickets / AssignedTickets * 100, 2);
            }
        }

        public DateTime LastActive { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Detailed view of a service request for reports
    /// </summary>
    public class ServiceRequestDetail
    {
        public int RequestId { get; set; }
        public string? RequestNumber { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? Requestor { get; set; }
        public string? RequestorEmail { get; set; }
        public string? AssignedTechnician { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        [Display(Name = "Days Open")]
        public int DaysOpen
        {
            get
            {
                var endDate = ClosedAt ?? ResolvedAt ?? DateTime.Now;
                return (int)(endDate - CreatedAt).TotalDays;
            }
        }

        [Display(Name = "Resolution Time (Hours)")]
        public double? ResolutionTimeHours
        {
            get
            {
                if (ResolvedAt.HasValue)
                    return (ResolvedAt.Value - CreatedAt).TotalHours;
                return null;
            }
        }
    }

    // ========== ERP DATA CLASSES ==========

    public class RequestsPerAssetData
    {
        public int AssetId { get; set; }
        public string? AssetTag { get; set; }
        public string? AssetName { get; set; }
        public int RequestCount { get; set; }
    }

    public class RequestsPerDepartmentData
    {
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int RequestCount { get; set; }
    }

    public class AssetStatusSummaryData
    {
        public string? Status { get; set; }
        public int Count { get; set; }
        public string? Color { get; set; }
    }
}

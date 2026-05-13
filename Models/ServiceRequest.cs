using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSMS.Models
{
    /// <summary>
    /// ServiceRequest entity - Main ticketing system for service requests
    /// </summary>
    public class ServiceRequest
    {
        [Key]
        public int RequestId { get; set; }

        [StringLength(20)]
        public string? RequestNumber { get; set; } // e.g., REQ-001, REQ-002 - Auto-generated

        [Required]
        [StringLength(150, MinimumLength = 5)]
        public string Title { get; set; }

        [Required]
        [MinLength(10)]
        public string Description { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int RequestorId { get; set; }

        public int? AssignedTechnicianId { get; set; } // Nullable - assigned later

        public int? AssetId { get; set; } // Nullable ERP integration

        public int? EmployeeId { get; set; } // Nullable ERP integration

        [Required]
        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;

        [Required]
        public ServiceRequestPriority Priority { get; set; } = ServiceRequestPriority.Medium;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public DateTime? ResolvedAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        // Navigation properties
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        [ForeignKey("RequestorId")]
        public User? Requestor { get; set; }

        [ForeignKey("AssignedTechnicianId")]
        public User? AssignedTechnician { get; set; }

        [ForeignKey("AssetId")]
        public Asset? Asset { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }

        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

        public Feedback? Feedback { get; set; }
        
        public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    }

    /// <summary>
    /// ENUM for Service Request Status
    /// </summary>
    public enum ServiceRequestStatus
    {
        Pending = 0,
        [Display(Name = "In Progress")]
        InProgress = 1,
        [Display(Name = "On Hold")]
        OnHold = 2,
        Resolved = 3,
        Closed = 4
    }

    /// <summary>
    /// ENUM for Service Request Priority
    /// </summary>
    public enum ServiceRequestPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }
}

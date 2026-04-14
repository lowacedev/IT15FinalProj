using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSMS.Models
{
    /// <summary>
    /// Assignment entity - Tracks assignment of service requests to technicians
    /// Maintains assignment history and current active assignment
    /// </summary>
    public class Assignment
    {
        [Key]
        public int AssignmentId { get; set; }

        [Required]
        public int RequestId { get; set; }

        [Required]
        public int TechnicianId { get; set; }

        [Required]
        public int AssignedBy { get; set; } // Admin/Manager who made assignment

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsActive { get; set; } = true; // Current assignment = true, reassigned = false

        [StringLength(255)]
        public string Notes { get; set; }

        // Navigation properties
        [ForeignKey("RequestId")]
        public ServiceRequest Request { get; set; }

        [ForeignKey("TechnicianId")]
        public User Technician { get; set; }

        [ForeignKey("AssignedBy")]
        public User AssignedByUser { get; set; }
    }
}

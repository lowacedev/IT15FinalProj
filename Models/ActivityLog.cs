using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSMS.Models
{
    /// <summary>
    /// ActivityLog entity (OPTIONAL) - Audit trail for system activities
    /// Tracks all user actions for compliance and debugging
    /// </summary>
    public class ActivityLog
    {
        [Key]
        public int LogId { get; set; }

        public int? UserId { get; set; }

        [StringLength(50)]
        public string Entity { get; set; } // e.g., "ServiceRequest", "Assignment"

        public int? EntityId { get; set; }

        [StringLength(50)]
        public string Action { get; set; } // e.g., "Create", "Update", "Delete"

        public string OldValue { get; set; }

        public string NewValue { get; set; }

        [StringLength(50)]
        public string IPAddress { get; set; }

        public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}

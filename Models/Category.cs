using System.ComponentModel.DataAnnotations;

namespace ITSMS.Models
{
    /// <summary>
    /// Category entity for service request categories
    /// Examples: Hardware, Software, Network, Email, Security, Other
    /// </summary>
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string CategoryName { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}

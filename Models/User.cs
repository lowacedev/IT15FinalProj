using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSMS.Models
{
    /// <summary>
    /// User entity representing system users (Admins, Technicians, Clients)
    /// </summary>
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Username can only contain letters, numbers, and characters: . - _")]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public int RoleId { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("RoleId")]
        public Role? Role { get; set; }

        public ICollection<ServiceRequest> RequestsCreated { get; set; } = new List<ServiceRequest>();
        
        public ICollection<ServiceRequest> RequestsAssigned { get; set; } = new List<ServiceRequest>();
        
        public ICollection<Assignment> AssignmentsMade { get; set; } = new List<Assignment>();
        
        public ICollection<Assignment> AssignmentsReceived { get; set; } = new List<Assignment>();
        
        public ICollection<Feedback> FeedbackProvided { get; set; } = new List<Feedback>();

        // 1:1 relationship with Employee
        public Employee? Employee { get; set; }

        // Helper property for display
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}

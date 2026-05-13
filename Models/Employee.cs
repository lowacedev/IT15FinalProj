using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSMS.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [StringLength(20)]
        [Display(Name = "Employee Code")]
        public string? EmployeeCode { get; set; }

        [Required]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [StringLength(100)]
        public string? Position { get; set; }

        [Required]
        public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

        [StringLength(20)]
        [Display(Name = "Employee Number")]
        public string? EmployeeNumber { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Hire Date")]
        public DateTime? HireDate { get; set; }

        [StringLength(50)]
        [Display(Name = "Employment Status")]
        public string? EmploymentStatus { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Salary Rate")]
        public decimal SalaryRate { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        public ICollection<AssetAssignment> AssetAssignments { get; set; } = new List<AssetAssignment>();
        
        // Employees can have associated Service Requests
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }

    public enum EmployeeStatus
    {
        Active = 1,
        Inactive = 0
    }
}

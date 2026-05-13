using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSMS.Models
{
    public class Payroll
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Employee")]
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Payroll Month")]
        public string PayrollMonth { get; set; } = DateTime.Now.ToString("MMMM yyyy");

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Basic Salary")]
        public decimal BasicSalary { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Allowance { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Deduction { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Overtime Pay")]
        public decimal OvertimePay { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Net Salary")]
        public decimal NetSalary { get; set; }

        [Required]
        [Display(Name = "Status")]
        public PayrollStatus PayrollStatus { get; set; } = PayrollStatus.Pending;

        // Navigation property
        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }
    }

    public enum PayrollStatus
    {
        Pending = 0,
        Paid = 1
    }
}

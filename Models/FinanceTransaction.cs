using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSMS.Models
{
    public class FinanceTransaction
    {
        [Key]
        public int Id { get; set; }

        public int? ServiceRequestId { get; set; }

        public int? AssetId { get; set; }

        public int? DepartmentId { get; set; }

        [Required]
        [Display(Name = "Transaction Type")]
        public FinanceTransactionType TransactionType { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Transaction Date")]
        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [Required]
        public int CreatedByUserId { get; set; }

        // Navigation Properties
        [ForeignKey("ServiceRequestId")]
        public ServiceRequest? ServiceRequest { get; set; }

        [ForeignKey("AssetId")]
        public Asset? Asset { get; set; }

        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        [ForeignKey("CreatedByUserId")]
        public User? CreatedByUser { get; set; }
    }

    public enum FinanceTransactionType
    {
        Repair = 1,
        Purchase = 2,
        Maintenance = 3,
        Upgrade = 4
    }
}

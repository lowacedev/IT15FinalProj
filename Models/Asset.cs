using System.ComponentModel.DataAnnotations;

namespace ITSMS.Models
{
    public class Asset
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Asset Tag")]
        public string AssetTag { get; set; } // e.g., PC-001

        [Required]
        [StringLength(150)]
        [Display(Name = "Asset Name")]
        public string AssetName { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; } // Hardware, Peripheral, etc.

        [Required]
        public AssetStatus Status { get; set; } = AssetStatus.Working;

        [DataType(DataType.Date)]
        [Display(Name = "Purchase Date")]
        public DateTime? PurchaseDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Warranty Expiry")]
        public DateTime? WarrantyExpiry { get; set; }

        // Navigation properties
        public ICollection<AssetAssignment> Assignments { get; set; } = new List<AssetAssignment>();
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }

    public enum AssetStatus
    {
        Working = 1,
        Defective = 2,
        [Display(Name = "Under Repair")]
        UnderRepair = 3,
        Retired = 4
    }
}

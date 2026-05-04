using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITSMS.Models
{
    public class ServiceRequestCreateViewModel
    {
        [Required]
        [StringLength(150, MinimumLength = 5)]
        public string Title { get; set; }

        [Required]
        [MinLength(10)]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public ServiceRequestPriority Priority { get; set; } = ServiceRequestPriority.Medium;

        [Display(Name = "Asset (Optional)")]
        public int? AssetId { get; set; }
        
        public List<SelectListItem> EmployeeAssets { get; set; } = new List<SelectListItem>();
    }
}

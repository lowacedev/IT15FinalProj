using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSMS.Models
{
    /// <summary>
    /// Feedback entity - Customer feedback on service requests
    /// One feedback per service request (UNIQUE constraint)
    /// </summary>
    public class Feedback
    {
        [Key]
        public int FeedbackId { get; set; }

        [Required]
        public int RequestId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string Comments { get; set; }

        [Required]
        public int ProvidedBy { get; set; }

        public DateTime ProvidedAt { get; set; } = DateTime.Now;

        // Navigation properties (nullable to exclude from validation)
        [ForeignKey("RequestId")]
        public ServiceRequest? Request { get; set; }

        [ForeignKey("ProvidedBy")]
        public User? User { get; set; }
    }
}

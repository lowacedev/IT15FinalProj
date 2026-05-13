using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSMS.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Action { get; set; } // CREATE, UPDATE, DELETE, ASSIGN

        [Required]
        [MaxLength(100)]
        public string Module { get; set; } // ServiceRequest, Asset, User, Employee

        [Required]
        [MaxLength(500)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}

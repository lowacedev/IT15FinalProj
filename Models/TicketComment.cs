using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSMS.Models
{
    public class TicketComment
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        public int RequestId { get; set; }

        [Required]
        public int AuthorId { get; set; }

        [Required]
        public string Body { get; set; }

        public bool IsInternal { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("RequestId")]
        public ServiceRequest? ServiceRequest { get; set; }

        [ForeignKey("AuthorId")]
        public User? Author { get; set; }
    }
}

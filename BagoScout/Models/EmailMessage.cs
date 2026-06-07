using System.ComponentModel.DataAnnotations;

namespace BagoScout.Models
{
    public class EmailMessage
    {
        [Key]
        public int EmailId { get; set; }

        [Required]
        public int ApplicationId { get; set; }

        [Required]
        public int SenderId { get; set; }

        [Required]
        public int ReceiverId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;
    }
}

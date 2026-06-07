using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BagoScout.Models
{
    public class Application
    {
        [Key]
        public int ApplicationId { get; set; }

        [Required]
        public int JobId { get; set; }

        [Required]
        public int SeekerId { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Reviewed, Accepted, Rejected

        public string CoverLetter { get; set; } = string.Empty;

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }

        // Navigation properties
        [ForeignKey("SeekerId")]
        public virtual User? Seeker { get; set; }

        [ForeignKey("JobId")]
        public virtual Job? Job { get; set; }
    }
}

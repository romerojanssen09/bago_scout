using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BagoScout.Models
{
    public class JobPreference
    {
        [Key]
        public int JobPreferenceId { get; set; }

        [Required]
        public int UserId { get; set; }

        [MaxLength(100)]
        public string? PreferredJobType { get; set; } // Full-time, Part-time, Contract, etc.

        [MaxLength(255)]
        public string? PreferredJobTitles { get; set; } // Comma-separated job titles

        public int? MinSalary { get; set; }

        public int? MaxSalary { get; set; }

        [MaxLength(100)]
        public string? PreferredLocation { get; set; }

        public double? PreferredLatitude { get; set; }

        public double? PreferredLongitude { get; set; }

        public int? MaxDistance { get; set; } // in kilometers

        [MaxLength(100)]
        public string? PreferredExperienceLevel { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}

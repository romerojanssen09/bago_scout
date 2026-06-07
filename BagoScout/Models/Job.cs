using System.ComponentModel.DataAnnotations;

namespace BagoScout.Models
{
    public class Job
    {
        [Key]
        public int JobId { get; set; }

        [Required]
        public int EmployerId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Company { get; set; } = string.Empty;

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(100)]
        public string SalaryRange { get; set; } = string.Empty;

        [MaxLength(50)]
        public string JobType { get; set; } = string.Empty; // Full-time, Part-time, Contract

        [MaxLength(50)]
        public string ExperienceLevel { get; set; } = string.Empty; // Entry, Mid, Senior

        public string Requirements { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}

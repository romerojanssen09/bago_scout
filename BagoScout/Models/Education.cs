using System.ComponentModel.DataAnnotations;

namespace BagoScout.Models
{
    public class Education
    {
        [Key]
        public int EducationId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string School { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Degree { get; set; } = string.Empty;

        [MaxLength(200)]
        public string FieldOfStudy { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BagoScout.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string UserType { get; set; } = string.Empty; // "Seeker" or "Employer"

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public bool IsEmailVerified { get; set; } = false;

        public string? VerificationCode { get; set; }

        public DateTime? VerificationCodeExpiry { get; set; }

        public string? SelfiePhotoPath { get; set; }

        public string? IdPhotoPath { get; set; }

        // Company Profile Fields (for Employers)
        [MaxLength(255)]
        public string? CompanyName { get; set; }

        public string? CompanyDescription { get; set; }

        [MaxLength(255)]
        public string? CompanyWebsite { get; set; }

        [MaxLength(500)]
        public string? CompanyAddress { get; set; }

        public double? CompanyLatitude { get; set; }

        public double? CompanyLongitude { get; set; }

        [MaxLength(100)]
        public string? CompanyIndustry { get; set; }

        [MaxLength(50)]
        public string? CompanySize { get; set; }

        public string? CompanyLogoPath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Mobile Auth & Push Notifications
        public string? AuthToken { get; set; }
        public DateTime? AuthTokenExpiry { get; set; }
        public string? FcmToken { get; set; }

        // Navigation properties
        public virtual ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
        public virtual ICollection<UserPreference> UserPreferences { get; set; } = new List<UserPreference>();
    }
}

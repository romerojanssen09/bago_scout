using Microsoft.AspNetCore.Mvc;
using BagoScout.Data;
using BagoScout.Models;
using Microsoft.EntityFrameworkCore;

namespace BagoScout.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Redirect("/?login=true");
            }

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View();
        }

        [HttpGet]
        [Route("api/profile/seeker")]
        public async Task<IActionResult> GetSeekerProfile()
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                var user = await _context.Users
                    .Where(u => u.UserId == userId.Value)
                    .Select(u => new
                    {
                        u.UserId,
                        u.FirstName,
                        u.LastName,
                        u.Email,
                        u.PhoneNumber,
                        u.SelfiePhotoPath
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Get user skills
                var skills = await _context.UserSkills
                    .Where(us => us.UserId == userId.Value)
                    .Join(_context.Skills,
                        us => us.SkillId,
                        s => s.SkillId,
                        (us, s) => s.SkillName)
                    .ToListAsync();

                // Get education
                var education = await _context.Educations
                    .Where(e => e.UserId == userId.Value)
                    .OrderByDescending(e => e.StartDate)
                    .ToListAsync();

                // Get experience
                var experience = await _context.Experiences
                    .Where(e => e.UserId == userId.Value)
                    .OrderByDescending(e => e.StartDate)
                    .ToListAsync();

                return Ok(new
                {
                    user.UserId,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.PhoneNumber,
                    user.SelfiePhotoPath,
                    Skills = skills,
                    Education = education,
                    Experience = experience
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching profile", error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/profile/seeker/{seekerId}")]
        public async Task<IActionResult> GetSeekerProfileById(int seekerId)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.UserId == seekerId)
                    .Select(u => new
                    {
                        u.UserId,
                        u.FirstName,
                        u.LastName,
                        u.Email,
                        u.PhoneNumber,
                        u.SelfiePhotoPath
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Get user skills
                var skills = await _context.UserSkills
                    .Where(us => us.UserId == seekerId)
                    .Join(_context.Skills,
                        us => us.SkillId,
                        s => s.SkillId,
                        (us, s) => s.SkillName)
                    .ToListAsync();

                // Get education
                var education = await _context.Educations
                    .Where(e => e.UserId == seekerId)
                    .OrderByDescending(e => e.StartDate)
                    .ToListAsync();

                // Get experience
                var experience = await _context.Experiences
                    .Where(e => e.UserId == seekerId)
                    .OrderByDescending(e => e.StartDate)
                    .ToListAsync();

                return Ok(new
                {
                    user.UserId,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.PhoneNumber,
                    user.SelfiePhotoPath,
                    Skills = skills,
                    Education = education,
                    Experience = experience
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching profile", error = ex.Message });
            }
        }

        [HttpPut]
        [Route("api/profile/update")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateRequest request)
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                var user = await _context.Users.FindAsync(userId.Value);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.PhoneNumber = request.PhoneNumber;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Update session
                HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");

                return Ok(new { message = "Profile updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating profile", error = ex.Message });
            }
        }

        [HttpPut]
        [Route("api/profile/skills")]
        public async Task<IActionResult> UpdateSkills([FromBody] SkillsUpdateRequest request)
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                // Remove existing skills
                var existingSkills = await _context.UserSkills
                    .Where(us => us.UserId == userId.Value)
                    .ToListAsync();

                _context.UserSkills.RemoveRange(existingSkills);

                // Add new skills
                foreach (var skillId in request.SkillIds)
                {
                    _context.UserSkills.Add(new UserSkill
                    {
                        UserId = userId.Value,
                        SkillId = skillId,
                        AddedAt = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Skills updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating skills", error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/skills/all")]
        public async Task<IActionResult> GetAllSkills()
        {
            try
            {
                var skills = await _context.Skills
                    .OrderBy(s => s.SkillName)
                    .Select(s => new
                    {
                        s.SkillId,
                        s.SkillName
                    })
                    .ToListAsync();

                return Ok(skills);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching skills", error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/skills/create")]
        public async Task<IActionResult> CreateCustomSkill([FromBody] CustomSkillRequest request)
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                // Check if skill already exists
                var existingSkill = await _context.Skills
                    .FirstOrDefaultAsync(s => s.SkillName.ToLower() == request.SkillName.ToLower());

                int skillId;
                if (existingSkill != null)
                {
                    skillId = existingSkill.SkillId;
                }
                else
                {
                    // Create new skill
                    var newSkill = new Skill
                    {
                        SkillName = request.SkillName,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Skills.Add(newSkill);
                    await _context.SaveChangesAsync();
                    skillId = newSkill.SkillId;
                }

                // Add skill to user if not already added
                var userSkillExists = await _context.UserSkills
                    .AnyAsync(us => us.UserId == userId.Value && us.SkillId == skillId);

                if (!userSkillExists)
                {
                    _context.UserSkills.Add(new UserSkill
                    {
                        UserId = userId.Value,
                        SkillId = skillId,
                        AddedAt = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = "Skill added successfully", skillId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating skill", error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/education/create")]
        public async Task<IActionResult> CreateEducation([FromBody] Education education)
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                education.UserId = userId.Value;
                education.CreatedAt = DateTime.UtcNow;

                _context.Educations.Add(education);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Education added successfully", educationId = education.EducationId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error adding education", error = ex.Message });
            }
        }

        [HttpPut]
        [Route("api/education/update/{id}")]
        public async Task<IActionResult> UpdateEducation(int id, [FromBody] Education education)
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                var existingEducation = await _context.Educations
                    .FirstOrDefaultAsync(e => e.EducationId == id && e.UserId == userId.Value);

                if (existingEducation == null)
                {
                    return NotFound(new { message = "Education not found" });
                }

                existingEducation.School = education.School;
                existingEducation.Degree = education.Degree;
                existingEducation.FieldOfStudy = education.FieldOfStudy;
                existingEducation.StartDate = education.StartDate;
                existingEducation.EndDate = education.EndDate;
                existingEducation.Description = education.Description;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Education updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating education", error = ex.Message });
            }
        }

        [HttpDelete]
        [Route("api/education/delete/{id}")]
        public async Task<IActionResult> DeleteEducation(int id)
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                var education = await _context.Educations
                    .FirstOrDefaultAsync(e => e.EducationId == id && e.UserId == userId.Value);

                if (education == null)
                {
                    return NotFound(new { message = "Education not found" });
                }

                _context.Educations.Remove(education);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Education deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting education", error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/experience/create")]
        public async Task<IActionResult> CreateExperience([FromBody] Experience experience)
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                experience.UserId = userId.Value;
                experience.CreatedAt = DateTime.UtcNow;

                _context.Experiences.Add(experience);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Experience added successfully", experienceId = experience.ExperienceId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error adding experience", error = ex.Message });
            }
        }

        [HttpPut]
        [Route("api/experience/update/{id}")]
        public async Task<IActionResult> UpdateExperience(int id, [FromBody] Experience experience)
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                var existingExperience = await _context.Experiences
                    .FirstOrDefaultAsync(e => e.ExperienceId == id && e.UserId == userId.Value);

                if (existingExperience == null)
                {
                    return NotFound(new { message = "Experience not found" });
                }

                existingExperience.JobTitle = experience.JobTitle;
                existingExperience.Company = experience.Company;
                existingExperience.Location = experience.Location;
                existingExperience.StartDate = experience.StartDate;
                existingExperience.EndDate = experience.EndDate;
                existingExperience.IsCurrentJob = experience.IsCurrentJob;
                existingExperience.Description = experience.Description;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Experience updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating experience", error = ex.Message });
            }
        }

        [HttpDelete]
        [Route("api/experience/delete/{id}")]
        public async Task<IActionResult> DeleteExperience(int id)
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                var experience = await _context.Experiences
                    .FirstOrDefaultAsync(e => e.ExperienceId == id && e.UserId == userId.Value);

                if (experience == null)
                {
                    return NotFound(new { message = "Experience not found" });
                }

                _context.Experiences.Remove(experience);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Experience deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting experience", error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/profile/upload-photo")]
        public async Task<IActionResult> UploadPhoto([FromForm] IFormFile photo)
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                if (photo == null || photo.Length == 0)
                {
                    return BadRequest(new { message = "No file uploaded" });
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(photo.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(new { message = "Invalid file type. Only JPG, PNG, and GIF are allowed." });
                }

                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
                Directory.CreateDirectory(uploadsPath);

                // Generate unique filename
                var fileName = $"{userId}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                // Update user profile
                var user = await _context.Users.FindAsync(userId.Value);
                if (user != null)
                {
                    // Delete old photo if exists
                    if (!string.IsNullOrEmpty(user.SelfiePhotoPath))
                    {
                        var oldPhotoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.SelfiePhotoPath.TrimStart('/'));
                        if (System.IO.File.Exists(oldPhotoPath))
                        {
                            System.IO.File.Delete(oldPhotoPath);
                        }
                    }

                    user.SelfiePhotoPath = $"/uploads/profiles/{fileName}";
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = "Photo uploaded successfully", photoPath = $"/uploads/profiles/{fileName}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error uploading photo", error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/profile/check-completion")]
        public async Task<IActionResult> CheckProfileCompletion()
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                var user = await _context.Users.FindAsync(userId.Value);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Check if user has at least one skill
                var hasSkills = await _context.UserSkills.AnyAsync(us => us.UserId == userId.Value);

                // Check if essential profile fields are filled
                bool isComplete = !string.IsNullOrEmpty(user.FirstName) &&
                                  !string.IsNullOrEmpty(user.LastName) &&
                                  !string.IsNullOrEmpty(user.PhoneNumber) &&
                                  hasSkills;

                return Ok(new { isComplete });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error checking profile completion", error = ex.Message });
            }
        }
    }

    public class ProfileUpdateRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
    }

    public class SkillsUpdateRequest
    {
        public List<int> SkillIds { get; set; } = new List<int>();
    }

    public class CustomSkillRequest
    {
        public string SkillName { get; set; } = string.Empty;
    }
}

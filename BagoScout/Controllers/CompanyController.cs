using Microsoft.AspNetCore.Mvc;
using BagoScout.Data;
using BagoScout.Models;
using Microsoft.EntityFrameworkCore;

namespace BagoScout.Controllers
{
    public class CompanyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CompanyController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Profile()
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
        [Route("api/company/profile")]
        public async Task<IActionResult> GetCompanyProfile()
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
                        u.CompanyName,
                        u.CompanyDescription,
                        u.CompanyWebsite,
                        u.CompanyAddress,
                        u.CompanyLatitude,
                        u.CompanyLongitude,
                        u.CompanyIndustry,
                        u.CompanySize,
                        u.CompanyLogoPath
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching company profile", error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/company/profile/{employerId}")]
        public async Task<IActionResult> GetCompanyProfileById(int employerId)
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);
                
                var user = await _context.Users
                    .Where(u => u.UserId == employerId)
                    .Select(u => new
                    {
                        u.UserId,
                        u.FirstName,
                        u.LastName,
                        u.Email,
                        u.PhoneNumber,
                        u.CompanyName,
                        u.CompanyDescription,
                        u.CompanyWebsite,
                        u.CompanyAddress,
                        u.CompanyLatitude,
                        u.CompanyLongitude,
                        u.CompanyIndustry,
                        u.CompanySize,
                        u.CompanyLogoPath
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new { message = "Company not found" });
                }

                // Check if the current user (seeker) has been accepted by this employer
                bool isAccepted = false;
                if (userId.HasValue)
                {
                    isAccepted = await _context.Applications
                        .Join(_context.Jobs,
                            app => app.JobId,
                            job => job.JobId,
                            (app, job) => new { app, job })
                        .AnyAsync(x => x.app.SeekerId == userId.Value && 
                                      x.job.EmployerId == employerId && 
                                      x.app.Status == "Accepted");
                }

                // Return profile with location only if accepted
                return Ok(new
                {
                    user.UserId,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.PhoneNumber,
                    user.CompanyName,
                    user.CompanyDescription,
                    user.CompanyWebsite,
                    CompanyAddress = isAccepted ? user.CompanyAddress : null,
                    CompanyLatitude = isAccepted ? user.CompanyLatitude : null,
                    CompanyLongitude = isAccepted ? user.CompanyLongitude : null,
                    user.CompanyIndustry,
                    user.CompanySize,
                    user.CompanyLogoPath,
                    IsAccepted = isAccepted
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching company profile", error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/company/check-completion")]
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

                // Check if essential company profile fields are filled
                bool isComplete = !string.IsNullOrEmpty(user.CompanyName) &&
                                  !string.IsNullOrEmpty(user.CompanyDescription) &&
                                  user.CompanyLatitude.HasValue &&
                                  user.CompanyLongitude.HasValue;

                return Ok(new { isComplete });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error checking profile completion", error = ex.Message });
            }
        }

        [HttpPut]
        [Route("api/company/update")]
        public async Task<IActionResult> UpdateCompanyProfile([FromBody] CompanyProfileUpdateRequest request)
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
                user.CompanyName = request.CompanyName;
                user.CompanyDescription = request.CompanyDescription;
                user.CompanyWebsite = request.CompanyWebsite;
                user.CompanyAddress = request.CompanyAddress;
                user.CompanyLatitude = request.CompanyLatitude;
                user.CompanyLongitude = request.CompanyLongitude;
                user.CompanyIndustry = request.CompanyIndustry;
                user.CompanySize = request.CompanySize;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Update session
                HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");

                return Ok(new { message = "Company profile updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating company profile", error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/company/upload-logo")]
        public async Task<IActionResult> UploadLogo([FromForm] IFormFile logo)
        {
            try
            {
                var userId = await this.GetAuthorizedUserIdAsync(_context);
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                if (logo == null || logo.Length == 0)
                {
                    return BadRequest(new { message = "No file uploaded" });
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(logo.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(new { message = "Invalid file type. Only JPG, PNG, and GIF are allowed." });
                }

                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "company-logos");
                Directory.CreateDirectory(uploadsPath);

                // Generate unique filename
                var fileName = $"{userId}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await logo.CopyToAsync(stream);
                }

                // Update user profile
                var user = await _context.Users.FindAsync(userId.Value);
                if (user != null)
                {
                    // Delete old logo if exists
                    if (!string.IsNullOrEmpty(user.CompanyLogoPath))
                    {
                        var oldLogoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.CompanyLogoPath.TrimStart('/'));
                        if (System.IO.File.Exists(oldLogoPath))
                        {
                            System.IO.File.Delete(oldLogoPath);
                        }
                    }

                    user.CompanyLogoPath = $"/uploads/company-logos/{fileName}";
                    user.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = "Logo uploaded successfully", logoPath = $"/uploads/company-logos/{fileName}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error uploading logo", error = ex.Message });
            }
        }
    }

    public class CompanyProfileUpdateRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyDescription { get; set; }
        public string? CompanyWebsite { get; set; }
        public string? CompanyAddress { get; set; }
        public double? CompanyLatitude { get; set; }
        public double? CompanyLongitude { get; set; }
        public string? CompanyIndustry { get; set; }
        public string? CompanySize { get; set; }
    }
}

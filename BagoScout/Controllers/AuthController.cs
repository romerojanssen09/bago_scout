using BagoScout.Data;
using BagoScout.Models;
using BagoScout.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BagoScout.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<AuthController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }



        [HttpGet("check-email/{email}")]
        public async Task<IActionResult> CheckEmail(string email)
        {
            try
            {
                var exists = await _context.Users.AnyAsync(u => u.Email == email);
                return Ok(new { exists = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking email: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred" });
            }
        }

        [HttpPost("send-verification-code")]
        public async Task<IActionResult> SendVerificationCode([FromBody] SendVerificationRequest request)
        {
            try
            {
                _logger.LogInformation($"=== SendVerificationCode called ===");
                _logger.LogInformation($"Request object is null: {request == null}");
                
                if (request != null)
                {
                    _logger.LogInformation($"Email: '{request.Email}', Name: '{request.Name}'");
                }

                // Validate request
                if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Name))
                {
                    _logger.LogWarning("Validation failed - Email or Name is missing");
                    return BadRequest(new { success = false, message = "Email and name are required" });
                }

                // Generate 6-digit code
                var random = new Random();
                var code = random.Next(100000, 999999).ToString();

                // Store code in session for temporary verification
                HttpContext.Session.SetString($"VerificationCode_{request.Email}", code);
                HttpContext.Session.SetString($"VerificationCodeExpiry_{request.Email}", 
                    DateTime.UtcNow.AddMinutes(15).ToString("o"));
                
                _logger.LogInformation($"Verification code for {request.Email}: {code}");

                // Send email
                var emailSent = await _emailService.SendVerificationEmailAsync(
                    request.Email,
                    request.Name,
                    code
                );

                if (emailSent)
                {
                    return Ok(new { 
                        success = true, 
                        message = "Verification code sent successfully",
                        // Remove this in production - only for testing
                        code = code 
                    });
                }

                // Email failed but code is still in session for testing
                _logger.LogWarning($"Email failed to send but code is stored in session for {request.Email}");
                return Ok(new { 
                    success = true, 
                    message = "Verification code generated (email delivery failed)",
                    code = code,
                    warning = "Email service unavailable - using code directly"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending verification code: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
                {
                    return BadRequest(new { success = false, message = "Email and code are required" });
                }

                _logger.LogInformation($"Verifying email for {request.Email} with code {request.Code}");

                // Check session for verification code (for new registrations)
                var sessionCode = HttpContext.Session.GetString($"VerificationCode_{request.Email}");
                var sessionExpiry = HttpContext.Session.GetString($"VerificationCodeExpiry_{request.Email}");

                _logger.LogInformation($"Session code: {sessionCode}, Session expiry: {sessionExpiry}");

                if (!string.IsNullOrEmpty(sessionCode) && !string.IsNullOrEmpty(sessionExpiry))
                {
                    // Verify from session
                    if (sessionCode != request.Code)
                    {
                        _logger.LogWarning($"Invalid code for {request.Email}. Expected: {sessionCode}, Got: {request.Code}");
                        return BadRequest(new { success = false, message = "Invalid verification code" });
                    }

                    if (DateTime.Parse(sessionExpiry) < DateTime.UtcNow)
                    {
                        _logger.LogWarning($"Expired code for {request.Email}");
                        return BadRequest(new { success = false, message = "Verification code has expired" });
                    }

                    // Clear session data
                    HttpContext.Session.Remove($"VerificationCode_{request.Email}");
                    HttpContext.Session.Remove($"VerificationCodeExpiry_{request.Email}");

                    _logger.LogInformation($"Email verified successfully for {request.Email}");
                    return Ok(new { success = true, message = "Email verified successfully" });
                }

                // Check database for existing users
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null)
                {
                    _logger.LogWarning($"User not found for email {request.Email}");
                    return NotFound(new { success = false, message = "User not found" });
                }

                if (user.VerificationCode != request.Code)
                {
                    _logger.LogWarning($"Invalid DB code for {request.Email}");
                    return BadRequest(new { success = false, message = "Invalid verification code" });
                }

                if (user.VerificationCodeExpiry < DateTime.UtcNow)
                {
                    _logger.LogWarning($"Expired DB code for {request.Email}");
                    return BadRequest(new { success = false, message = "Verification code has expired" });
                }

                // Mark email as verified
                user.IsEmailVerified = true;
                user.VerificationCode = null;
                user.VerificationCodeExpiry = null;
                await _context.SaveChangesAsync();

                // Send welcome email
                await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName);

                _logger.LogInformation($"Email verified and welcome email sent for {request.Email}");
                return Ok(new { success = true, message = "Email verified successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error verifying email: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                {
                    return BadRequest(new { success = false, message = "Email already registered" });
                }

                // Create user with email already verified (since we verified in step 3)
                var user = new User
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    UserType = request.UserType,
                    IsEmailVerified = true, // Already verified in step 3
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Send welcome email
                await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName);

                return Ok(new { 
                    success = true, 
                    message = "Registration successful!",
                    userId = user.UserId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during registration: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred during registration" });
            }
        }

        [HttpPost("upload-photos")]
        public async Task<IActionResult> UploadPhotos([FromForm] PhotoUploadRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                // Create user directory
                var userFolder = Path.Combine("wwwroot", "uploads", request.UserId.ToString());
                Directory.CreateDirectory(userFolder);

                // Save selfie photo
                if (request.SelfiePhoto != null)
                {
                    var selfieFileName = $"selfie_{DateTime.UtcNow.Ticks}{Path.GetExtension(request.SelfiePhoto.FileName)}";
                    var selfiePath = Path.Combine(userFolder, selfieFileName);
                    
                    using (var stream = new FileStream(selfiePath, FileMode.Create))
                    {
                        await request.SelfiePhoto.CopyToAsync(stream);
                    }
                    
                    user.SelfiePhotoPath = $"/uploads/{request.UserId}/{selfieFileName}";
                }

                // Save ID photo
                if (request.IdPhoto != null)
                {
                    var idFileName = $"id_{DateTime.UtcNow.Ticks}{Path.GetExtension(request.IdPhoto.FileName)}";
                    var idPath = Path.Combine(userFolder, idFileName);
                    
                    using (var stream = new FileStream(idPath, FileMode.Create))
                    {
                        await request.IdPhoto.CopyToAsync(stream);
                    }
                    
                    user.IdPhotoPath = $"/uploads/{request.UserId}/{idFileName}";
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Photos uploaded successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading photos: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while uploading photos" });
            }
        }

        [HttpPost("save-skills")]
        public async Task<IActionResult> SaveSkills([FromBody] SaveSkillsRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                if (user.UserType == "seeker")
                {
                    // Save job seeker skills
                    foreach (var skillName in request.Skills)
                    {
                        // Find or create skill
                        var skill = await _context.Skills.FirstOrDefaultAsync(s => s.SkillName == skillName);
                        if (skill == null)
                        {
                            skill = new Skill { SkillName = skillName };
                            _context.Skills.Add(skill);
                            await _context.SaveChangesAsync();
                        }

                        // Add user skill if not exists
                        var userSkillExists = await _context.UserSkills
                            .AnyAsync(us => us.UserId == request.UserId && us.SkillId == skill.SkillId);
                        
                        if (!userSkillExists)
                        {
                            _context.UserSkills.Add(new UserSkill
                            {
                                UserId = request.UserId,
                                SkillId = skill.SkillId
                            });
                        }
                    }
                }
                else if (user.UserType == "employer")
                {
                    // Save employer preferences
                    foreach (var prefName in request.Skills)
                    {
                        // Find or create skill
                        var skill = await _context.Skills.FirstOrDefaultAsync(s => s.SkillName == prefName);
                        if (skill == null)
                        {
                            skill = new Skill { SkillName = prefName };
                            _context.Skills.Add(skill);
                            await _context.SaveChangesAsync();
                        }

                        // Add user preference if not exists
                        var userPrefExists = await _context.UserPreferences
                            .AnyAsync(up => up.UserId == request.UserId && up.SkillId == skill.SkillId);
                        
                        if (!userPrefExists)
                        {
                            _context.UserPreferences.Add(new UserPreference
                            {
                                UserId = request.UserId,
                                SkillId = skill.SkillId
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Skills saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving skills: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred while saving skills" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null)
                {
                    return BadRequest(new { success = false, message = "Invalid email or password" });
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return BadRequest(new { success = false, message = "Invalid email or password" });
                }

                if (!user.IsActive)
                {
                    return BadRequest(new { success = false, message = "Account is deactivated" });
                }

                // Store user info in session
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserType", user.UserType);
                HttpContext.Session.SetString("UserName", $"{user.FirstName} {user.LastName}");

                // Generate and set JWT token in Response Cookies for the WebApp
                var token = BagoScout.Services.JwtHelper.GenerateToken(
                    user.UserId.ToString(), 
                    user.Email, 
                    user.UserType.ToLower(), 
                    $"{user.FirstName} {user.LastName}"
                );
                Response.Cookies.Append("jwt_token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, // Set to true in production if HTTPS is required
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(30)
                });

                return Ok(new { 
                    success = true, 
                    message = "Login successful",
                    userType = user.UserType,
                    userId = user.UserId,
                    name = $"{user.FirstName} {user.LastName}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during login: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred during login" });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("jwt_token");
            return Ok(new { success = true, message = "Logged out successfully" });
        }

        [HttpGet("test-user/{email}")]
        public async Task<IActionResult> TestUser(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return Ok(new { exists = false, message = "User not found" });
            }

            return Ok(new { 
                exists = true, 
                userId = user.UserId,
                email = user.Email,
                userType = user.UserType,
                isActive = user.IsActive,
                isEmailVerified = user.IsEmailVerified,
                hasPassword = !string.IsNullOrEmpty(user.PasswordHash)
            });
        }

        [HttpGet("test-session")]
        public IActionResult TestSession()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userType = HttpContext.Session.GetString("UserType");
            var userName = HttpContext.Session.GetString("UserName");

            return Ok(new {
                hasSession = userId != null,
                userId = userId,
                userEmail = userEmail,
                userType = userType,
                userName = userName,
                sessionId = HttpContext.Session.Id
            });
        }

        [HttpPost("forgot-password/request")]
        public async Task<IActionResult> ForgotPasswordRequest([FromBody] ForgotPasswordRequestModel request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { success = false, message = "Email is required" });
                }

                var email = request.Email.Trim();
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (user != null)
                {
                    // Generate 6-digit OTP
                    var random = new Random();
                    var code = random.Next(100000, 999999).ToString();

                    user.VerificationCode = code;
                    user.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(15);
                    await _context.SaveChangesAsync();

                    // Send password reset OTP email
                    var emailSent = await _emailService.SendPasswordResetOtpEmailAsync(
                        user.Email,
                        user.FirstName,
                        code
                    );

                    if (emailSent)
                    {
                        return Ok(new { 
                            success = true, 
                            message = "If the email is registered, we have sent an OTP code to it.",
                            code = code // For testing/local debugging
                        });
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to send password reset OTP to {email}, but code is stored in DB.");
                        return Ok(new { 
                            success = true, 
                            message = "If the email is registered, we have generated an OTP code for it (email delivery failed).",
                            code = code, // For testing/local debugging
                            warning = "Email service unavailable - using code directly"
                        });
                    }
                }

                // Return success to prevent email harvesting
                return Ok(new { 
                    success = true, 
                    message = "If the email is registered, we have sent an OTP code to it." 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ForgotPasswordRequest: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { success = false, message = $"An error occurred: {ex.GetType().Name}: {ex.Message}" });
            }
        }

        [HttpPost("forgot-password/verify")]
        public async Task<IActionResult> ForgotPasswordVerify([FromBody] VerifyForgotPasswordOtpRequestModel request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
                {
                    return BadRequest(new { success = false, message = "Email and code are required" });
                }

                var email = request.Email.Trim();
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    return BadRequest(new { success = false, message = "Invalid code or email" });
                }

                if (user.VerificationCode != request.Code)
                {
                    return BadRequest(new { success = false, message = "Invalid verification code" });
                }

                if (user.VerificationCodeExpiry < DateTime.UtcNow)
                {
                    return BadRequest(new { success = false, message = "Verification code has expired" });
                }

                return Ok(new { success = true, message = "OTP verified successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ForgotPasswordVerify: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { success = false, message = $"An error occurred: {ex.GetType().Name}: {ex.Message}" });
            }
        }

        [HttpPost("forgot-password/reset")]
        public async Task<IActionResult> ForgotPasswordReset([FromBody] ResetPasswordRequestModel request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new { success = false, message = "Email, code, and new password are required" });
                }

                var email = request.Email.Trim();
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    return BadRequest(new { success = false, message = "Invalid request" });
                }

                if (user.VerificationCode != request.Code)
                {
                    return BadRequest(new { success = false, message = "Invalid verification code" });
                }

                if (user.VerificationCodeExpiry < DateTime.UtcNow)
                {
                    return BadRequest(new { success = false, message = "Verification code has expired" });
                }

                // Password strength validation
                var password = request.Password;
                var hasLength = password.Length >= 8;
                var hasUppercase = System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]");
                var hasNumber = System.Text.RegularExpressions.Regex.IsMatch(password, @"[0-9]");
                var hasSpecial = System.Text.RegularExpressions.Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>_\-+=]");

                if (!hasLength || !hasUppercase || !hasNumber || !hasSpecial)
                {
                    return BadRequest(new { success = false, message = "Password does not meet validation requirements" });
                }

                // Reset password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                user.VerificationCode = null;
                user.VerificationCodeExpiry = null;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Password reset successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ForgotPasswordReset: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { success = false, message = $"An error occurred: {ex.GetType().Name}: {ex.Message}" });
            }
        }
    }

    // Request models
    public class SendVerificationRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class VerifyEmailRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    public class PhotoUploadRequest
    {
        public int UserId { get; set; }
        public IFormFile? SelfiePhoto { get; set; }
        public IFormFile? IdPhoto { get; set; }
    }

    public class SaveSkillsRequest
    {
        public int UserId { get; set; }
        public List<string> Skills { get; set; } = new List<string>();
    }

    public class ForgotPasswordRequestModel
    {
        public string Email { get; set; } = string.Empty;
    }

    public class VerifyForgotPasswordOtpRequestModel
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class ResetPasswordRequestModel
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

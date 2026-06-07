using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BagoScout.Data;
using BagoScout.Models;
using BCrypt.Net;

namespace BagoScout.Controllers
{
    [Route("api/mobile")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public class MobileApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MobileApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                return Unauthorized(new { success = false, message = "Invalid email or password" });
            }

            // Generate JWT Token
            var token = BagoScout.Services.JwtHelper.GenerateToken(user.UserId.ToString(), user.Email, user.UserType.ToLower(), $"{user.FirstName} {user.LastName}");

            // Save FCM token if provided atomically
            if (!string.IsNullOrEmpty(model.FcmToken))
            {
                user.FcmToken = model.FcmToken;
            }

            user.AuthToken = token;
            user.AuthTokenExpiry = DateTime.UtcNow.AddDays(30);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                token = token,
                userId = user.UserId,
                userType = user.UserType.ToLower(),
                name = $"{user.FirstName} {user.LastName}"
            });
        }

        [HttpPost("validate-token")]
        public async Task<IActionResult> ValidateToken([FromBody] TokenModel model)
        {
            if (!BagoScout.Services.JwtHelper.ValidateToken(model.Token, out var claims) || claims == null)
            {
                return Unauthorized(new { success = false, message = "Invalid or expired token" });
            }

            if (claims.TryGetValue("sub", out var subVal) && int.TryParse(subVal, out var userId))
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    return Ok(new
                    {
                        success = true,
                        userId = user.UserId,
                        userType = user.UserType.ToLower(),
                        name = $"{user.FirstName} {user.LastName}"
                    });
                }
            }

            return Unauthorized(new { success = false, message = "User not found" });
        }

        [HttpPost("register-fcm")]
        public async Task<IActionResult> RegisterFcm([FromBody] FcmModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.AuthToken == model.Token);
            if (user == null) return Unauthorized();

            user.FcmToken = model.FcmToken;
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpPost("applications/{id}/read")]
        public async Task<IActionResult> ReadApplication(int id, [FromBody] TokenModel model)
        {
            var employer = await _context.Users.FirstOrDefaultAsync(u => u.AuthToken == model.Token);
            if (employer == null || employer.UserType != "Employer") return Unauthorized();

            var application = await _context.Applications
                .Include(a => a.Seeker)
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.ApplicationId == id);

            if (application == null) return NotFound();

            // Only update if not already read (optional logic)
            // application.IsRead = true; // Assuming there's an IsRead field or similar

            // Send push notification to the seeker
            if (application.Seeker != null && !string.IsNullOrEmpty(application.Seeker.FcmToken))
            {
                // In a real app, call FirebaseAdmin SDK here. 
                // For now, we'll log it. I'll add the FirebaseAdmin logic once the NuGet is added.
                System.Diagnostics.Debug.WriteLine($"Sending push notification to {application.Seeker.Email} for job {application.Job?.Title}");
            }

            return Ok(new { success = true });
        }

        [HttpGet("dashboard/seeker")]
        public async Task<IActionResult> GetSeekerDashboard([FromQuery] string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.AuthToken == token);
            if (user == null) return Unauthorized();

            var appCount = await _context.Applications.CountAsync(a => a.SeekerId == user.UserId);
            var recentJobs = await _context.Jobs.OrderByDescending(j => j.CreatedAt).Take(5).ToListAsync();

            return Ok(new
            {
                applicationsCount = appCount,
                recentJobs = recentJobs
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] TokenModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.AuthToken == model.Token);
            if (user != null)
            {
                user.AuthToken = null;
                user.AuthTokenExpiry = null;
                await _context.SaveChangesAsync();
            }
            return Ok(new { success = true });
        }

        public class LoginModel 
        { 
            public string Email { get; set; } = ""; 
            public string Password { get; set; } = ""; 
            public string? FcmToken { get; set; }
        }
        public class TokenModel { public string Token { get; set; } = ""; }
        public class FcmModel { public string Token { get; set; } = ""; public string FcmToken { get; set; } = ""; }
    }
}

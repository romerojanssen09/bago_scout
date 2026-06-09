using Microsoft.AspNetCore.Mvc;
using BagoScout.Data;
using BagoScout.Models;
using BagoScout.Services;
using Microsoft.EntityFrameworkCore;

namespace BagoScout.Controllers
{
    public class EmailController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IHttpClientFactory _httpClientFactory;

        public EmailController(IConfiguration configuration, ApplicationDbContext context, IEmailService emailService, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _context = context;
            _emailService = emailService;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        [Route("api/email/send")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                var senderName = HttpContext.Session.GetString("UserName") ?? "BagoScout User";
                var senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "noreply@bagoscout.com";
                var senderDisplayName = _configuration["EmailSettings:SenderName"] ?? "BagoScout";
                var resendApiKey = _configuration["EmailSettings:ResendApiKey"] ?? "";

                // Save email to database
                var emailMessage = new EmailMessage
                {
                    ApplicationId = request.ApplicationId,
                    SenderId = userId.Value,
                    ReceiverId = request.ReceiverId,
                    Subject = request.Subject,
                    Message = request.Message,
                    SentAt = DateTime.UtcNow,
                    IsRead = false
                };

                _context.EmailMessages.Add(emailMessage);
                await _context.SaveChangesAsync();

                // Send email via Resend HTTP API (SMTP blocked on Railway)
                if (!string.IsNullOrWhiteSpace(resendApiKey))
                {
                    var payload = new
                    {
                        from = $"{senderDisplayName} <{senderEmail}>",
                        to = new[] { request.To },
                        subject = request.Subject,
                        html = $@"
                            <html>
                            <body style='font-family: Arial, sans-serif;'>
                                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                                    <h2 style='color: #1C2B53;'>Message from {senderName}</h2>
                                    <div style='background: #F0F4FF; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                                        <p style='color: #333; line-height: 1.6;'>{request.Message}</p>
                                    </div>
                                    <hr style='border: 1px solid #E5E7EB; margin: 20px 0;'>
                                    <p style='color: #6B7280; font-size: 14px;'>
                                        This email was sent via BagoScout job platform.
                                    </p>
                                </div>
                            </body>
                            </html>"
                    };

                    var json = System.Text.Json.JsonSerializer.Serialize(payload);
                    var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
                    httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", resendApiKey);
                    httpRequest.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    var client = _httpClientFactory.CreateClient("Resend");
                    var response = await client.SendAsync(httpRequest);

                    if (!response.IsSuccessStatusCode)
                    {
                        var err = await response.Content.ReadAsStringAsync();
                        return StatusCode(500, new { message = "Email saved but delivery failed", error = err });
                    }
                }

                return Ok(new { message = "Email sent successfully", emailId = emailMessage.EmailId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error sending email", error = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/email/application/{applicationId}")]
        public async Task<IActionResult> GetEmailsByApplication(int applicationId)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not logged in" });
                }

                var emails = await _context.EmailMessages
                    .Where(e => e.ApplicationId == applicationId && 
                               (e.SenderId == userId.Value || e.ReceiverId == userId.Value))
                    .OrderByDescending(e => e.SentAt)
                    .Select(e => new
                    {
                        e.EmailId,
                        e.ApplicationId,
                        e.SenderId,
                        e.ReceiverId,
                        e.Subject,
                        e.Message,
                        e.SentAt,
                        e.IsRead
                    })
                    .ToListAsync();

                return Ok(emails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching emails", error = ex.Message });
            }
        }

        [HttpPost]
        [Route("api/email/mark-read/{emailId}")]
        public async Task<IActionResult> MarkAsRead(int emailId)
        {
            try
            {
                var email = await _context.EmailMessages.FindAsync(emailId);
                if (email == null)
                {
                    return NotFound(new { message = "Email not found" });
                }

                email.IsRead = true;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Email marked as read" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error marking email as read", error = ex.Message });
            }
        }
    }

    public class EmailRequest
    {
        public int ApplicationId { get; set; }
        public int ReceiverId { get; set; }
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}

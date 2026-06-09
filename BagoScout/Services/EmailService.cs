using BagoScout.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BagoScout.Services
{
    /// <summary>
    /// Sends emails via the Resend HTTP API (https://resend.com).
    /// Uses HTTPS on port 443 — works on Railway where SMTP ports 587/465 are blocked.
    /// 
    /// Required config:
    ///   EmailSettings__SenderName, EmailSettings__SenderEmail (must be verified in Resend)
    ///   EmailSettings__ResendApiKey  (from resend.com dashboard)
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        private const string ResendApiUrl = "https://api.resend.com/emails";

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Core send method — calls Resend REST API over HTTPS (port 443).
        /// </summary>
        private async Task<bool> SendViaResendAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            var apiKey = _emailSettings.ResendApiKey;

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogError("[EmailService] ResendApiKey is not configured. Set EmailSettings__ResendApiKey in Railway variables.");
                return false;
            }

            // Resend does not allow sending from free email providers like Gmail.
            // If the configured sender is a Gmail/Hotmail/Yahoo address, fall back to
            // the Resend shared test domain so emails still go out.
            var senderEmail = _emailSettings.SenderEmail ?? "";
            var freeProviders = new[] { "@gmail.com", "@hotmail.com", "@yahoo.com", "@outlook.com" };
            bool isFreeDomain = freeProviders.Any(p => senderEmail.EndsWith(p, StringComparison.OrdinalIgnoreCase));

            string fromAddress;
            if (isFreeDomain)
            {
                // Use Resend's shared onboarding domain for testing
                fromAddress = $"{_emailSettings.SenderName} <onboarding@resend.dev>";
                _logger.LogWarning($"[EmailService] Sender '{senderEmail}' is a free email provider — Resend does not allow this. Using 'onboarding@resend.dev' instead. To send from your own address, verify a domain at resend.com.");
            }
            else
            {
                fromAddress = $"{_emailSettings.SenderName} <{senderEmail}>";
            }

            var payload = new
            {
                from = fromAddress,
                to = new[] { toEmail },
                subject = subject,
                html = htmlBody
            };

            var json = JsonSerializer.Serialize(payload);
            _logger.LogInformation($"[EmailService] Sending via Resend API: from='{fromAddress}' to='{toEmail}' subject='{subject}'");

            try
            {
                var client = _httpClientFactory.CreateClient("Resend");
                var request = new HttpRequestMessage(HttpMethod.Post, ResendApiUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"[EmailService] Email sent successfully to {toEmail}. Response: {responseBody}");
                    return true;
                }
                else
                {
                    _logger.LogError($"[EmailService] Resend API error {(int)response.StatusCode}: {responseBody}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[EmailService] HTTP error sending to {toEmail}: {ex.GetType().Name}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendVerificationEmailAsync(string toEmail, string toName, string verificationCode)
        {
            var html = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: 'Inter', Arial, sans-serif; background-color: #f5f5f5; margin: 0; padding: 0; }}
                        .container {{ max-width: 600px; margin: 40px auto; background-color: white; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1); }}
                        .header {{ background: linear-gradient(135deg, #1C2B53 0%, #6C63FF 100%); padding: 40px 30px; text-align: center; }}
                        .header h1 {{ color: white; margin: 0; font-size: 28px; }}
                        .content {{ padding: 40px 30px; }}
                        .content h2 {{ color: #1C2B53; font-size: 24px; margin-bottom: 20px; }}
                        .content p {{ color: #6B7280; line-height: 1.6; margin-bottom: 20px; }}
                        .code-box {{ background: #F0F4FF; border: 2px solid #6C63FF; border-radius: 12px; padding: 30px; text-align: center; margin: 30px 0; }}
                        .code {{ font-size: 36px; font-weight: bold; color: #1C2B53; letter-spacing: 8px; }}
                        .footer {{ background: #F9FAFB; padding: 20px 30px; text-align: center; color: #9CA3AF; font-size: 14px; }}
                        .footer a {{ color: #6C63FF; text-decoration: none; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🎉 Welcome to BagoScout!</h1>
                        </div>
                        <div class='content'>
                            <h2>Hi {toName},</h2>
                            <p>Thank you for registering with BagoScout! We're excited to have you join our community of job seekers and employers.</p>
                            <p>To complete your registration, please verify your email address by entering the verification code below:</p>
                            <div class='code-box'>
                                <div class='code'>{verificationCode}</div>
                            </div>
                            <p>This code will expire in 15 minutes for security reasons.</p>
                            <p>If you didn't create an account with BagoScout, please ignore this email.</p>
                        </div>
                        <div class='footer'>
                            <p>© 2024 BagoScout. All rights reserved.</p>
                            <p>Need help? Contact us at <a href='mailto:BagoScoutLife@gmail.com'>BagoScoutLife@gmail.com</a></p>
                        </div>
                    </div>
                </body>
                </html>";

            return await SendViaResendAsync(toEmail, toName, "Verify Your BagoScout Account", html);
        }

        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string toName)
        {
            var html = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: 'Inter', Arial, sans-serif; background-color: #f5f5f5; margin: 0; padding: 0; }}
                        .container {{ max-width: 600px; margin: 40px auto; background-color: white; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1); }}
                        .header {{ background: linear-gradient(135deg, #1C2B53 0%, #6C63FF 100%); padding: 40px 30px; text-align: center; }}
                        .header h1 {{ color: white; margin: 0; font-size: 28px; }}
                        .content {{ padding: 40px 30px; }}
                        .content h2 {{ color: #1C2B53; font-size: 24px; margin-bottom: 20px; }}
                        .content p {{ color: #6B7280; line-height: 1.6; margin-bottom: 20px; }}
                        .button {{ display: inline-block; background: #1C2B53; color: white; padding: 14px 32px; border-radius: 8px; text-decoration: none; font-weight: 600; margin: 20px 0; }}
                        .footer {{ background: #F9FAFB; padding: 20px 30px; text-align: center; color: #9CA3AF; font-size: 14px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>✅ Email Verified Successfully!</h1>
                        </div>
                        <div class='content'>
                            <h2>Welcome aboard, {toName}!</h2>
                            <p>Your email has been verified and your account is now active. You're all set to start your journey with BagoScout!</p>
                            <p>Here's what you can do next:</p>
                            <ul style='color: #6B7280; line-height: 1.8;'>
                                <li>Complete your profile</li>
                                <li>Browse job opportunities</li>
                                <li>Connect with employers</li>
                                <li>Start applying for jobs</li>
                            </ul>
                        </div>
                        <div class='footer'>
                            <p>© 2024 BagoScout. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            return await SendViaResendAsync(toEmail, toName, "Welcome to BagoScout!", html);
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink)
        {
            var html = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: 'Inter', Arial, sans-serif; background-color: #f5f5f5; margin: 0; padding: 0; }}
                        .container {{ max-width: 600px; margin: 40px auto; background-color: white; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1); }}
                        .header {{ background: linear-gradient(135deg, #1C2B53 0%, #6C63FF 100%); padding: 40px 30px; text-align: center; }}
                        .header h1 {{ color: white; margin: 0; font-size: 28px; }}
                        .content {{ padding: 40px 30px; }}
                        .content h2 {{ color: #1C2B53; font-size: 24px; margin-bottom: 20px; }}
                        .content p {{ color: #6B7280; line-height: 1.6; margin-bottom: 20px; }}
                        .button {{ display: inline-block; background: #1C2B53; color: white; padding: 14px 32px; border-radius: 8px; text-decoration: none; font-weight: 600; margin: 20px 0; }}
                        .footer {{ background: #F9FAFB; padding: 20px 30px; text-align: center; color: #9CA3AF; font-size: 14px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🔒 Password Reset Request</h1>
                        </div>
                        <div class='content'>
                            <h2>Hi {toName},</h2>
                            <p>We received a request to reset your password. Click the button below to create a new password:</p>
                            <center>
                                <a href='{resetLink}' class='button'>Reset Password</a>
                            </center>
                            <p>This link will expire in 1 hour for security reasons.</p>
                            <p>If you didn't request a password reset, please ignore this email or contact support if you have concerns.</p>
                        </div>
                        <div class='footer'>
                            <p>© 2024 BagoScout. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            return await SendViaResendAsync(toEmail, toName, "Reset Your BagoScout Password", html);
        }

        public async Task<bool> SendPasswordResetOtpEmailAsync(string toEmail, string toName, string otpCode)
        {
            var html = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: 'Inter', Arial, sans-serif; background-color: #f5f5f5; margin: 0; padding: 0; }}
                        .container {{ max-width: 600px; margin: 40px auto; background-color: white; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1); }}
                        .header {{ background: linear-gradient(135deg, #1C2B53 0%, #6C63FF 100%); padding: 40px 30px; text-align: center; }}
                        .header h1 {{ color: white; margin: 0; font-size: 28px; }}
                        .content {{ padding: 40px 30px; }}
                        .content h2 {{ color: #1C2B53; font-size: 24px; margin-bottom: 20px; }}
                        .content p {{ color: #6B7280; line-height: 1.6; margin-bottom: 20px; }}
                        .code-box {{ background: #F0F4FF; border: 2px solid #6C63FF; border-radius: 12px; padding: 30px; text-align: center; margin: 30px 0; }}
                        .code {{ font-size: 36px; font-weight: bold; color: #1C2B53; letter-spacing: 8px; }}
                        .footer {{ background: #F9FAFB; padding: 20px 30px; text-align: center; color: #9CA3AF; font-size: 14px; }}
                        .footer a {{ color: #6C63FF; text-decoration: none; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🔒 Password Reset Request</h1>
                        </div>
                        <div class='content'>
                            <h2>Hi {toName},</h2>
                            <p>We received a request to reset your password. Use the verification code below to verify your request and set a new password:</p>
                            <div class='code-box'>
                                <div class='code'>{otpCode}</div>
                            </div>
                            <p>This code will expire in 15 minutes for security reasons.</p>
                            <p>If you didn't request a password reset, please ignore this email.</p>
                        </div>
                        <div class='footer'>
                            <p>© 2024 BagoScout. All rights reserved.</p>
                            <p>Need help? Contact us at <a href='mailto:BagoScoutLife@gmail.com'>BagoScoutLife@gmail.com</a></p>
                        </div>
                    </div>
                </body>
                </html>";

            return await SendViaResendAsync(toEmail, toName, "Reset Your BagoScout Password", html);
        }
    }
}

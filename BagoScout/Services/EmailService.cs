using BagoScout.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using GmailMessage = Google.Apis.Gmail.v1.Data.Message;

namespace BagoScout.Services
{
    /// <summary>
    /// Sends emails via the Gmail API using OAuth2 refresh token.
    /// Works on Railway (HTTPS only, no SMTP ports needed).
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _settings = emailSettings.Value;
            _logger = logger;
        }

        private async Task<bool> SendViaGmailApiAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            try
            {
                _logger.LogInformation($"[GmailAPI] Sending to {toEmail}, subject: {subject}");

                // Validate config
                if (string.IsNullOrWhiteSpace(_settings.GmailClientId) ||
                    string.IsNullOrWhiteSpace(_settings.GmailClientSecret) ||
                    string.IsNullOrWhiteSpace(_settings.GmailRefreshToken))
                {
                    _logger.LogError("[GmailAPI] Missing OAuth2 config. Set EmailSettings__GmailClientId, EmailSettings__GmailClientSecret, EmailSettings__GmailRefreshToken in Railway.");
                    return false;
                }

                // Build OAuth2 credential from stored refresh token
                var tokenResponse = new TokenResponse
                {
                    RefreshToken = _settings.GmailRefreshToken
                };

                var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = _settings.GmailClientId,
                        ClientSecret = _settings.GmailClientSecret
                    },
                    Scopes = new[] { GmailService.Scope.GmailSend }
                });

                var credential = new UserCredential(flow, "user", tokenResponse);

                // Build Gmail service
                var gmailService = new GmailService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "BagoScout"
                });

                // Build MIME message
                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
                mimeMessage.To.Add(new MailboxAddress(toName, toEmail));
                mimeMessage.Subject = subject;
                mimeMessage.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

                // Encode as base64url (required by Gmail API)
                using var stream = new MemoryStream();
                await mimeMessage.WriteToAsync(stream);
                var rawMessage = Convert.ToBase64String(stream.ToArray())
                    .Replace('+', '-').Replace('/', '_').Replace("=", "");

                var gmailMessage = new GmailMessage { Raw = rawMessage };
                await gmailService.Users.Messages.Send(gmailMessage, "me").ExecuteAsync();

                _logger.LogInformation($"[GmailAPI] Email sent successfully to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[GmailAPI] Failed to send to {toEmail}: {ex.GetType().Name}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendVerificationEmailAsync(string toEmail, string toName, string verificationCode)
        {
            var html = $@"
                <!DOCTYPE html><html><head><style>
                    body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; margin: 0; padding: 0; }}
                    .container {{ max-width: 600px; margin: 40px auto; background: white; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1); }}
                    .header {{ background: linear-gradient(135deg, #1C2B53 0%, #6C63FF 100%); padding: 40px 30px; text-align: center; }}
                    .header h1 {{ color: white; margin: 0; font-size: 28px; }}
                    .content {{ padding: 40px 30px; }}
                    .content h2 {{ color: #1C2B53; font-size: 24px; margin-bottom: 20px; }}
                    .content p {{ color: #6B7280; line-height: 1.6; margin-bottom: 20px; }}
                    .code-box {{ background: #F0F4FF; border: 2px solid #6C63FF; border-radius: 12px; padding: 30px; text-align: center; margin: 30px 0; }}
                    .code {{ font-size: 36px; font-weight: bold; color: #1C2B53; letter-spacing: 8px; }}
                    .footer {{ background: #F9FAFB; padding: 20px 30px; text-align: center; color: #9CA3AF; font-size: 14px; }}
                    .footer a {{ color: #6C63FF; text-decoration: none; }}
                </style></head><body>
                <div class='container'>
                    <div class='header'><h1>🎉 Welcome to BagoScout!</h1></div>
                    <div class='content'>
                        <h2>Hi {toName},</h2>
                        <p>Thank you for registering with BagoScout! Please verify your email by entering the code below:</p>
                        <div class='code-box'><div class='code'>{verificationCode}</div></div>
                        <p>This code will expire in 15 minutes.</p>
                        <p>If you didn't create an account, please ignore this email.</p>
                    </div>
                    <div class='footer'>
                        <p>© 2024 BagoScout. All rights reserved.</p>
                        <p>Need help? <a href='mailto:BagoScoutLife@gmail.com'>Contact us</a></p>
                    </div>
                </div></body></html>";

            return await SendViaGmailApiAsync(toEmail, toName, "Verify Your BagoScout Account", html);
        }

        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string toName)
        {
            var html = $@"
                <!DOCTYPE html><html><head><style>
                    body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; margin: 0; padding: 0; }}
                    .container {{ max-width: 600px; margin: 40px auto; background: white; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1); }}
                    .header {{ background: linear-gradient(135deg, #1C2B53 0%, #6C63FF 100%); padding: 40px 30px; text-align: center; }}
                    .header h1 {{ color: white; margin: 0; font-size: 28px; }}
                    .content {{ padding: 40px 30px; }}
                    .content h2 {{ color: #1C2B53; font-size: 24px; margin-bottom: 20px; }}
                    .content p {{ color: #6B7280; line-height: 1.6; margin-bottom: 20px; }}
                    .footer {{ background: #F9FAFB; padding: 20px 30px; text-align: center; color: #9CA3AF; font-size: 14px; }}
                </style></head><body>
                <div class='container'>
                    <div class='header'><h1>✅ Email Verified Successfully!</h1></div>
                    <div class='content'>
                        <h2>Welcome aboard, {toName}!</h2>
                        <p>Your email has been verified and your account is now active.</p>
                        <p>Here's what you can do next:</p>
                        <ul style='color: #6B7280; line-height: 1.8;'>
                            <li>Complete your profile</li>
                            <li>Browse job opportunities</li>
                            <li>Connect with employers</li>
                            <li>Start applying for jobs</li>
                        </ul>
                    </div>
                    <div class='footer'><p>© 2024 BagoScout. All rights reserved.</p></div>
                </div></body></html>";

            return await SendViaGmailApiAsync(toEmail, toName, "Welcome to BagoScout!", html);
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink)
        {
            var html = $@"
                <!DOCTYPE html><html><head><style>
                    body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; margin: 0; padding: 0; }}
                    .container {{ max-width: 600px; margin: 40px auto; background: white; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1); }}
                    .header {{ background: linear-gradient(135deg, #1C2B53 0%, #6C63FF 100%); padding: 40px 30px; text-align: center; }}
                    .header h1 {{ color: white; margin: 0; font-size: 28px; }}
                    .content {{ padding: 40px 30px; }}
                    .content h2 {{ color: #1C2B53; font-size: 24px; margin-bottom: 20px; }}
                    .content p {{ color: #6B7280; line-height: 1.6; margin-bottom: 20px; }}
                    .button {{ display: inline-block; background: #1C2B53; color: white; padding: 14px 32px; border-radius: 8px; text-decoration: none; font-weight: 600; }}
                    .footer {{ background: #F9FAFB; padding: 20px 30px; text-align: center; color: #9CA3AF; font-size: 14px; }}
                </style></head><body>
                <div class='container'>
                    <div class='header'><h1>🔒 Password Reset Request</h1></div>
                    <div class='content'>
                        <h2>Hi {toName},</h2>
                        <p>We received a request to reset your password. Click the button below:</p>
                        <center><a href='{resetLink}' class='button'>Reset Password</a></center>
                        <p>This link will expire in 1 hour.</p>
                        <p>If you didn't request this, please ignore this email.</p>
                    </div>
                    <div class='footer'><p>© 2024 BagoScout. All rights reserved.</p></div>
                </div></body></html>";

            return await SendViaGmailApiAsync(toEmail, toName, "Reset Your BagoScout Password", html);
        }

        public async Task<bool> SendPasswordResetOtpEmailAsync(string toEmail, string toName, string otpCode)
        {
            var html = $@"
                <!DOCTYPE html><html><head><style>
                    body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; margin: 0; padding: 0; }}
                    .container {{ max-width: 600px; margin: 40px auto; background: white; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1); }}
                    .header {{ background: linear-gradient(135deg, #1C2B53 0%, #6C63FF 100%); padding: 40px 30px; text-align: center; }}
                    .header h1 {{ color: white; margin: 0; font-size: 28px; }}
                    .content {{ padding: 40px 30px; }}
                    .content h2 {{ color: #1C2B53; font-size: 24px; margin-bottom: 20px; }}
                    .content p {{ color: #6B7280; line-height: 1.6; margin-bottom: 20px; }}
                    .code-box {{ background: #F0F4FF; border: 2px solid #6C63FF; border-radius: 12px; padding: 30px; text-align: center; margin: 30px 0; }}
                    .code {{ font-size: 36px; font-weight: bold; color: #1C2B53; letter-spacing: 8px; }}
                    .footer {{ background: #F9FAFB; padding: 20px 30px; text-align: center; color: #9CA3AF; font-size: 14px; }}
                    .footer a {{ color: #6C63FF; text-decoration: none; }}
                </style></head><body>
                <div class='container'>
                    <div class='header'><h1>🔒 Password Reset Request</h1></div>
                    <div class='content'>
                        <h2>Hi {toName},</h2>
                        <p>Use the verification code below to reset your password:</p>
                        <div class='code-box'><div class='code'>{otpCode}</div></div>
                        <p>This code will expire in 15 minutes.</p>
                        <p>If you didn't request this, please ignore this email.</p>
                    </div>
                    <div class='footer'>
                        <p>© 2024 BagoScout. All rights reserved.</p>
                        <p>Need help? <a href='mailto:BagoScoutLife@gmail.com'>Contact us</a></p>
                    </div>
                </div></body></html>";

            return await SendViaGmailApiAsync(toEmail, toName, "Reset Your BagoScout Password", html);
        }
    }
}

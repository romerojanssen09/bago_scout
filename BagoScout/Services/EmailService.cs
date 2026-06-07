using BagoScout.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BagoScout.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendVerificationEmailAsync(string toEmail, string toName, string verificationCode)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = "Verify Your BagoScout Account";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
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
                        </html>
                    "
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Verification email sent successfully to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending verification email to {toEmail}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string toName)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = "Welcome to BagoScout!";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
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
                                    <center>
                                        <a href='http://localhost:5180' class='button'>Get Started</a>
                                    </center>
                                </div>
                                <div class='footer'>
                                    <p>© 2024 BagoScout. All rights reserved.</p>
                                </div>
                            </div>
                        </body>
                        </html>
                    "
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Welcome email sent successfully to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending welcome email to {toEmail}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = "Reset Your BagoScout Password";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
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
                        </html>
                    "
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Password reset email sent successfully to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending password reset email to {toEmail}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendPasswordResetOtpEmailAsync(string toEmail, string toName, string otpCode)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = "Reset Your BagoScout Password";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
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
                        </html>
                    "
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Password reset OTP email sent successfully to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending password reset OTP email to {toEmail}: {ex.Message}");
                return false;
            }
        }
    }
}

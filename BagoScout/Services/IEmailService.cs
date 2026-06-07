namespace BagoScout.Services
{
    public interface IEmailService
    {
        Task<bool> SendVerificationEmailAsync(string toEmail, string toName, string verificationCode);
        Task<bool> SendWelcomeEmailAsync(string toEmail, string toName);
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink);
        Task<bool> SendPasswordResetOtpEmailAsync(string toEmail, string toName, string otpCode);
    }
}

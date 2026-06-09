namespace BagoScout.Models
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// API key from resend.com — used instead of SMTP so email works on Railway.
        /// Set via Railway env var: EmailSettings__ResendApiKey=re_xxxxxxxxxxxx
        /// </summary>
        public string ResendApiKey { get; set; } = string.Empty;
    }
}

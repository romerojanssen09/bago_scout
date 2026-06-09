namespace BagoScout.Models
{
    public class EmailSettings
    {
        public string SenderName { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;

        // Gmail OAuth2 credentials
        public string GmailClientId { get; set; } = string.Empty;
        public string GmailClientSecret { get; set; } = string.Empty;
        public string GmailRefreshToken { get; set; } = string.Empty;
    }
}

using System.Net;
using System.Net.Mail;

namespace TooltipAI.Backend.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _logger = logger;
        _smtpHost = configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
        _smtpUsername = configuration["Email:SmtpUsername"] ?? "";
        _smtpPassword = configuration["Email:SmtpPassword"] ?? "";
        _fromEmail = configuration["Email:FromEmail"] ?? "noreply@tooltipai.com";
        _fromName = configuration["Email:FromName"] ?? "Tooltip AI";
    }

    public async Task SendLicenseEmail(string email, string licenseKey, string tier, DateTime expiresAt)
    {
        try
        {
            var subject = $"Tooltip AI License - {tier.ToUpper()}";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; background-color: #1a1a2e; color: #ffffff; padding: 20px;'>
                    <div style='max-width: 600px; margin: 0 auto; background-color: #16213e; border-radius: 10px; padding: 30px; border: 1px solid #00d4ff;'>
                        <h1 style='color: #00d4ff; text-align: center;'>Tooltip AI License</h1>
                        <p>Thank you for purchasing Tooltip AI {tier.ToUpper()}!</p>
                        <p>Your license key:</p>
                        <div style='background-color: #0a0a12; padding: 15px; border-radius: 5px; border: 1px solid #00d4ff; font-family: monospace; font-size: 18px; text-align: center; margin: 20px 0;'>
                            {licenseKey}
                        </div>
                        <p><strong>Tier:</strong> {tier.ToUpper()}</p>
                        <p><strong>Expires:</strong> {expiresAt:MMMM dd, yyyy}</p>
                        <p>Enter this key in the Tooltip AI settings to activate your license.</p>
                        <hr style='border-color: #00d4ff; margin: 20px 0;'>
                        <p style='color: #888; font-size: 12px;'>This email was sent by Tooltip AI. Do not share this license key.</p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
            _logger.LogInformation("License email sent to {Email} for tier {Tier}", email, tier);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send license email to {Email}", email);
        }
    }

    public async Task SendSubscriptionEmail(string email, string licenseKey, string tier, DateTime expiresAt)
    {
        try
        {
            var subject = $"Tooltip AI Subscription - {tier.ToUpper()}";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; background-color: #1a1a2e; color: #ffffff; padding: 20px;'>
                    <div style='max-width: 600px; margin: 0 auto; background-color: #16213e; border-radius: 10px; padding: 30px; border: 1px solid #00d4ff;'>
                        <h1 style='color: #00d4ff; text-align: center;'>Tooltip AI Subscription</h1>
                        <p>Your subscription is now active!</p>
                        <p>Your license key:</p>
                        <div style='background-color: #0a0a12; padding: 15px; border-radius: 5px; border: 1px solid #00d4ff; font-family: monospace; font-size: 18px; text-align: center; margin: 20px 0;'>
                            {licenseKey}
                        </div>
                        <p><strong>Tier:</strong> {tier.ToUpper()}</p>
                        <p><strong>Renews:</strong> {expiresAt:MMMM dd, yyyy}</p>
                        <p>Your subscription will auto-renew. You can cancel anytime in the settings.</p>
                        <hr style='border-color: #00d4ff; margin: 20px 0;'>
                        <p style='color: #888; font-size: 12px;'>This email was sent by Tooltip AI.</p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
            _logger.LogInformation("Subscription email sent to {Email} for tier {Tier}", email, tier);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send subscription email to {Email}", email);
        }
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
        {
            _logger.LogWarning("SMTP credentials not configured - email not sent to {ToEmail}", toEmail);
            return;
        }

        using var message = new MailMessage();
        message.From = new MailAddress(_fromEmail, _fromName);
        message.To.Add(toEmail);
        message.Subject = subject;
        message.Body = htmlBody;
        message.IsBodyHtml = true;

        using var client = new SmtpClient(_smtpHost, _smtpPort)
        {
            Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
            EnableSsl = true,
            Timeout = 30000
        };

        await client.SendMailAsync(message);
    }
}
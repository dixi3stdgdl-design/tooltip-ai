namespace TooltipAI.Backend.Services;

public interface IEmailService
{
    Task SendLicenseEmail(string email, string licenseKey, string tier, DateTime expiresAt);
    Task SendSubscriptionEmail(string email, string licenseKey, string tier, DateTime expiresAt);
}
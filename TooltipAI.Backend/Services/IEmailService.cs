namespace TooltipAI.Backend.Services;

/// <summary>
/// Interface for sending email notifications to users.
/// Handles license delivery and subscription updates.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a license email to the specified user.
    /// </summary>
    /// <param name="email">Recipient email address.</param>
    /// <param name="licenseKey">The license key to include.</param>
    /// <param name="tier">The license tier (e.g., "basic", "pro", "enterprise").</param>
    /// <param name="expiresAt">License expiration date.</param>
    /// <returns>A task representing the async operation.</returns>
    Task SendLicenseEmail(string email, string licenseKey, string tier, DateTime expiresAt);

    /// <summary>
    /// Sends a subscription confirmation email to the specified user.
    /// </summary>
    /// <param name="email">Recipient email address.</param>
    /// <param name="licenseKey">The subscription license key.</param>
    /// <param name="tier">The subscription tier.</param>
    /// <param name="expiresAt">Subscription expiration date.</param>
    /// <returns>A task representing the async operation.</returns>
    Task SendSubscriptionEmail(string email, string licenseKey, string tier, DateTime expiresAt);
}
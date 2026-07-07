using System.Security.Cryptography;
using System.Text;
using TooltipAI.Backend.Models;

namespace TooltipAI.Backend.Services;

public class PaymentService
{
    private readonly LicenseService _licenseService;
    private readonly IEmailService _emailService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(LicenseService licenseService, IEmailService emailService, ILogger<PaymentService> logger)
    {
        _licenseService = licenseService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task GenerateLicenseForOrder(string orderId, string email, string tier)
    {
        try
        {
            _logger.LogInformation("Generating license for order {OrderId}, email {Email}, tier {Tier}", 
                orderId, email, tier);

            // Generate license key
            var licenseKey = GenerateLicenseKey();
            
            // Calculate expiry based on tier
            var expiryDate = tier switch
            {
                "pro" => DateTime.UtcNow.AddMonths(1),
                "business" => DateTime.UtcNow.AddMonths(1),
                "enterprise" => DateTime.UtcNow.AddYears(1),
                _ => DateTime.UtcNow.AddMonths(1)
            };

            // Create license in database
            await _licenseService.CreateLicenseAsync(new LicenseInfo
            {
                LicenseId = orderId,
                LicenseKey = licenseKey,
                Tier = tier,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiryDate,
                IsActive = true
            });

            _logger.LogInformation("License generated: {LicenseKey} for {Email}", licenseKey, email);

            // Send email with license key
            await _emailService.SendLicenseEmail(email, licenseKey, tier, expiryDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate license for order {OrderId}", orderId);
            throw;
        }
    }

    public async Task GenerateLicenseForSubscription(string subscriptionId, string email, string tier)
    {
        try
        {
            _logger.LogInformation("Generating license for subscription {SubscriptionId}, email {Email}, tier {Tier}", 
                subscriptionId, email, tier);

            // Generate license key
            var licenseKey = GenerateLicenseKey();
            
            // Subscriptions expire after 1 month
            var expiryDate = DateTime.UtcNow.AddMonths(1);

            // Create license in database
            await _licenseService.CreateLicenseAsync(new LicenseInfo
            {
                LicenseId = $"sub_{subscriptionId}",
                LicenseKey = licenseKey,
                Tier = tier,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiryDate,
                IsActive = true
            });

            _logger.LogInformation("Subscription license generated: {LicenseKey} for {Email}", licenseKey, email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate license for subscription {SubscriptionId}", subscriptionId);
            throw;
        }
    }

    public async Task ActivateSubscription(string subscriptionId)
    {
        try
        {
            _logger.LogInformation("Activating subscription: {SubscriptionId}", subscriptionId);
            await _licenseService.ActivateLicenseAsync($"sub_{subscriptionId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate subscription {SubscriptionId}", subscriptionId);
            throw;
        }
    }

    public async Task DeactivateSubscription(string subscriptionId)
    {
        try
        {
            _logger.LogInformation("Deactivating subscription: {SubscriptionId}", subscriptionId);
            await _licenseService.DeactivateLicenseAsync($"sub_{subscriptionId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deactivate subscription {SubscriptionId}", subscriptionId);
            throw;
        }
    }

    public async Task DeactivateSubscriptionAtPeriodEnd(string subscriptionId, string? endsAt)
    {
        try
        {
            _logger.LogInformation("Scheduling subscription deactivation: {SubscriptionId}, ends at: {EndsAt}", 
                subscriptionId, endsAt);

            if (DateTime.TryParse(endsAt, out var endDate))
            {
                // Extend license until end date, then it will naturally expire
                await _licenseService.ExtendLicenseUntilAsync($"sub_{subscriptionId}", endDate);
            }
            else
            {
                // Default: deactivate immediately if we can't parse the end date
                await _licenseService.DeactivateLicenseAsync($"sub_{subscriptionId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule deactivation for subscription {SubscriptionId}", subscriptionId);
            throw;
        }
    }

    public async Task MarkSubscriptionPastDue(string subscriptionId)
    {
        try
        {
            _logger.LogInformation("Marking subscription as past due: {SubscriptionId}", subscriptionId);
            // Don't deactivate immediately - give user time to update payment
            // License remains active but flagged for review
            await _licenseService.MarkLicensePastDueAsync($"sub_{subscriptionId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark subscription past due {SubscriptionId}", subscriptionId);
            throw;
        }
    }

    public async Task RevokeLicense(string licenseKey)
    {
        try
        {
            _logger.LogInformation("Revoking license: {LicenseKey}", licenseKey);
            await _licenseService.RevokeLicenseAsync(licenseKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke license {LicenseKey}", licenseKey);
            throw;
        }
    }

    public async Task RevokeLicenseForOrder(string orderId)
    {
        try
        {
            _logger.LogInformation("Revoking license for order: {OrderId}", orderId);
            await _licenseService.RevokeLicenseByOrderIdAsync(orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke license for order {OrderId}", orderId);
            throw;
        }
    }

    public async Task ExtendLicense(string licenseKey, int days)
    {
        try
        {
            _logger.LogInformation("Extending license {LicenseKey} by {Days} days", licenseKey, days);
            await _licenseService.ExtendLicenseAsync(licenseKey, days);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extend license {LicenseKey}", licenseKey);
            throw;
        }
    }

    private string GenerateLicenseKey()
    {
        var random = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(random);
        
        var key = Convert.ToBase64String(random)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "")
            .Substring(0, 32);
        
        // Format as XXXX-XXXX-XXXX-XXXX-XXXX
        return string.Join("-", Enumerable.Range(0, 5)
            .Select(i => key.Substring(i * 6, 6)));
    }
}

using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TooltipAI.Backend.Services;

namespace TooltipAI.Backend.Controllers;

[ApiController]
[Route("api/webhooks")]
public class WebhookController : ControllerBase
{
    private readonly PaymentService _paymentService;
    private readonly ILogger<WebhookController> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly string _webhookSecret;

    private static readonly HashSet<string> PlaceholderSecrets = new(StringComparer.OrdinalIgnoreCase)
    {
        "CHANGE_ME",
        "CHANGE_ME_TO_YOUR_WEBHOOK_SECRET",
    };

    public WebhookController(
        PaymentService paymentService,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger<WebhookController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
        _environment = environment;
        var secret = configuration["LemonSqueezy:WebhookSecret"] ?? string.Empty;
        _webhookSecret = PlaceholderSecrets.Contains(secret) ? string.Empty : secret;
    }

    /// <summary>
    /// Handle LemonSqueezy webhooks
    /// </summary>
    [HttpPost("lemonsqueezy")]
    public async Task<IActionResult> HandleLemonSqueezyWebhook()
    {
        try
        {
            // Read request body
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            _logger.LogDebug("Webhook received, length: {Length}", body.Length);

            // Verify webhook signature (LemonSqueezy v2)
            if (!VerifyWebhookSignature(body))
            {
                _logger.LogWarning("Invalid webhook signature from IP: {IP}", HttpContext.Connection.RemoteIpAddress);
                return Unauthorized(new { error = "Invalid signature" });
            }

            // Parse event
            using var doc = JsonDocument.Parse(body);
            
            if (!doc.RootElement.TryGetProperty("meta", out var meta))
            {
                _logger.LogWarning("Webhook missing 'meta' property");
                return BadRequest(new { error = "Invalid webhook payload" });
            }

            var eventName = meta.GetProperty("event_name").GetString();
            var webhookId = meta.TryGetProperty("webhook_id", out var wid) ? wid.GetString() : "unknown";

            _logger.LogInformation("Processing LemonSqueezy webhook: {EventName}, ID: {WebhookId}", 
                eventName, webhookId);

            // Handle event
            switch (eventName)
            {
                case "order_created":
                    await HandleOrderCreated(doc.RootElement);
                    break;
                case "order_updated":
                    await HandleOrderUpdated(doc.RootElement);
                    break;
                case "order_expired":
                    await HandleOrderExpired(doc.RootElement);
                    break;
                case "subscription_created":
                    await HandleSubscriptionCreated(doc.RootElement);
                    break;
                case "subscription_updated":
                    await HandleSubscriptionUpdated(doc.RootElement);
                    break;
                case "subscription_cancelled":
                    await HandleSubscriptionCancelled(doc.RootElement);
                    break;
                case "subscription_resumed":
                    await HandleSubscriptionResumed(doc.RootElement);
                    break;
                case "subscription_expired":
                    await HandleSubscriptionExpired(doc.RootElement);
                    break;
                default:
                    _logger.LogInformation("Unhandled webhook event: {EventName}", eventName);
                    break;
            }

            return Ok(new { received = true, event_name = eventName });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON in webhook payload");
            return BadRequest(new { error = "Invalid JSON" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Test webhook endpoint (development only)
    /// </summary>
    [HttpPost("test")]
    public IActionResult TestWebhook([FromBody] object payload)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        _logger.LogInformation("Test webhook received: {Payload}", JsonSerializer.Serialize(payload));
        return Ok(new { status = "received", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Health check for webhook endpoint
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }

    private bool VerifyWebhookSignature(string body)
    {
        if (string.IsNullOrEmpty(_webhookSecret))
        {
            if (_environment.IsDevelopment())
            {
                _logger.LogWarning("Webhook secret not configured - allowing in development mode");
                return true;
            }

            _logger.LogError("Webhook secret not configured - rejecting webhook");
            return false; // Fail closed outside development
        }

        // LemonSqueezy v2 uses X-Signature header with HMAC-SHA256
        var signature = Request.Headers["X-Signature"].FirstOrDefault();
        if (string.IsNullOrEmpty(signature))
        {
            _logger.LogWarning("Missing X-Signature header");
            return false;
        }

        try
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_webhookSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
            var computedSignature = Convert.ToHexString(hash).ToLowerInvariant();

            // Constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(signature),
                Encoding.UTF8.GetBytes(computedSignature));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying webhook signature");
            return false;
        }
    }

    private async Task HandleOrderCreated(JsonElement data)
    {
        var attributes = data.GetProperty("data").GetProperty("attributes");
        
        var orderId = data.GetProperty("data").GetProperty("id").GetString();
        var email = attributes.GetProperty("user_email").GetString();
        var variantId = attributes.GetProperty("variant_id").GetString();
        var status = attributes.TryGetProperty("status", out var statusProp) ? statusProp.GetString() : "completed";
        var total = attributes.TryGetProperty("total", out var totalProp) ? totalProp.GetInt32() : 0;

        _logger.LogInformation("Order created: {OrderId}, Email: {Email}, Variant: {VariantId}, Total: {Total}¢", 
            orderId, email, variantId, total);

        // Only process completed orders
        if (status == "completed")
        {
            var tier = GetTierFromVariantId(variantId);
            await _paymentService.GenerateLicenseForOrder(orderId ?? "", email ?? "", tier);
        }
    }

    private async Task HandleOrderUpdated(JsonElement data)
    {
        var attributes = data.GetProperty("data").GetProperty("attributes");
        
        var orderId = data.GetProperty("data").GetProperty("id").GetString();
        var status = attributes.TryGetProperty("status", out var statusProp) ? statusProp.GetString() : "unknown";

        _logger.LogInformation("Order updated: {OrderId}, Status: {Status}", orderId, status);

        // Handle refunded orders
        if (status == "refunded")
        {
            _logger.LogInformation("Order refunded: {OrderId}", orderId);
            await _paymentService.RevokeLicenseForOrder(orderId ?? "");
        }

        await Task.CompletedTask;
    }

    private async Task HandleOrderExpired(JsonElement data)
    {
        var orderId = data.GetProperty("data").GetProperty("id").GetString();
        _logger.LogInformation("Order expired: {OrderId}", orderId);
        await Task.CompletedTask;
    }

    private async Task HandleSubscriptionCreated(JsonElement data)
    {
        var attributes = data.GetProperty("data").GetProperty("attributes");
        
        var subscriptionId = data.GetProperty("data").GetProperty("id").GetString();
        var email = attributes.GetProperty("user_email").GetString();
        var variantId = attributes.GetProperty("variant_id").GetString();
        var status = attributes.TryGetProperty("status", out var statusProp) ? statusProp.GetString() : "active";

        _logger.LogInformation("Subscription created: {SubscriptionId}, Email: {Email}, Status: {Status}", 
            subscriptionId, email, status);

        if (status == "active")
        {
            var tier = GetTierFromVariantId(variantId);
            await _paymentService.GenerateLicenseForSubscription(subscriptionId ?? "", email ?? "", tier);
        }

        await Task.CompletedTask;
    }

    private async Task HandleSubscriptionUpdated(JsonElement data)
    {
        var attributes = data.GetProperty("data").GetProperty("attributes");
        
        var subscriptionId = data.GetProperty("data").GetProperty("id").GetString();
        var status = attributes.TryGetProperty("status", out var statusProp) ? statusProp.GetString() : "unknown";
        var renewsAt = attributes.TryGetProperty("renews_at", out var renewsProp) ? renewsProp.GetString() : null;

        _logger.LogInformation("Subscription updated: {SubscriptionId}, Status: {Status}, Renews: {Renews}", 
            subscriptionId, status, renewsAt);

        // Handle status changes
        switch (status)
        {
            case "active":
                await _paymentService.ActivateSubscription(subscriptionId ?? "");
                break;
            case "past_due":
                await _paymentService.MarkSubscriptionPastDue(subscriptionId ?? "");
                break;
            case "cancelled":
                // Will be handled by subscription_cancelled
                break;
        }

        await Task.CompletedTask;
    }

    private async Task HandleSubscriptionCancelled(JsonElement data)
    {
        var attributes = data.GetProperty("data").GetProperty("attributes");
        
        var subscriptionId = data.GetProperty("data").GetProperty("id").GetString();
        var endsAt = attributes.TryGetProperty("ends_at", out var endsProp) ? endsProp.GetString() : null;

        _logger.LogInformation("Subscription cancelled: {SubscriptionId}, Ends: {Ends}", subscriptionId, endsAt);

        // Deactivate at period end
        await _paymentService.DeactivateSubscriptionAtPeriodEnd(subscriptionId ?? "", endsAt);

        await Task.CompletedTask;
    }

    private async Task HandleSubscriptionResumed(JsonElement data)
    {
        var subscriptionId = data.GetProperty("data").GetProperty("id").GetString();
        _logger.LogInformation("Subscription resumed: {SubscriptionId}", subscriptionId);
        
        await _paymentService.ActivateSubscription(subscriptionId ?? "");
    }

    private async Task HandleSubscriptionExpired(JsonElement data)
    {
        var subscriptionId = data.GetProperty("data").GetProperty("id").GetString();
        _logger.LogInformation("Subscription expired: {SubscriptionId}", subscriptionId);
        
        await _paymentService.DeactivateSubscription(subscriptionId ?? "");
    }

    private string GetTierFromVariantId(string? variantId)
    {
        if (string.IsNullOrEmpty(variantId))
            return "pro";

        // Map LemonSqueezy variant IDs to tiers
        // Replace these with your actual variant IDs from LemonSqueezy dashboard
        return variantId.ToLowerInvariant() switch
        {
            var id when id.Contains("pro") || id.Contains("499") => "pro",
            var id when id.Contains("business") || id.Contains("1499") => "business",
            var id when id.Contains("enterprise") => "enterprise",
            _ => "pro"
        };
    }
}

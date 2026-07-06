using Microsoft.AspNetCore.Mvc;
using TooltipAI.Backend.Models;

namespace TooltipAI.Backend.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly ILogger<AdminController> _logger;

    public AdminController(ILogger<AdminController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Provision a tenant with licenses
    /// </summary>
    [HttpPost("provision")]
    public IActionResult Provision([FromBody] TenantProvisionRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _logger.LogInformation("Provisioning tenant: {TenantId}", request.TenantId);

        var response = new TenantProvisionResponse
        {
            TenantId = request.TenantId,
            LicensesCreated = request.SeatsRequested,
            ProvisionedAt = DateTime.UtcNow
        };

        return Ok(response);
    }

    /// <summary>
    /// List users for a tenant
    /// </summary>
    [HttpGet("users")]
    public IActionResult GetUsers([FromQuery] string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
            return BadRequest("TenantId is required");

        var users = new List<UserInfo>
        {
            new() { UserId = "user-1", Email = "admin@example.com", Role = "Admin", LastActive = DateTime.UtcNow },
            new() { UserId = "user-2", Email = "user@example.com", Role = "User", LastActive = DateTime.UtcNow.AddHours(-2) }
        };

        return Ok(users);
    }

    /// <summary>
    /// Update tenant policies (blacklist, features)
    /// </summary>
    [HttpPut("policies")]
    public IActionResult UpdatePolicies([FromBody] PolicyUpdateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _logger.LogInformation("Updating policies for tenant: {TenantId}", request.TenantId);

        return Ok(new { request.TenantId, UpdatedAt = DateTime.UtcNow });
    }

    /// <summary>
    /// Get usage metrics for a tenant
    /// </summary>
    [HttpGet("metrics")]
    public IActionResult GetMetrics([FromQuery] string tenantId, [FromQuery] string? period = "30d")
    {
        if (string.IsNullOrEmpty(tenantId))
            return BadRequest("TenantId is required");

        var metrics = new TenantMetrics
        {
            TenantId = tenantId,
            Period = period ?? "30d",
            ActiveUsers = 45,
            TotalTooltipsShown = 12500,
            AverageTooltipsPerUser = 278,
            EnrichmentUsageRate = 0.65,
            Retention7Day = 0.82,
            Retention30Day = 0.68
        };

        return Ok(metrics);
    }

    /// <summary>
    /// Configure gradual rollout
    /// </summary>
    [HttpPost("rollout")]
    public IActionResult ConfigureRollout([FromBody] RolloutRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _logger.LogInformation("Configuring rollout for tenant: {TenantId}, percentage: {Percentage}%", 
            request.TenantId, request.Percentage);

        return Ok(new { request.TenantId, request.Percentage, ConfiguredAt = DateTime.UtcNow });
    }
}

// Request/Response models
public sealed class TenantProvisionRequest
{
    public string TenantId { get; init; } = string.Empty;
    public int SeatsRequested { get; init; }
    public string Plan { get; init; } = "business";
}

public sealed class TenantProvisionResponse
{
    public string TenantId { get; init; } = string.Empty;
    public int LicensesCreated { get; init; }
    public DateTime ProvisionedAt { get; init; }
}

public sealed class UserInfo
{
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public DateTime LastActive { get; init; }
}

public sealed class PolicyUpdateRequest
{
    public string TenantId { get; init; } = string.Empty;
    public List<string>? AppBlacklist { get; init; }
    public bool AIEnrichmentEnabled { get; init; } = true;
    public bool TelemetryEnabled { get; init; } = false;
}

public sealed class TenantMetrics
{
    public string TenantId { get; init; } = string.Empty;
    public string Period { get; init; } = string.Empty;
    public int ActiveUsers { get; init; }
    public long TotalTooltipsShown { get; init; }
    public double AverageTooltipsPerUser { get; init; }
    public double EnrichmentUsageRate { get; init; }
    public double Retention7Day { get; init; }
    public double Retention30Day { get; init; }
}

public sealed class RolloutRequest
{
    public string TenantId { get; init; } = string.Empty;
    [System.ComponentModel.DataAnnotations.Range(0, 100)]
    public int Percentage { get; init; }
    public string? FeatureFlag { get; init; }
}

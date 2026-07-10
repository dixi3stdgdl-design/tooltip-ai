using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TooltipAI.Backend.Models;
using TooltipAI.Backend.Services;

namespace TooltipAI.Backend.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly ILogger<AdminController> _logger;
    private readonly TelemetryAggregator _telemetry;
    private readonly PluginRegistryService _pluginRegistry;
    private readonly UserStoreService _userStore;

    public AdminController(
        ILogger<AdminController> logger,
        TelemetryAggregator telemetry,
        PluginRegistryService pluginRegistry,
        UserStoreService userStore)
    {
        _logger = logger;
        _telemetry = telemetry;
        _pluginRegistry = pluginRegistry;
        _userStore = userStore;
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

        _logger.LogInformation("Listing users for tenant: {TenantId}", tenantId);

        var users = _userStore.GetAllUsers(tenantId);

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

        _logger.LogInformation("Getting metrics for tenant: {TenantId}, period: {Period}", tenantId, period);

        var tenantMetrics = _telemetry.GetTenantMetrics(tenantId, period ?? "30d");

        return Ok(tenantMetrics);
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

    /// <summary>
    /// Get active user count across all tenants
    /// </summary>
    [HttpGet("users/active")]
    public IActionResult GetActiveUserCount([FromQuery] int? sinceMinutes = 60)
    {
        _logger.LogInformation("Getting active user count, since: {SinceMinutes} minutes", sinceMinutes);

        var activeCount = _telemetry.GetActiveUserCount(sinceMinutes ?? 60);

        return Ok(new
        {
            ActiveUsers = activeCount,
            SinceMinutes = sinceMinutes ?? 60,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get system health metrics
    /// </summary>
    [HttpGet("health")]
    public IActionResult GetSystemHealth()
    {
        _logger.LogInformation("Getting system health metrics");

        var process = Process.GetCurrentProcess();
        var health = new SystemHealthMetrics
        {
            Status = "healthy",
            Version = "1.0.0",
            UptimeSeconds = (long)(DateTime.UtcNow - process.StartTime.ToUniversalTime()).TotalSeconds,
            MemoryUsageMB = Math.Round(process.WorkingSet64 / 1024.0 / 1024.0, 2),
            ThreadCount = process.Threads.Count,
            CPUUsagePercent = Math.Round(process.TotalProcessorTime.TotalMilliseconds /
                (DateTime.UtcNow - process.StartTime.ToUniversalTime()).TotalMilliseconds * 100, 2),
            ActivePlugins = _pluginRegistry.GetActivePluginCount(),
            Timestamp = DateTime.UtcNow
        };

        return Ok(health);
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
    public string TenantId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string LicenseTier { get; init; } = "free";
    public string Role { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
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

public sealed class SystemHealthMetrics
{
    public string Status { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public long UptimeSeconds { get; init; }
    public double MemoryUsageMB { get; init; }
    public int ThreadCount { get; init; }
    public double CPUUsagePercent { get; init; }
    public int ActivePlugins { get; init; }
    public DateTime Timestamp { get; init; }
}

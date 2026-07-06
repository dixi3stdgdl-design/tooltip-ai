using Microsoft.AspNetCore.Mvc;
using TooltipAI.Backend.Services;

namespace TooltipAI.Backend.Controllers;

[ApiController]
[Route("api/telemetry")]
public class TelemetryController : ControllerBase
{
    private readonly TelemetryAggregator _aggregator;
    private readonly ILogger<TelemetryController> _logger;

    public TelemetryController(TelemetryAggregator aggregator, ILogger<TelemetryController> logger)
    {
        _aggregator = aggregator;
        _logger = logger;
    }

    /// <summary>
    /// Submit telemetry event
    /// </summary>
    [HttpPost]
    public IActionResult SubmitEvent([FromBody] TelemetryEvent request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _logger.LogDebug("Telemetry event received: {EventType}", request.EventType);

        _aggregator.TrackEvent(request);

        return Accepted();
    }

    /// <summary>
    /// Get aggregated metrics
    /// </summary>
    [HttpGet("metrics")]
    public IActionResult GetMetrics([FromQuery] string? tenantId = null, [FromQuery] string? period = "7d")
    {
        var metrics = _aggregator.GetMetrics(tenantId, period);
        return Ok(metrics);
    }

    /// <summary>
    /// Health check
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { Status = "healthy", Timestamp = DateTime.UtcNow });
    }
}

public sealed class TelemetryEvent
{
    public string EventType { get; init; } = string.Empty;
    public string? TenantId { get; init; }
    public string? UserId { get; init; }
    public string? AppName { get; init; }
    public string? ControlType { get; init; }
    public Dictionary<string, string>? Properties { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public sealed class TelemetryMetrics
{
    public long TotalEvents { get; init; }
    public int UniqueUsers { get; init; }
    public double AverageTooltipsPerUser { get; init; }
    public double EnrichmentUsageRate { get; init; }
    public double RelevanceRate { get; init; }
    public Dictionary<string, long> EventsByType { get; init; } = new();
}

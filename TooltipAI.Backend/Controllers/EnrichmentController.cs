using Microsoft.AspNetCore.Mvc;
using TooltipAI.Backend.Services;

namespace TooltipAI.Backend.Controllers;

[ApiController]
[Route("api/enrich")]
public class EnrichmentController : ControllerBase
{
    private readonly EnrichmentEngine _engine;
    private readonly ILogger<EnrichmentController> _logger;

    public EnrichmentController(EnrichmentEngine engine, ILogger<EnrichmentController> logger)
    {
        _engine = engine;
        _logger = logger;
    }

    /// <summary>
    /// Enrich UI context with AI-powered description
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Enrich([FromBody] EnrichmentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var startTime = DateTime.UtcNow;

        try
        {
            var result = await _engine.EnrichAsync(request.ControlType, request.AppName, request.Properties);

            var latencyMs = (DateTime.UtcNow - startTime).TotalMilliseconds;

            _logger.LogInformation("Enrichment completed in {Latency}ms for {AppName}/{ControlType}", 
                latencyMs, request.AppName, request.ControlType);

            return Ok(new EnrichmentResponse
            {
                Summary = result.Summary,
                Shortcut = result.Shortcut,
                Tips = result.Tips,
                LatencyMs = latencyMs,
                Source = result.Source
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Enrichment failed for {AppName}/{ControlType}", request.AppName, request.ControlType);
            
            return Ok(new EnrichmentResponse
            {
                Summary = "Unable to generate enriched context",
                Source = "error"
            });
        }
    }

    /// <summary>
    /// Health check for enrichment service
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { Status = "healthy", Timestamp = DateTime.UtcNow });
    }
}

public sealed class EnrichmentRequest
{
    public string ControlType { get; init; } = string.Empty;
    public string AppName { get; init; } = string.Empty;
    public Dictionary<string, string>? Properties { get; init; }
}

public sealed class EnrichmentResponse
{
    public string Summary { get; init; } = string.Empty;
    public string? Shortcut { get; init; }
    public List<string>? Tips { get; init; }
    public double LatencyMs { get; init; }
    public string Source { get; init; } = string.Empty;
}

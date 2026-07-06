using Microsoft.AspNetCore.Mvc;
using TooltipAI.Core.AI;

namespace TooltipAI.Backend.Controllers;

[ApiController]
[Route("api/ai")]
public class AIController : ControllerBase
{
    private readonly AIRouter _aiRouter;
    private readonly ILogger<AIController> _logger;

    public AIController(AIRouter aiRouter, ILogger<AIController> logger)
    {
        _aiRouter = aiRouter;
        _logger = logger;
    }

    /// <summary>
    /// Enrich UI context using AI (local or cloud based on tier)
    /// </summary>
    [HttpPost("enrich")]
    public async Task<IActionResult> EnrichContext([FromBody] EnrichRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var startTime = DateTime.UtcNow;

        var aiRequest = new AIRequest
        {
            ControlType = request.ControlType,
            AppName = request.AppName,
            ElementName = request.ElementName,
            ElementState = request.ElementState,
            Properties = request.Properties ?? new(),
            AvailableActions = request.AvailableActions ?? Array.Empty<string>(),
            UserQuery = request.UserQuery
        };

        var response = await _aiRouter.EnrichContextAsync(aiRequest, request.Tier ?? "free");

        var latencyMs = (DateTime.UtcNow - startTime).TotalMilliseconds;

        _logger.LogInformation("AI enrichment: {App}/{Element} -> {Provider} ({Latency}ms, {Confidence}%)",
            request.AppName, request.ElementName, response.Provider, latencyMs, response.Confidence);

        return Ok(new EnrichResponse
        {
            Summary = response.Summary,
            Shortcut = response.Shortcut,
            Tips = response.Tips,
            Provider = response.Provider,
            LatencyMs = response.LatencyMs,
            Confidence = response.Confidence
        });
    }

    /// <summary>
    /// Get AI provider health status
    /// </summary>
    [HttpGet("health")]
    public async Task<IActionResult> GetHealth()
    {
        var health = await _aiRouter.GetHealthAsync();
        return Ok(health);
    }

    /// <summary>
    /// Get available AI tiers
    /// </summary>
    [HttpGet("tiers")]
    public IActionResult GetTiers()
    {
        return Ok(new
        {
            tiers = new[]
            {
                new { id = "free", name = "Free", provider = "Gemini Nano (local)", cost = "$0" },
                new { id = "pro", name = "Pro", provider = "Cloud LLM", cost = "$0.001/query" },
                new { id = "business", name = "Business", provider = "Cloud LLM Dedicated", cost = "$0.005/query" },
                new { id = "enterprise", name = "Enterprise", provider = "Custom LLM", cost = "Custom" }
            }
        });
    }
}

public sealed class EnrichRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    public string ControlType { get; init; } = string.Empty;
    
    [System.ComponentModel.DataAnnotations.Required]
    public string AppName { get; init; } = string.Empty;
    
    [System.ComponentModel.DataAnnotations.Required]
    public string ElementName { get; init; } = string.Empty;
    
    public string ElementState { get; init; } = string.Empty;
    public Dictionary<string, string>? Properties { get; init; }
    public string[]? AvailableActions { get; init; }
    public string? UserQuery { get; init; }
    public string? Tier { get; init; } = "free";
}

public sealed class EnrichResponse
{
    public string Summary { get; init; } = string.Empty;
    public string? Shortcut { get; init; }
    public List<string> Tips { get; init; } = new();
    public string Provider { get; init; } = string.Empty;
    public double LatencyMs { get; init; }
    public int Confidence { get; init; }
}

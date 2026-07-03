using Microsoft.AspNetCore.Mvc;
using TooltipAI.Backend.Services;

namespace TooltipAI.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContextController : ControllerBase
{
    private readonly ContextCacheService _cacheService;

    public ContextController(ContextCacheService cacheService)
    {
        _cacheService = cacheService;
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> Get(string key)
    {
        var entry = await _cacheService.GetAsync(key);
        if (entry == null)
            return NotFound(new { message = "Context not cached" });

        return Ok(new { entry.Key, entry.Value, entry.Source, entry.CachedAt, entry.HitCount });
    }

    [HttpPost]
    public async Task<IActionResult> Set([FromBody] SetContextRequest request)
    {
        await _cacheService.SetAsync(request.Key, request.Value, request.Source ?? "api");
        return Ok(new { message = "Context cached" });
    }

    [HttpGet("stats")]
    public async Task<IActionResult> Stats()
    {
        var stats = await _cacheService.GetStatsAsync();
        return Ok(stats);
    }

    [HttpPost("cleanup")]
    public async Task<IActionResult> Cleanup()
    {
        var removed = await _cacheService.CleanupAsync();
        return Ok(new { removed });
    }
}

public record SetContextRequest(string Key, string Value, string? Source);

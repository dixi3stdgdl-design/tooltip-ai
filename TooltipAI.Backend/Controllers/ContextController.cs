using Microsoft.AspNetCore.Mvc;
using TooltipAI.Backend.Models;
using TooltipAI.Backend.Services;

namespace TooltipAI.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ContextController : ControllerBase
{
    private readonly ContextCacheService _contextCache;

    public ContextController(ContextCacheService contextCache)
    {
        _contextCache = contextCache;
    }

    [HttpGet("{key}")]
    public ActionResult<ContextEntry> Get(string key)
    {
        var entry = _contextCache.Get(key);
        if (entry == null)
        {
            return NotFound(new { error = "Context not found", key });
        }
        return Ok(entry);
    }

    [HttpPost]
    public ActionResult<object> Set([FromBody] ContextCacheRequest request)
    {
        _contextCache.Set(request);
        return Ok(new
        {
            success = true,
            key = request.Key,
            expiresAt = DateTime.UtcNow.AddSeconds(request.TtlSeconds)
        });
    }

    [HttpGet("stats")]
    public ActionResult<ContextCacheStats> Stats()
    {
        return Ok(_contextCache.GetStats());
    }
}

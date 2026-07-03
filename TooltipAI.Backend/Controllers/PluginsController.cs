using Microsoft.AspNetCore.Mvc;
using TooltipAI.Backend.Models;
using TooltipAI.Backend.Services;

namespace TooltipAI.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PluginsController : ControllerBase
{
    private readonly PluginRegistryService _registry;

    public PluginsController(PluginRegistryService registry)
    {
        _registry = registry;
    }

    [HttpGet]
    public ActionResult<IEnumerable<PluginManifest>> GetAll([FromQuery] string? query = null, [FromQuery] string[]? tags = null)
    {
        var plugins = _registry.Search(query, tags);
        return Ok(plugins);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var plugin = _registry.GetById(id);
        if (plugin == null)
            return NotFound(new { message = $"Plugin '{id}' not found" });

        return Ok(plugin);
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] PluginManifest manifest)
    {
        await _registry.RegisterAsync(manifest);
        return CreatedAtAction(nameof(GetById), new { id = manifest.Id }, manifest);
    }
}

using Microsoft.AspNetCore.Mvc;
using TooltipAI.Backend.Models;
using TooltipAI.Backend.Services;

namespace TooltipAI.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PluginsController : ControllerBase
{
    private readonly PluginRegistryService _pluginRegistry;

    public PluginsController(PluginRegistryService pluginRegistry)
    {
        _pluginRegistry = pluginRegistry;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<PluginInfo>> GetAll()
    {
        return Ok(_pluginRegistry.GetAll());
    }

    [HttpGet("{id}")]
    public ActionResult<PluginInfo> GetById(string id)
    {
        var plugin = _pluginRegistry.GetById(id);
        if (plugin == null)
        {
            return NotFound(new { error = "Plugin not found", id });
        }
        return Ok(plugin);
    }

    [HttpPost]
    public ActionResult<object> Register([FromBody] PluginRegisterRequest request)
    {
        var success = _pluginRegistry.Register(request);
        if (!success)
        {
            return Conflict(new { error = "Plugin already registered", id = request.Id });
        }
        return CreatedAtAction(nameof(GetById), new { id = request.Id }, new
        {
            success = true,
            id = request.Id,
            name = request.Name
        });
    }

    [HttpGet("stats")]
    public ActionResult<PluginRegistryStats> Stats()
    {
        return Ok(_pluginRegistry.GetStats());
    }
}

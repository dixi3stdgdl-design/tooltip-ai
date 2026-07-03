using System.Collections.Concurrent;
using TooltipAI.Backend.Models;

namespace TooltipAI.Backend.Services;

public class PluginRegistryService
{
    private readonly ILogger<PluginRegistryService> _logger;
    private readonly ConcurrentDictionary<string, PluginManifest> _plugins = new();

    public PluginRegistryService(ILogger<PluginRegistryService> logger)
    {
        _logger = logger;
        SeedDefaultPlugins();
    }

    private void SeedDefaultPlugins()
    {
        var defaults = new[]
        {
            new PluginManifest
            {
                Id = "tooltipai-context-azure",
                Name = "Azure Context Provider",
                Version = "1.0.0",
                Description = "Enriches tooltips with Azure resource context and documentation",
                Author = "TooltipAI Team",
                DownloadUrl = "https://github.com/dixi3stdgdl-design/tooltip-ai/releases/download/plugins/azure-context.dll",
                Hash = "sha256:placeholder",
                PublishedAt = DateTime.UtcNow,
                Tags = ["azure", "cloud", "context"],
                MinAppVersion = "1.0.0"
            },
            new PluginManifest
            {
                Id = "tooltipai-devtools",
                Name = "Developer Tools Pack",
                Version = "1.2.0",
                Description = "Extended tooltips for VS Code, JetBrains, and browser dev tools",
                Author = "TooltipAI Team",
                DownloadUrl = "https://github.com/dixi3stdgdl-design/tooltip-ai/releases/download/plugins/devtools.dll",
                Hash = "sha256:placeholder",
                PublishedAt = DateTime.UtcNow.AddDays(-7),
                Tags = ["developer", "ide", "browser"],
                MinAppVersion = "1.0.0"
            }
        };

        foreach (var plugin in defaults)
        {
            _plugins[plugin.Id] = plugin;
        }
    }

    public IEnumerable<PluginManifest> GetAll()
    {
        return _plugins.Values.Where(p => p.PublishedAt <= DateTime.UtcNow);
    }

    public IEnumerable<PluginManifest> Search(string? query = null, string[]? tags = null)
    {
        var results = _plugins.Values.AsEnumerable();

        if (!string.IsNullOrEmpty(query))
        {
            results = results.Where(p =>
                p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        if (tags is { Length: > 0 })
        {
            results = results.Where(p => tags.Any(t => p.Tags.Contains(t, StringComparer.OrdinalIgnoreCase)));
        }

        return results;
    }

    public PluginManifest? GetById(string id)
    {
        return _plugins.TryGetValue(id, out var plugin) ? plugin : null;
    }

    public async Task<bool> RegisterAsync(PluginManifest manifest)
    {
        _plugins[manifest.Id] = manifest;
        _logger.LogInformation("Plugin registered: {Id} v{Version}", manifest.Id, manifest.Version);
        await Task.CompletedTask;
        return true;
    }

    public async Task<int> CountAsync()
    {
        await Task.CompletedTask;
        return _plugins.Count;
    }
}

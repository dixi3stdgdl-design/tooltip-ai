using System.Collections.Concurrent;
using TooltipAI.Backend.Models;

namespace TooltipAI.Backend.Services;

public sealed class PluginRegistryService
{
    private readonly ILogger<PluginRegistryService> _logger;
    private readonly ConcurrentDictionary<string, PluginInfo> _plugins = new();

    public PluginRegistryService(ILogger<PluginRegistryService> logger)
    {
        _logger = logger;
        SeedOfficialPlugins();
    }

    public IReadOnlyList<PluginInfo> GetAll()
    {
        return _plugins.Values
            .OrderByDescending(p => p.Downloads)
            .ToList()
            .AsReadOnly();
    }

    public PluginInfo? GetById(string id)
    {
        _plugins.TryGetValue(id, out var plugin);
        return plugin;
    }

    public bool Register(PluginRegisterRequest request)
    {
        if (_plugins.ContainsKey(request.Id))
        {
            _logger.LogWarning("Plugin already registered: {PluginId}", request.Id);
            return false;
        }

        var plugin = new PluginInfo
        {
            Id = request.Id,
            Name = request.Name,
            Description = request.Description,
            Version = request.Version,
            Author = request.Author,
            DownloadUrl = request.DownloadUrl,
            Sha256Hash = request.Sha256Hash,
            MinAppVersion = request.MinAppVersion,
            PublishedAt = DateTime.UtcNow,
            Downloads = 0,
            IsOfficial = false
        };

        _plugins.TryAdd(request.Id, plugin);
        _logger.LogInformation("Plugin registered: {PluginId} by {Author}", request.Id, request.Author);
        return true;
    }

    public PluginRegistryStats GetStats()
    {
        var plugins = _plugins.Values.ToList();
        return new PluginRegistryStats
        {
            TotalPlugins = plugins.Count,
            OfficialPlugins = plugins.Count(p => p.IsOfficial),
            CommunityPlugins = plugins.Count(p => !p.IsOfficial),
            TotalDownloads = plugins.Sum(p => (long)p.Downloads)
        };
    }

    public int GetActivePluginCount()
    {
        return _plugins.Count;
    }

    private void SeedOfficialPlugins()
    {
        var official = new[]
        {
            new PluginInfo
            {
                Id = "tooltipai-context-basic",
                Name = "Basic Context Pack",
                Description = "Elementary context patterns for common Windows controls",
                Version = "1.0.0",
                Author = "MiMo Team",
                DownloadUrl = "https://api.tooltip-ai.com/plugins/tooltipai-context-basic.dll",
                Sha256Hash = "",
                MinAppVersion = 100,
                PublishedAt = DateTime.UtcNow,
                Downloads = 0,
                IsOfficial = true
            },
            new PluginInfo
            {
                Id = "tooltipai-shortcuts",
                Name = "Keyboard Shortcuts",
                Description = "Detects and displays keyboard shortcuts for focused elements",
                Version = "1.0.0",
                Author = "MiMo Team",
                DownloadUrl = "https://api.tooltip-ai.com/plugins/tooltipai-shortcuts.dll",
                Sha256Hash = "",
                MinAppVersion = 100,
                PublishedAt = DateTime.UtcNow,
                Downloads = 0,
                IsOfficial = true
            }
        };

        foreach (var plugin in official)
        {
            _plugins.TryAdd(plugin.Id, plugin);
        }
    }
}

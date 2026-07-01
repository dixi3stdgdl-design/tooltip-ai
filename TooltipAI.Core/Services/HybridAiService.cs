using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using TooltipAI.Core.Interfaces;
using TooltipAI.Core.Models;

namespace TooltipAI.Core.Services;

public class HybridAiService : IAIService, IDisposable
{
    private readonly AiComplexityConfig _config;
    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly Timer _cacheCleanupTimer;
    private readonly SoftwareCategoryClassifier _classifier = new();

    public HybridAiService(AiComplexityConfig config)
    {
        _config = config;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromMilliseconds(config.ApiTimeoutMs) };
        _cacheCleanupTimer = new Timer(CleanupCache, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    public async Task<string?> GetContextAsync(ElementInfo element, CancellationToken ct = default)
    {
        var complexity = DetermineComplexity(element);

        if (complexity.Level == AiComplexityLevel.None)
            return null;

        var cacheKey = $"ctx_{element.AutomationId}_{element.Name}";
        if (_cache.TryGetValue(cacheKey, out var cached) && !cached.IsExpired)
            return cached.Value;

        string? result = null;

        if (complexity.RequiresCloudApi && _config.EnableCloudApi)
        {
            result = await GetCloudContextAsync(element, ct);
        }
        else
        {
            result = GetLocalContext(element);
        }

        if (result is not null)
        {
            _cache[cacheKey] = new CacheEntry
            {
                Value = result,
                Expiration = DateTime.UtcNow.AddMinutes(_config.CacheExpirationMinutes)
            };
        }

        return result;
    }

    public async Task<string?> GetDescriptionAsync(ElementInfo element, CancellationToken ct = default)
    {
        var cacheKey = $"desc_{element.AutomationId}_{element.Name}";
        if (_cache.TryGetValue(cacheKey, out var cached) && !cached.IsExpired)
            return cached.Value;

        string? result = null;

        if (_config.EnableCloudApi)
        {
            result = await GetCloudDescriptionAsync(element, ct);
        }
        else
        {
            result = GetLocalDescription(element);
        }

        if (result is not null)
        {
            _cache[cacheKey] = new CacheEntry
            {
                Value = result,
                Expiration = DateTime.UtcNow.AddMinutes(_config.CacheExpirationMinutes)
            };
        }

        return result;
    }

    public string GetGestureHint(ElementInfo element, SoftwareCategory category)
    {
        var controlType = element.ControlType?.ToLower() ?? "";

        return controlType switch
        {
            "button" => "Click to activate",
            "edit" => "Type to input text",
            "text" => "Read-only element",
            "hyperlink" => "Click to navigate",
            "image" => "Click or hover for details",
            "slider" => "Drag to adjust value",
            "checkbox" => "Click to toggle",
            "combobox" => "Click to expand options",
            "listitem" => "Click to select",
            "treeitem" => "Click to expand",
            "tab" => "Click to switch view",
            "menu" => "Click to open menu",
            "toolbar" => "Contains action buttons",
            "scrollbar" => "Drag to scroll",
            "progress" => "Shows loading progress",
            "statusbar" => "Shows status information",
            _ => GetCategoryGestureHint(category)
        };
    }

    private string GetCategoryGestureHint(SoftwareCategory category)
    {
        return category switch
        {
            SoftwareCategory.Audio => "Hover for audio analysis",
            SoftwareCategory.Creative => "Hover for design context",
            SoftwareCategory.Development => "Hover for code insights",
            SoftwareCategory.Terminal => "Hover for command info",
            SoftwareCategory.Browser => "Hover for page context",
            SoftwareCategory.Video => "Hover for media details",
            SoftwareCategory.Gaming => "Hover for game info",
            _ => "Hover for element details"
        };
    }

    public string GetQualityTip(ElementInfo element, SoftwareCategory category)
    {
        var controlType = element.ControlType?.ToLower() ?? "";

        if (category == SoftwareCategory.Audio)
        {
            if (element.ClassName?.Contains("slider", StringComparison.OrdinalIgnoreCase) == true)
                return "Adjust slowly for precise tuning";
            if (element.ClassName?.Contains("knob", StringComparison.OrdinalIgnoreCase) == true)
                return "Fine-tune with small movements";
            if (controlType == "button")
                return "Toggle for A/B comparison";
        }

        if (category == SoftwareCategory.Creative)
        {
            if (controlType == "slider")
                return "Use for precise adjustments";
            if (controlType == "edit")
                return "Enter exact values for precision";
        }

        if (category == SoftwareCategory.Development)
        {
            if (controlType == "edit")
                return "Code input — check syntax";
            if (controlType == "button")
                return "Action trigger — verify before click";
        }

        return category switch
        {
            SoftwareCategory.Terminal => "Commands are case-sensitive",
            SoftwareCategory.Browser => "Check URL before entering data",
            SoftwareCategory.Office => "Save frequently to prevent data loss",
            SoftwareCategory.Security => "Verify before granting permissions",
            _ => "Standard interaction"
        };
    }

    public string GetMoveGuide(ElementInfo element, SoftwareCategory category)
    {
        var controlType = element.ControlType?.ToLower() ?? "";

        return controlType switch
        {
            "slider" => "Drag vertically or horizontally",
            "scrollbar" => "Drag along the track",
            "splitter" => "Drag to resize panels",
            "thumb" => "Drag to reposition",
            _ => category switch
            {
                SoftwareCategory.Audio => "Move knobs slowly for smooth transitions",
                SoftwareCategory.Creative => "Click and drag for direct manipulation",
                SoftwareCategory.Development => "Select text to copy or edit",
                SoftwareCategory.Terminal => "Click to focus, then type commands",
                _ => "Click to interact"
            }
        };
    }

    public string GetDataInsight(ElementInfo element, SoftwareCategory category)
    {
        var controlType = element.ControlType?.ToLower() ?? "";

        if (!string.IsNullOrEmpty(element.HelpText))
            return element.HelpText;

        if (category == SoftwareCategory.Audio)
            return "Audio processing active";

        if (category == SoftwareCategory.Development)
            return "Build status: Ready";

        if (category == SoftwareCategory.Browser)
            return "Page loaded";

        return controlType switch
        {
            "progress" => "Loading in progress...",
            "statusbar" => "System operational",
            "text" => "Static content",
            _ => "Element active"
        };
    }

    private AiComplexityResult DetermineComplexity(ElementInfo element)
    {
        var result = new AiComplexityResult
        {
            CacheKey = $"{element.AutomationId}_{element.Name}"
        };

        if (string.IsNullOrEmpty(element.Name) && string.IsNullOrEmpty(element.HelpText))
        {
            result.Level = AiComplexityLevel.None;
            result.RequiresCloudApi = false;
            return result;
        }

        if (!string.IsNullOrEmpty(element.HelpText))
        {
            result.Level = AiComplexityLevel.Basic;
            result.LocalContext = element.HelpText;
            result.RequiresCloudApi = false;
            return result;
        }

        if (!string.IsNullOrEmpty(element.Name) && element.Name.Length > 3)
        {
            result.Level = AiComplexityLevel.Standard;
            result.RequiresCloudApi = true;
            return result;
        }

        result.Level = AiComplexityLevel.Complex;
        result.RequiresCloudApi = true;
        return result;
    }

    public string GetLocalContext(ElementInfo element)
    {
        var sb = new StringBuilder();
        sb.Append($"Type: {element.ControlType}");

        if (!string.IsNullOrEmpty(element.ClassName))
            sb.Append($" | Class: {element.ClassName}");

        if (element.IsEnabled)
            sb.Append(" | Status: Enabled");
        else
            sb.Append(" | Status: Disabled");

        if (element.IsKeyboardFocusable)
            sb.Append(" | Keyboard: Focusable");

        return sb.ToString();
    }

    public string GetLocalDescription(ElementInfo element)
    {
        return element.ControlType switch
        {
            "Button" => "Interactive button element",
            "Edit" => "Text input field",
            "Text" => "Static text display",
            "Hyperlink" => "Clickable link",
            "Image" => "Image element",
            _ => $"UI Element: {element.ControlType}"
        };
    }

    private async Task<string?> GetCloudContextAsync(ElementInfo element, CancellationToken ct)
    {
        try
        {
            var prompt = $"Describe what this UI element does: Name='{element.Name}', Type='{element.ControlType}', ClassName='{element.ClassName}'";
            var response = await CallCloudApiAsync(prompt, ct);
            return response;
        }
        catch
        {
            return GetLocalContext(element);
        }
    }

    private async Task<string?> GetCloudDescriptionAsync(ElementInfo element, CancellationToken ct)
    {
        try
        {
            var prompt = $"Give a brief description of this UI element: {element.Name} ({element.ControlType})";
            var response = await CallCloudApiAsync(prompt, ct);
            return response;
        }
        catch
        {
            return GetLocalDescription(element);
        }
    }

    private async Task<string?> CallCloudApiAsync(string prompt, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(_config.ApiEndpoint) || string.IsNullOrEmpty(_config.ApiKey))
            return null;

        var modelName = _config.ModelName ?? "gpt-3.5-turbo";

        var request = new
        {
            model = modelName,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            max_tokens = 100
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        content.Headers.Add("Authorization", $"Bearer {_config.ApiKey}");

        var response = await _httpClient.PostAsync(_config.ApiEndpoint, content, ct);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(responseJson);
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();
    }

    private void CleanupCache(object? state)
    {
        var now = DateTime.UtcNow;
        foreach (var key in _cache.Keys)
        {
            if (_cache.TryGetValue(key, out var entry) && entry.IsExpired)
            {
                _cache.TryRemove(key, out _);
            }
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        _cacheCleanupTimer?.Dispose();
    }

    private class CacheEntry
    {
        public string? Value { get; set; }
        public DateTime Expiration { get; set; }
        public bool IsExpired => DateTime.UtcNow > Expiration;
    }
}

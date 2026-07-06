using Microsoft.Extensions.Caching.Memory;
using TooltipAI.Core.Services;

namespace TooltipAI.Backend.Services;

public class EnrichmentEngine
{
    private readonly LLMProvider _llmProvider;
    private readonly PIIFilter _piiFilter;
    private readonly IMemoryCache _cache;
    private readonly ILogger<EnrichmentEngine> _logger;

    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public EnrichmentEngine(
        LLMProvider llmProvider,
        PIIFilter piiFilter,
        IMemoryCache cache,
        ILogger<EnrichmentEngine> logger)
    {
        _llmProvider = llmProvider;
        _piiFilter = piiFilter;
        _cache = cache;
        _logger = logger;
    }

    public async Task<EnrichmentResult> EnrichAsync(string controlType, string appName, Dictionary<string, string>? properties = null)
    {
        var cacheKey = $"{appName}:{controlType}";
        
        if (_cache.TryGetValue<EnrichmentResult>(cacheKey, out var cached) && cached != null)
        {
            _logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return cached;
        }

        // Try rule-based enrichment first
        var ruleResult = GetRuleBasedEnrichment(controlType, appName);
        if (ruleResult != null)
        {
            _cache.Set(cacheKey, ruleResult, CacheTtl);
            return ruleResult;
        }

        // Fall back to LLM enrichment
        try
        {
            var sanitizedProps = properties != null 
                ? _piiFilter.SanitizeProperties(properties) 
                : properties;

            var llmResult = await _llmProvider.EnrichAsync(controlType, appName, sanitizedProps);
            
            _cache.Set(cacheKey, llmResult, CacheTtl);
            return llmResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LLM enrichment failed");
            
            return new EnrichmentResult
            {
                Summary = $"UI Element: {controlType}",
                Source = "fallback"
            };
        }
    }

    private EnrichmentResult? GetRuleBasedEnrichment(string controlType, string appName)
    {
        var rules = GetRulesForApp(appName);
        
        foreach (var rule in rules)
        {
            if (rule.ControlType.Equals(controlType, StringComparison.OrdinalIgnoreCase))
            {
                return new EnrichmentResult
                {
                    Summary = rule.Context,
                    Shortcut = rule.Shortcut,
                    Tips = rule.Tips,
                    Source = "rules"
                };
            }
        }

        return null;
    }

    private List<EnrichmentRule> GetRulesForApp(string appName)
    {
        var lower = appName.ToLowerInvariant();
        
        if (lower.Contains("excel") || lower.Contains("word") || lower.Contains("powerpoint"))
        {
            return OfficeRules;
        }
        
        if (lower.Contains("chrome") || lower.Contains("firefox") || lower.Contains("edge"))
        {
            return BrowserRules;
        }
        
        if (lower.Contains("code"))
        {
            return VSCodeRules;
        }

        return GenericRules;
    }

    private static readonly List<EnrichmentRule> OfficeRules = new()
    {
        new() { ControlType = "Button", Context = "Office ribbon button. Click to activate the associated command.", Shortcut = null },
        new() { ControlType = "ComboBox", Context = "Office dropdown selector. Click to expand and choose from available options.", Shortcut = null },
        new() { ControlType = "TextBox", Context = "Text input field. Type to enter data or search.", Shortcut = null },
        new() { ControlType = "CheckBox", Context = "Toggle option on or off.", Shortcut = null },
    };

    private static readonly List<EnrichmentRule> BrowserRules = new()
    {
        new() { ControlType = "Button", Context = "Browser action button. Click to perform the associated action.", Shortcut = null },
        new() { ControlType = "Link", Context = "Clickable hyperlink. Will navigate to the linked page.", Shortcut = null },
        new() { ControlType = "TextBox", Context = "Address bar or search field. Type URL or search query.", Shortcut = "Ctrl+L" },
        new() { ControlType = "Tab", Context = "Browser tab. Click to switch between open pages.", Shortcut = "Ctrl+Tab" },
    };

    private static readonly List<EnrichmentRule> VSCodeRules = new()
    {
        new() { ControlType = "Button", Context = "VS Code action button. Click to trigger the associated command.", Shortcut = null },
        new() { ControlType = "TreeView", Context = "File explorer or sidebar tree. Navigate project structure.", Shortcut = "Ctrl+Shift+E" },
        new() { ControlType = "Editor", Context = "Code editor. Type to edit, use shortcuts for efficiency.", Shortcut = "Ctrl+P" },
        new() { ControlType = "Terminal", Context = "Integrated terminal. Run commands and scripts.", Shortcut = "Ctrl+`" },
    };

    private static readonly List<EnrichmentRule> GenericRules = new()
    {
        new() { ControlType = "Button", Context = "Interactive button. Click to activate.", Shortcut = null },
        new() { ControlType = "TextBox", Context = "Text input field. Click to type.", Shortcut = null },
        new() { ControlType = "ComboBox", Context = "Dropdown selection. Click to expand options.", Shortcut = null },
        new() { ControlType = "CheckBox", Context = "Toggle option on/off.", Shortcut = null },
        new() { ControlType = "Link", Context = "Clickable hyperlink. Opens destination.", Shortcut = null },
        new() { ControlType = "Menu", Context = "Context menu with options.", Shortcut = null },
        new() { ControlType = "Tab", Context = "Switch between views or sections.", Shortcut = null },
        new() { ControlType = "Slider", Context = "Drag to adjust value within range.", Shortcut = null },
        new() { ControlType = "ProgressBar", Context = "Shows completion status of an operation.", Shortcut = null },
        new() { ControlType = "TreeView", Context = "Expandable/collapsible item in a tree view.", Shortcut = null },
        new() { ControlType = "DataGrid", Context = "Table with rows and columns of data.", Shortcut = null },
    };
}

public sealed class EnrichmentResult
{
    public string Summary { get; init; } = string.Empty;
    public string? Shortcut { get; init; }
    public List<string>? Tips { get; init; }
    public string Source { get; init; } = string.Empty;
}

public sealed class EnrichmentRule
{
    public string ControlType { get; init; } = string.Empty;
    public string Context { get; init; } = string.Empty;
    public string? Shortcut { get; init; }
    public List<string>? Tips { get; init; }
}

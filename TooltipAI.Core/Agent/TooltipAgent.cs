using TooltipAI.Core.Models;
using TooltipAI.Core.Rules;
using TooltipAI.Core.Services;

namespace TooltipAI.Core.Agent;

public class TooltipAgent : IDisposable
{
    private readonly AppSpecificRules _rules;
    private readonly ResponseCacheService _cache;
    private readonly SettingsService _settings;
    private readonly PIIFilter _piiFilter;
    private readonly LoggingService _logging;

    private DateTime _lastElementTime = DateTime.MinValue;
    private string _lastElementKey = string.Empty;
    private bool _isEnabled = true;

    public bool IsEnabled => _isEnabled;
    public int CacheSize => _cache.EntryCount;
    public int RuleCount => _rules.RuleCount;

    public event Action<TooltipData>? TooltipReady;
    public event Action? TooltipHidden;

    public TooltipAgent(string? settingsPath = null, string? rulesPath = null, string? cachePath = null)
    {
        _settings = new SettingsService(settingsPath);
        _rules = new AppSpecificRules(rulesPath);
        _cache = new ResponseCacheService(cachePath);
        _piiFilter = PIIFilter.Instance;
        _logging = new LoggingService();

        _settings.SettingsChanged += OnSettingsChanged;
        _rules.RulesChanged += OnRulesChanged;
    }

    public TooltipData? ProcessElement(ElementInfo element)
    {
        if (!_isEnabled)
            return null;

        if (element == null || string.IsNullOrEmpty(element.Name))
        {
            element = CreateFallbackElement(element);
        }

        var dedupeKey = _cache.GenerateKey(element);
        if (dedupeKey == _lastElementKey && (DateTime.UtcNow - _lastElementTime).TotalMilliseconds < 100)
        {
            return null;
        }

        _lastElementKey = dedupeKey;
        _lastElementTime = DateTime.UtcNow;

        var cached = _cache.Get(dedupeKey);
        if (cached != null)
        {
            OnTooltipReady(cached);
            return cached;
        }

        var tooltipData = BuildTooltipData(element);

        _cache.Set(dedupeKey, tooltipData);

        OnTooltipReady(tooltipData);

        _logging.LogInfo($"Tooltip shown: {element.ProcessName}/{element.Name}");

        return tooltipData;
    }

    public void HideTooltip()
    {
        TooltipHidden?.Invoke();
    }

    public void Enable()
    {
        _isEnabled = true;
        _settings.UpdateSettings(s => s.IsEnabled = true);
        _logging.LogInfo("Agent enabled");
    }

    public void Disable()
    {
        _isEnabled = false;
        _settings.UpdateSettings(s => s.IsEnabled = false);
        HideTooltip();
        _logging.LogInfo("Agent disabled");
    }

    public void ClearCache()
    {
        _cache.Clear();
        _logging.LogInfo("Cache cleared");
    }

    public CacheStats GetCacheStats()
    {
        return _cache.GetStats();
    }

    public AppSettings GetSettings()
    {
        return _settings.GetSettings();
    }

    public void UpdateSettings(Action<AppSettings> update)
    {
        _settings.UpdateSettings(update);
    }

    private TooltipData BuildTooltipData(ElementInfo element)
    {
        var settings = _settings.GetSettings();

        var context = _rules.GetContextForElement(element);
        var shortcut = _rules.GetShortcutForElement(element);

        if (settings.ShowAiContext && !string.IsNullOrEmpty(context))
        {
            context = _piiFilter.SanitizeForTransmission(context);
        }

        return new TooltipData
        {
            Element = element,
            EnrichedContext = context,
            FunctionHint = shortcut,
            SoftwareCategory = ClassifyApp(element.ProcessName),
            ProcessName = element.ProcessName,
            WindowTitle = element.WindowTitle
        };
    }

    private string ClassifyApp(string processName)
    {
        var lower = processName.ToLowerInvariant();
        if (lower.Contains("excel") || lower.Contains("winword") || lower.Contains("powerpnt"))
            return "Office";
        if (lower.Contains("chrome") || lower.Contains("firefox") || lower.Contains("edge"))
            return "Browser";
        if (lower.Contains("code"))
            return "IDE";
        return "Unknown";
    }

    private ElementInfo CreateFallbackElement(ElementInfo? original)
    {
        return new ElementInfo
        {
            Name = !string.IsNullOrEmpty(original?.Name) ? original!.Name : "(unknown)",
            ControlType = !string.IsNullOrEmpty(original?.ControlType) ? original!.ControlType : "Unknown",
            ClassName = original?.ClassName ?? "",
            ProcessName = original?.ProcessName ?? "",
            WindowTitle = original?.WindowTitle ?? "",
            IsEnabled = original?.IsEnabled ?? true,
            IsKeyboardFocusable = original?.IsKeyboardFocusable ?? false,
            Timestamp = DateTime.UtcNow
        };
    }

    private void OnSettingsChanged(AppSettings settings)
    {
        _isEnabled = settings.IsEnabled;
    }

    private void OnRulesChanged()
    {
        _logging.LogInfo($"Rules updated: {_rules.RuleCount} rules loaded");
    }

    private void OnTooltipReady(TooltipData data)
    {
        TooltipReady?.Invoke(data);
    }

    public void Dispose()
    {
        _settings.SettingsChanged -= OnSettingsChanged;
        _rules.RulesChanged -= OnRulesChanged;
        _settings.Dispose();
        _rules.Dispose();
        _cache.Dispose();
    }
}

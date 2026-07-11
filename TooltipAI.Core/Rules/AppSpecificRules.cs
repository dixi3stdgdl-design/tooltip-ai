using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TooltipAI.Core.Models;

namespace TooltipAI.Core.Rules;

public class AppSpecificRules : IDisposable
{
    private readonly string _rulesPath;
    private readonly FileSystemWatcher? _watcher;
    private readonly ILogger? _logger;
    private List<RuleDefinition> _rules = new();
    private DateTime _lastLoad = DateTime.MinValue;

    public event Action? RulesChanged;

    public int RuleCount => _rules.Count;
    public DateTime LastUpdated => _lastLoad;

    public AppSpecificRules(string? customPath = null, ILogger? logger = null)
    {
        _logger = logger;
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "TooltipAI");
        Directory.CreateDirectory(appFolder);

        _rulesPath = customPath ?? Path.Combine(appFolder, "rules.json");

        if (!File.Exists(_rulesPath))
        {
            CopyDefaultRules();
        }

        try
        {
            _rules = LoadRules();
        }
        catch (Exception ex)
        {
            ReportError(ex, "load");
        }

        try
        {
            var watcher = new FileSystemWatcher(Path.GetDirectoryName(_rulesPath)!, Path.GetFileName(_rulesPath))
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };
            watcher.Changed += OnRulesChanged;
            _watcher = watcher;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Rules file watcher is unavailable for {Path}", _rulesPath);
            if (_logger is null)
                Trace.TraceWarning($"Rules file watcher is unavailable for '{_rulesPath}': {ex}");
        }
    }

    public string? GetContextForElement(ElementInfo element)
    {
        var processName = element.ProcessName.ToLowerInvariant();
        var elementName = element.Name.ToLowerInvariant();
        var className = element.ClassName.ToLowerInvariant();
        var controlType = element.ControlType.ToLowerInvariant();

        foreach (var rule in _rules)
        {
            if (RuleMatches(rule, processName, elementName, className, controlType))
            {
                return rule.Context;
            }
        }

        return GenerateDefaultContext(element);
    }

    public string? GetShortcutForElement(ElementInfo element)
    {
        var processName = element.ProcessName.ToLowerInvariant();
        var elementName = element.Name.ToLowerInvariant();
        var className = element.ClassName.ToLowerInvariant();
        var controlType = element.ControlType.ToLowerInvariant();

        foreach (var rule in _rules)
        {
            if (RuleMatches(rule, processName, elementName, className, controlType) &&
                !string.IsNullOrEmpty(rule.Shortcut))
            {
                return rule.Shortcut;
            }
        }

        return null;
    }

    public bool ReloadRules()
    {
        try
        {
            _rules = LoadRules();
            return true;
        }
        catch (Exception ex)
        {
            ReportError(ex, "reload");
            return false;
        }
    }

    public List<RuleDefinition> GetAllRules() => new(_rules);

    public List<RuleDefinition> GetRulesForApp(string processName)
    {
        return _rules.Where(r =>
            r.Apps.Contains("*") ||
            r.Apps.Any(a => processName.Contains(a, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    private bool RuleMatches(RuleDefinition rule, string processName, string elementName, string className, string controlType)
    {
        if (!rule.Apps.Contains("*") &&
            !rule.Apps.Any(a => processName.Contains(a, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(rule.Match.ControlType) &&
            !controlType.Contains(rule.Match.ControlType, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (rule.Match.NameContains != null && rule.Match.NameContains.Count > 0)
        {
            if (rule.Match.NameContains.Any(n => elementName.Contains(n, StringComparison.OrdinalIgnoreCase)))
                return true;
        }

        if (rule.Match.ClassName != null && rule.Match.ClassName.Count > 0)
        {
            if (rule.Match.ClassName.Any(c => className.Contains(c, StringComparison.OrdinalIgnoreCase)))
                return true;
        }

        if (!string.IsNullOrEmpty(rule.Match.ControlType) && string.IsNullOrEmpty(elementName))
        {
            return controlType.Contains(rule.Match.ControlType, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private string GenerateDefaultContext(ElementInfo element)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(element.ControlType))
            parts.Add($"Type: {element.ControlType}");

        if (!string.IsNullOrEmpty(element.ClassName))
            parts.Add($"Class: {element.ClassName}");

        parts.Add(element.IsEnabled ? "Enabled" : "Disabled");

        if (element.IsKeyboardFocusable)
            parts.Add("Focusable");

        return string.Join(" | ", parts);
    }

    private List<RuleDefinition> LoadRules()
    {
        if (File.Exists(_rulesPath))
        {
            var json = File.ReadAllText(_rulesPath);
            var rules = JsonSerializer.Deserialize<List<RuleDefinition>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            _lastLoad = DateTime.UtcNow;
            return rules ?? new List<RuleDefinition>();
        }

        return new List<RuleDefinition>();
    }

    private void CopyDefaultRules()
    {
        try
        {
            var assembly = typeof(AppSpecificRules).Assembly;
            var resourceName = "TooltipAI.Core.Rules.rules.json";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                var content = reader.ReadToEnd();
                File.WriteAllText(_rulesPath, content);
            }
        }
        catch (Exception ex)
        {
            ReportError(ex, "copy default");
            var defaultRules = new List<RuleDefinition>
            {
                new()
                {
                    Id = "generic-button",
                    Apps = new List<string> { "*" },
                    Match = new RuleMatch { ControlType = "Button" },
                    Context = "Interactive button. Click to activate.",
                    Shortcut = "",
                    Version = "1.0.0"
                }
            };
            var json = JsonSerializer.Serialize(defaultRules, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_rulesPath, json);
        }
    }

    private void OnRulesChanged(object sender, FileSystemEventArgs e)
    {
        var lastWrite = File.GetLastWriteTime(_rulesPath);
        if (lastWrite <= _lastLoad)
            return;

        try
        {
            var rules = LoadRules();
            _rules = rules;
            RulesChanged?.Invoke();
        }
        catch (Exception ex)
        {
            ReportError(ex, "reload after file change");
        }
    }

    private void ReportError(Exception ex, string operation)
    {
        _logger?.LogError(ex, "Failed to {Operation} rules from {Path}", operation, _rulesPath);
        if (_logger is null)
            Trace.TraceError($"Failed to {operation} rules from '{_rulesPath}': {ex}");
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }
}

public class RuleDefinition
{
    public string Id { get; set; } = string.Empty;
    public List<string> Apps { get; set; } = new();
    public RuleMatch Match { get; set; } = new();
    public string Context { get; set; } = string.Empty;
    public string Shortcut { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
}

public class RuleMatch
{
    public List<string>? NameContains { get; set; }
    public List<string>? ClassName { get; set; }
    public string ControlType { get; set; } = string.Empty;
}

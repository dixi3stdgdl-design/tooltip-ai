using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace TooltipAI.Core.Services;

public class ConsentManager : IDisposable
{
    private readonly string _consentPath;
    private readonly object _lock = new();
    private ConsentState _state;
    private readonly FileSystemWatcher? _watcher;
    private readonly ILogger? _logger;

    public event Action<ConsentState>? ConsentChanged;

    public ConsentState State
    {
        get
        {
            lock (_lock)
            {
                return _state;
            }
        }
    }

    public ConsentManager(string? customPath = null, ILogger? logger = null)
    {
        _logger = logger;
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "TooltipAI");
        Directory.CreateDirectory(appFolder);

        _consentPath = customPath ?? Path.Combine(appFolder, "consent.json");
        _state = LoadState();

        try
        {
            _watcher = new FileSystemWatcher(Path.GetDirectoryName(_consentPath)!, Path.GetFileName(_consentPath))
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };
            _watcher.Changed += OnConsentChanged;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Consent file watcher is unavailable for {Path}", _consentPath);
            if (_logger is null)
                Trace.TraceWarning($"Consent file watcher is unavailable for '{_consentPath}': {ex}");
        }
    }

    public bool IsAIEnrichmentEnabled => _state.AIEnrichmentEnabled && !_state.LocalOnlyMode;
    public bool IsTelemetryEnabled => _state.TelemetryEnabled && !_state.LocalOnlyMode;
    public bool IsLocalOnlyMode => _state.LocalOnlyMode;
    public bool IsAppBlacklisted(string processName)
    {
        return _state.AppBlacklist.Any(app => 
            processName.Contains(app, StringComparison.OrdinalIgnoreCase));
    }

    public void EnableAIEnrichment(bool enabled)
    {
        lock (_lock)
        {
            UpdateState(state => state.AIEnrichmentEnabled = enabled);
        }
        ConsentChanged?.Invoke(_state);
    }

    public void EnableTelemetry(bool enabled)
    {
        lock (_lock)
        {
            UpdateState(state => state.TelemetryEnabled = enabled);
        }
        ConsentChanged?.Invoke(_state);
    }

    public void SetLocalOnlyMode(bool enabled)
    {
        lock (_lock)
        {
            UpdateState(state => state.LocalOnlyMode = enabled);
        }
        ConsentChanged?.Invoke(_state);
    }

    public void AddToBlacklist(string processName)
    {
        lock (_lock)
        {
            if (_state.AppBlacklist.Contains(processName))
                return;

            UpdateState(state => state.AppBlacklist.Add(processName));
        }
        ConsentChanged?.Invoke(_state);
    }

    public void RemoveFromBlacklist(string processName)
    {
        lock (_lock)
        {
            UpdateState(state => state.AppBlacklist.Remove(processName));
        }
        ConsentChanged?.Invoke(_state);
    }

    public void SetAppBlacklist(List<string> apps)
    {
        lock (_lock)
        {
            UpdateState(state => state.AppBlacklist = new List<string>(apps));
        }
        ConsentChanged?.Invoke(_state);
    }

    public void ResetToDefaults()
    {
        lock (_lock)
        {
            var defaults = new ConsentState();
            SaveState(defaults);
            _state = defaults;
        }
        ConsentChanged?.Invoke(_state);
    }

    private void UpdateState(Action<ConsentState> update)
    {
        var updated = new ConsentState
        {
            AIEnrichmentEnabled = _state.AIEnrichmentEnabled,
            TelemetryEnabled = _state.TelemetryEnabled,
            LocalOnlyMode = _state.LocalOnlyMode,
            AppBlacklist = new List<string>(_state.AppBlacklist),
            LastUpdated = DateTime.UtcNow
        };
        update(updated);
        SaveState(updated);
        _state = updated;
    }

    private ConsentState LoadState()
    {
        try
        {
            if (File.Exists(_consentPath))
            {
                var json = File.ReadAllText(_consentPath);
                return JsonSerializer.Deserialize<ConsentState>(json) ?? new ConsentState();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load consent state from {Path}", _consentPath);
            if (_logger is null)
                Trace.TraceError($"Failed to load consent state from '{_consentPath}': {ex}");
        }
        return new ConsentState();
    }

    private void SaveState(ConsentState state)
    {
        try
        {
            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_consentPath, json);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to save consent state to {Path}", _consentPath);
            if (_logger is null)
                Trace.TraceError($"Failed to save consent state to '{_consentPath}': {ex}");
            throw;
        }
    }

    private void OnConsentChanged(object sender, FileSystemEventArgs e)
    {
        lock (_lock)
        {
            _state = LoadState();
        }
        ConsentChanged?.Invoke(_state);
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }
}

public class ConsentState
{
    public bool AIEnrichmentEnabled { get; set; } = false;
    public bool TelemetryEnabled { get; set; } = false;
    public bool LocalOnlyMode { get; set; } = true;
    public List<string> AppBlacklist { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

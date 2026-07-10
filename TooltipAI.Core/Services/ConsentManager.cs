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
        catch
        {
            // FileSystemWatcher not available in all contexts
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
            _state.AIEnrichmentEnabled = enabled;
            SaveState(_state);
        }
        ConsentChanged?.Invoke(_state);
    }

    public void EnableTelemetry(bool enabled)
    {
        lock (_lock)
        {
            _state.TelemetryEnabled = enabled;
            SaveState(_state);
        }
        ConsentChanged?.Invoke(_state);
    }

    public void SetLocalOnlyMode(bool enabled)
    {
        lock (_lock)
        {
            _state.LocalOnlyMode = enabled;
            SaveState(_state);
        }
        ConsentChanged?.Invoke(_state);
    }

    public void AddToBlacklist(string processName)
    {
        lock (_lock)
        {
            if (!_state.AppBlacklist.Contains(processName))
            {
                _state.AppBlacklist.Add(processName);
                SaveState(_state);
            }
        }
        ConsentChanged?.Invoke(_state);
    }

    public void RemoveFromBlacklist(string processName)
    {
        lock (_lock)
        {
            _state.AppBlacklist.Remove(processName);
            SaveState(_state);
        }
        ConsentChanged?.Invoke(_state);
    }

    public void SetAppBlacklist(List<string> apps)
    {
        lock (_lock)
        {
            _state.AppBlacklist = apps;
            SaveState(_state);
        }
        ConsentChanged?.Invoke(_state);
    }

    public void ResetToDefaults()
    {
        lock (_lock)
        {
            _state = new ConsentState();
            SaveState(_state);
        }
        ConsentChanged?.Invoke(_state);
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

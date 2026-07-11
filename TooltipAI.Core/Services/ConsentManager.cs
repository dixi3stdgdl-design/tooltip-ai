using Microsoft.Extensions.Logging;
using TooltipAI.Core.Common;

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
        var appFolder = AppDataPaths.EnsureRoot();

        _consentPath = customPath ?? Path.Combine(appFolder, "consent.json");
        _state = LoadState();

        _watcher = FileChangeWatcher.TryWatch(_consentPath, OnConsentChanged);
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
        => JsonFile.Load(_consentPath, () => new ConsentState(), _logger, description: "consent state");

    private void SaveState(ConsentState state)
        => JsonFile.Save(_consentPath, state, _logger, description: "consent state");

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

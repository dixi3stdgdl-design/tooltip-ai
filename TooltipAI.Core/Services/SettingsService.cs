using Microsoft.Extensions.Logging;
using TooltipAI.Core.Common;

namespace TooltipAI.Core.Services;

public class SettingsService : IDisposable
{
    private readonly string _settingsPath;
    private readonly FileSystemWatcher? _watcher;
    private readonly object _lock = new();
    private AppSettings _settings;
    private DateTime _lastLoad = DateTime.MinValue;
    private readonly ILogger? _logger;

    public event Action<AppSettings>? SettingsChanged;

    public SettingsService(string? customPath = null, ILogger? logger = null)
    {
        _logger = logger;
        var appFolder = AppDataPaths.EnsureRoot();

        _settingsPath = customPath ?? Path.Combine(appFolder, "settings.json");
        _settings = LoadSettings();

        _watcher = FileChangeWatcher.TryWatch(_settingsPath, OnSettingsChanged);
    }

    public AppSettings GetSettings()
    {
        lock (_lock)
        {
            return _settings;
        }
    }

    public void UpdateSettings(Action<AppSettings> update)
    {
        lock (_lock)
        {
            update(_settings);
            SaveSettings(_settings);
        }
    }

    public void ResetToDefaults()
    {
        lock (_lock)
        {
            _settings = new AppSettings();
            SaveSettings(_settings);
        }
    }

    private AppSettings LoadSettings()
        => JsonFile.Load(_settingsPath, () => new AppSettings(), _logger, description: "settings");

    private void SaveSettings(AppSettings settings)
    {
        if (JsonFile.Save(_settingsPath, settings, _logger, description: "settings"))
            _lastLoad = DateTime.UtcNow;
    }

    private void OnSettingsChanged(object sender, FileSystemEventArgs e)
    {
        var lastWrite = File.GetLastWriteTime(_settingsPath);
        if (lastWrite <= _lastLoad)
            return;

        lock (_lock)
        {
            _settings = LoadSettings();
        }

        SettingsChanged?.Invoke(_settings);
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }
}

public class AppSettings
{
    public bool IsEnabled { get; set; } = true;
    public bool ShowAiContext { get; set; } = true;
    public int TooltipDelayMs { get; set; } = 100;
    public int TooltipMaxWidth { get; set; } = 400;
    public int TooltipMaxHeight { get; set; } = 250;
    public string Theme { get; set; } = "System";
    public string Language { get; set; } = "en";
    public bool EnableNotifications { get; set; } = true;
    public bool EnableSound { get; set; } = false;
    public string? ApiEndpoint { get; set; }
    public string? ApiKey { get; set; }
    public int ApiTimeoutMs { get; set; } = 3000;
    public bool EnableTelemetry { get; set; } = false;
    public string? LastWindowState { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

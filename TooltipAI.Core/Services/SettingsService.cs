using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace TooltipAI.Core.Services;

public class SettingsService : IDisposable
{
    private readonly string _settingsPath;
    private readonly FileSystemWatcher _watcher;
    private readonly object _lock = new();
    private AppSettings _settings;
    private DateTime _lastLoad = DateTime.MinValue;
    private readonly ILogger? _logger;

    public event Action<AppSettings>? SettingsChanged;

    public SettingsService(string? customPath = null, ILogger? logger = null)
    {
        _logger = logger;
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "TooltipAI");
        Directory.CreateDirectory(appFolder);

        _settingsPath = customPath ?? Path.Combine(appFolder, "settings.json");
        _settings = LoadSettings();

        _watcher = new FileSystemWatcher(Path.GetDirectoryName(_settingsPath)!, Path.GetFileName(_settingsPath))
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = true
        };
        _watcher.Changed += OnSettingsChanged;
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
            var updated = CloneSettings(_settings);
            update(updated);
            SaveSettings(updated);
            _settings = updated;
        }
    }

    public void ResetToDefaults()
    {
        lock (_lock)
        {
            var defaults = new AppSettings();
            SaveSettings(defaults);
            _settings = defaults;
        }
    }

    private AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load settings from {Path}", _settingsPath);
            if (_logger is null)
                Trace.TraceError($"Failed to load settings from '{_settingsPath}': {ex}");
        }
        return new AppSettings();
    }

    private void SaveSettings(AppSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
            _lastLoad = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to save settings to {Path}", _settingsPath);
            if (_logger is null)
                Trace.TraceError($"Failed to save settings to '{_settingsPath}': {ex}");
            throw;
        }
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

    private static AppSettings CloneSettings(AppSettings settings)
    {
        return new AppSettings
        {
            IsEnabled = settings.IsEnabled,
            ShowAiContext = settings.ShowAiContext,
            TooltipDelayMs = settings.TooltipDelayMs,
            TooltipMaxWidth = settings.TooltipMaxWidth,
            TooltipMaxHeight = settings.TooltipMaxHeight,
            Theme = settings.Theme,
            Language = settings.Language,
            EnableNotifications = settings.EnableNotifications,
            EnableSound = settings.EnableSound,
            ApiEndpoint = settings.ApiEndpoint,
            ApiKey = settings.ApiKey,
            ApiTimeoutMs = settings.ApiTimeoutMs,
            EnableTelemetry = settings.EnableTelemetry,
            LastWindowState = settings.LastWindowState,
            LastUpdated = settings.LastUpdated
        };
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

using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace TooltipAI.Core.Services;

public class AppBlacklistService : IDisposable
{
    private readonly string _blacklistPath;
    private readonly object _lock = new();
    private List<string> _blacklist;
    private readonly FileSystemWatcher? _watcher;
    private readonly ILogger? _logger;

    public event Action<List<string>>? BlacklistChanged;

    public List<string> Blacklist
    {
        get
        {
            lock (_lock)
            {
                return new List<string>(_blacklist);
            }
        }
    }

    public AppBlacklistService(string? customPath = null, ILogger? logger = null)
    {
        _logger = logger;
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "TooltipAI");
        Directory.CreateDirectory(appFolder);

        _blacklistPath = customPath ?? Path.Combine(appFolder, "blacklist.json");
        _blacklist = LoadBlacklist();

        try
        {
            _watcher = new FileSystemWatcher(Path.GetDirectoryName(_blacklistPath)!, Path.GetFileName(_blacklistPath))
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };
            _watcher.Changed += OnBlacklistChanged;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Blacklist file watcher is unavailable for {Path}", _blacklistPath);
            if (_logger is null)
                Trace.TraceWarning($"Blacklist file watcher is unavailable for '{_blacklistPath}': {ex}");
        }
    }

    public bool IsBlacklisted(string processName)
    {
        lock (_lock)
        {
            return _blacklist.Any(app =>
                processName.Contains(app, StringComparison.OrdinalIgnoreCase));
        }
    }

    public void Add(string processName)
    {
        lock (_lock)
        {
            if (!_blacklist.Contains(processName))
            {
                var updated = new List<string>(_blacklist) { processName };
                SaveBlacklist(updated);
                _blacklist = updated;
            }
        }
        BlacklistChanged?.Invoke(Blacklist);
    }

    public void Remove(string processName)
    {
        lock (_lock)
        {
            var updated = new List<string>(_blacklist);
            updated.Remove(processName);
            SaveBlacklist(updated);
            _blacklist = updated;
        }
        BlacklistChanged?.Invoke(Blacklist);
    }

    public void SetBlacklist(List<string> apps)
    {
        lock (_lock)
        {
            var updated = new List<string>(apps);
            SaveBlacklist(updated);
            _blacklist = updated;
        }
        BlacklistChanged?.Invoke(Blacklist);
    }

    public void Clear()
    {
        lock (_lock)
        {
            var updated = new List<string>();
            SaveBlacklist(updated);
            _blacklist = updated;
        }
        BlacklistChanged?.Invoke(Blacklist);
    }

    private List<string> LoadBlacklist()
    {
        try
        {
            if (File.Exists(_blacklistPath))
            {
                var json = File.ReadAllText(_blacklistPath);
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load blacklist from {Path}", _blacklistPath);
            if (_logger is null)
                Trace.TraceError($"Failed to load blacklist from '{_blacklistPath}': {ex}");
        }
        return new List<string>();
    }

    private void SaveBlacklist(List<string> blacklist)
    {
        try
        {
            var json = JsonSerializer.Serialize(blacklist, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_blacklistPath, json);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to save blacklist to {Path}", _blacklistPath);
            if (_logger is null)
                Trace.TraceError($"Failed to save blacklist to '{_blacklistPath}': {ex}");
            throw;
        }
    }

    private void OnBlacklistChanged(object sender, FileSystemEventArgs e)
    {
        lock (_lock)
        {
            _blacklist = LoadBlacklist();
        }
        BlacklistChanged?.Invoke(Blacklist);
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }
}

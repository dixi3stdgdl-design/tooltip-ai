using Microsoft.Extensions.Logging;
using TooltipAI.Core.Common;

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
        var appFolder = AppDataPaths.EnsureRoot();

        _blacklistPath = customPath ?? Path.Combine(appFolder, "blacklist.json");
        _blacklist = LoadBlacklist();

        _watcher = FileChangeWatcher.TryWatch(_blacklistPath, OnBlacklistChanged);
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
                _blacklist.Add(processName);
                SaveBlacklist(_blacklist);
            }
        }
        BlacklistChanged?.Invoke(Blacklist);
    }

    public void Remove(string processName)
    {
        lock (_lock)
        {
            _blacklist.Remove(processName);
            SaveBlacklist(_blacklist);
        }
        BlacklistChanged?.Invoke(Blacklist);
    }

    public void SetBlacklist(List<string> apps)
    {
        lock (_lock)
        {
            _blacklist = new List<string>(apps);
            SaveBlacklist(_blacklist);
        }
        BlacklistChanged?.Invoke(Blacklist);
    }

    public void Clear()
    {
        lock (_lock)
        {
            _blacklist.Clear();
            SaveBlacklist(_blacklist);
        }
        BlacklistChanged?.Invoke(Blacklist);
    }

    private List<string> LoadBlacklist()
        => JsonFile.Load(_blacklistPath, () => new List<string>(), _logger, description: "blacklist");

    private void SaveBlacklist(List<string> blacklist)
        => JsonFile.Save(_blacklistPath, blacklist, _logger, description: "blacklist");

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

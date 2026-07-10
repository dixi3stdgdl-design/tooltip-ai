using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace TooltipAI.Core.Services;

public class UsageMeteringService : IDisposable
{
    private readonly string _usagePath;
    private readonly object _lock = new();
    private UsageData _data;
    private readonly FileSystemWatcher? _watcher;
    private readonly ILogger? _logger;

    public int DailyUsage => _data.DailyCount;
    public int DailyLimit => _data.DailyLimit;
    public bool IsLimitReached => _data.DailyCount >= _data.DailyLimit;

    public UsageMeteringService(string? customPath = null, ILogger? logger = null)
    {
        _logger = logger;
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "TooltipAI");
        Directory.CreateDirectory(appFolder);

        _usagePath = customPath ?? Path.Combine(appFolder, "usage.json");
        _data = LoadData();

        // Reset daily count if it's a new day
        if (_data.LastResetDate.Date < DateTime.UtcNow.Date)
        {
            ResetDailyCount();
        }

        try
        {
            _watcher = new FileSystemWatcher(Path.GetDirectoryName(_usagePath)!, Path.GetFileName(_usagePath))
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };
            _watcher.Changed += OnUsageChanged;
        }
        catch
        {
            // FileSystemWatcher not available in all contexts
        }
    }

    public void IncrementUsage()
    {
        lock (_lock)
        {
            // Reset if new day
            if (_data.LastResetDate.Date < DateTime.UtcNow.Date)
            {
                _data.DailyCount = 0;
                _data.LastResetDate = DateTime.UtcNow;
            }

            _data.DailyCount++;
            _data.TotalCount++;
            SaveData(_data);
        }
    }

    public bool CanUse()
    {
        lock (_lock)
        {
            // Reset if new day
            if (_data.LastResetDate.Date < DateTime.UtcNow.Date)
            {
                _data.DailyCount = 0;
                _data.LastResetDate = DateTime.UtcNow;
                SaveData(_data);
            }

            return _data.DailyCount < _data.DailyLimit;
        }
    }

    public void SetDailyLimit(int limit)
    {
        lock (_lock)
        {
            _data.DailyLimit = limit;
            SaveData(_data);
        }
    }

    public void ResetDailyCount()
    {
        lock (_lock)
        {
            _data.DailyCount = 0;
            _data.LastResetDate = DateTime.UtcNow;
            SaveData(_data);
        }
    }

    public void ResetAll()
    {
        lock (_lock)
        {
            _data = new UsageData();
            SaveData(_data);
        }
    }

    public UsageStats GetStats()
    {
        lock (_lock)
        {
            return new UsageStats
            {
                DailyCount = _data.DailyCount,
                DailyLimit = _data.DailyLimit,
                TotalCount = _data.TotalCount,
                LastResetDate = _data.LastResetDate,
                RemainingToday = Math.Max(0, _data.DailyLimit - _data.DailyCount)
            };
        }
    }

    private UsageData LoadData()
    {
        try
        {
            if (File.Exists(_usagePath))
            {
                var json = File.ReadAllText(_usagePath);
                return JsonSerializer.Deserialize<UsageData>(json) ?? new UsageData();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load usage data from {Path}", _usagePath);
        }
        return new UsageData();
    }

    private void SaveData(UsageData data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_usagePath, json);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to save usage data to {Path}", _usagePath);
        }
    }

    private void OnUsageChanged(object sender, FileSystemEventArgs e)
    {
        lock (_lock)
        {
            _data = LoadData();
        }
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }
}

public class UsageData
{
    public int DailyCount { get; set; } = 0;
    public int DailyLimit { get; set; } = 10;
    public long TotalCount { get; set; } = 0;
    public DateTime LastResetDate { get; set; } = DateTime.UtcNow;
}

public class UsageStats
{
    public int DailyCount { get; init; }
    public int DailyLimit { get; init; }
    public long TotalCount { get; init; }
    public DateTime LastResetDate { get; init; }
    public int RemainingToday { get; init; }
}

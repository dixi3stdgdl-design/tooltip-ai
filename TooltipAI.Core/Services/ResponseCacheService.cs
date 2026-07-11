using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TooltipAI.Core.Models;

namespace TooltipAI.Core.Services;

public class ResponseCacheService : IDisposable
{
    private readonly string _dbPath;
    private readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(5);
    private readonly int _maxEntries = 1000;
    private readonly object _lock = new();
    private SqliteConnection? _connection;
    private readonly ILogger? _logger;

    private long _cacheHits;
    private long _cacheMisses;

    public int EntryCount => GetEntryCount();
    public long CacheHits => Interlocked.Read(ref _cacheHits);
    public long CacheMisses => Interlocked.Read(ref _cacheMisses);
    public double HitRate => (_cacheHits + _cacheMisses) == 0 ? 0.0 : (double)_cacheHits / (_cacheHits + _cacheMisses);

    public ResponseCacheService(string? customPath = null, ILogger? logger = null)
    {
        _logger = logger;
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "TooltipAI");
        Directory.CreateDirectory(appFolder);

        _dbPath = customPath ?? Path.Combine(appFolder, "cache.db");
        InitializeDatabase();
    }

    public TooltipData? Get(string key)
    {
        lock (_lock)
        {
            try
            {
                using var command = _connection!.CreateCommand();
                command.CommandText = @"
                    SELECT data, expires_at FROM cache
                    WHERE key = @key AND expires_at > @now";

                command.Parameters.AddWithValue("@key", key);
                command.Parameters.AddWithValue("@now", DateTime.UtcNow.Ticks);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    Interlocked.Increment(ref _cacheHits);

                    var json = reader.GetString(0);

                    // Update last_access_at for LRU tracking
                    using var updateCommand = _connection!.CreateCommand();
                    updateCommand.CommandText = "UPDATE cache SET last_access_at = @now WHERE key = @key";
                    updateCommand.Parameters.AddWithValue("@now", DateTime.UtcNow.Ticks);
                    updateCommand.Parameters.AddWithValue("@key", key);
                    updateCommand.ExecuteNonQuery();

                    return JsonSerializer.Deserialize<TooltipData>(json);
                }

                Interlocked.Increment(ref _cacheMisses);
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _cacheMisses);
                _logger?.LogError(ex, "Cache Get failed for key: {Key}", key);
            }
        }
        return null;
    }

    public void Set(string key, TooltipData value, TimeSpan? ttl = null)
    {
        lock (_lock)
        {
            try
            {
                var expiry = DateTime.UtcNow + (ttl ?? _defaultTtl);
                var now = DateTime.UtcNow.Ticks;
                var json = JsonSerializer.Serialize(value);

                using var command = _connection!.CreateCommand();
                command.CommandText = @"
                    INSERT OR REPLACE INTO cache (key, data, expires_at, created_at, last_access_at)
                    VALUES (@key, @data, @expires_at, @created_at, @last_access_at)";

                command.Parameters.AddWithValue("@key", key);
                command.Parameters.AddWithValue("@data", json);
                command.Parameters.AddWithValue("@expires_at", expiry.Ticks);
                command.Parameters.AddWithValue("@created_at", now);
                command.Parameters.AddWithValue("@last_access_at", now);

                command.ExecuteNonQuery();

                CleanupExpiredEntries();
                EnforceMaxEntries();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Cache Set failed for key: {Key}", key);
            }
        }
    }

    public void Remove(string key)
    {
        lock (_lock)
        {
            try
            {
                using var command = _connection!.CreateCommand();
                command.CommandText = "DELETE FROM cache WHERE key = @key";
                command.Parameters.AddWithValue("@key", key);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Cache Remove failed for key: {Key}", key);
            }
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            try
            {
                using var command = _connection!.CreateCommand();
                command.CommandText = "DELETE FROM cache";
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Cache Clear failed");
            }
        }
    }

    public CacheStats GetStats()
    {
        lock (_lock)
        {
            try
            {
                using var command = _connection!.CreateCommand();
                command.CommandText = @"
                    SELECT
                        COUNT(*) as total,
                        SUM(CASE WHEN expires_at > @now THEN 1 ELSE 0 END) as active,
                        SUM(CASE WHEN expires_at <= @now THEN 1 ELSE 0 END) as expired
                    FROM cache";
                command.Parameters.AddWithValue("@now", DateTime.UtcNow.Ticks);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new CacheStats
                    {
                        TotalEntries = reader.GetInt32(0),
                        ActiveEntries = reader.GetInt32(1),
                        ExpiredEntries = reader.GetInt32(2),
                        CacheHits = CacheHits,
                        CacheMisses = CacheMisses,
                        HitRate = HitRate
                    };
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Cache GetStats failed");
            }
        }
        return new CacheStats();
    }

    public string GenerateKey(ElementInfo element)
    {
        return $"{element.ProcessName}:{element.Name}:{element.ControlType}:{element.ClassName}";
    }

    private void InitializeDatabase()
    {
        lock (_lock)
        {
            try
            {
                _connection = new SqliteConnection($"Data Source={_dbPath}");
                _connection.Open();

                using var command = _connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS cache (
                        key TEXT PRIMARY KEY,
                        data TEXT NOT NULL,
                        expires_at INTEGER NOT NULL,
                        created_at INTEGER NOT NULL,
                        last_access_at INTEGER NOT NULL
                    )";
                command.ExecuteNonQuery();

                using var indexCommand = _connection.CreateCommand();
                indexCommand.CommandText = "CREATE INDEX IF NOT EXISTS idx_expires ON cache(expires_at)";
                indexCommand.ExecuteNonQuery();

                using var accessIndexCommand = _connection.CreateCommand();
                accessIndexCommand.CommandText = "CREATE INDEX IF NOT EXISTS idx_last_access ON cache(last_access_at)";
                accessIndexCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Cache database initialization failed at path: {Path}", _dbPath);
            }
        }
    }

    private void CleanupExpiredEntries()
    {
        try
        {
            using var command = _connection!.CreateCommand();
            command.CommandText = "DELETE FROM cache WHERE expires_at <= @now";
            command.Parameters.AddWithValue("@now", DateTime.UtcNow.Ticks);
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Cache expired entries cleanup failed");
        }
    }

    private void EnforceMaxEntries()
    {
        try
        {
            using var command = _connection!.CreateCommand();
            command.CommandText = @"
                DELETE FROM cache WHERE key IN (
                    SELECT key FROM cache
                    ORDER BY last_access_at ASC
                    LIMIT (SELECT MAX(COUNT(*) - @max, 0) FROM cache)
                )";
            command.Parameters.AddWithValue("@max", _maxEntries);
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Cache LRU eviction failed");
        }
    }

    private int GetEntryCount()
    {
        lock (_lock)
        {
            try
            {
                using var command = _connection!.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM cache";
                return Convert.ToInt32(command.ExecuteScalar());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Cache entry count failed");
                return 0;
            }
        }
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}

public class CacheStats
{
    public int TotalEntries { get; set; }
    public int ActiveEntries { get; set; }
    public int ExpiredEntries { get; set; }
    public long CacheHits { get; set; }
    public long CacheMisses { get; set; }
    public double HitRate { get; set; }
}

using Microsoft.Data.Sqlite;
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

    public int EntryCount => GetEntryCount();

    public ResponseCacheService(string? customPath = null)
    {
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
                    var json = reader.GetString(0);
                    return JsonSerializer.Deserialize<TooltipData>(json);
                }
            }
            catch
            {
                // Return null on error
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
                var json = JsonSerializer.Serialize(value);

                using var command = _connection!.CreateCommand();
                command.CommandText = @"
                    INSERT OR REPLACE INTO cache (key, data, expires_at, created_at)
                    VALUES (@key, @data, @expires_at, @created_at)";

                command.Parameters.AddWithValue("@key", key);
                command.Parameters.AddWithValue("@data", json);
                command.Parameters.AddWithValue("@expires_at", expiry.Ticks);
                command.Parameters.AddWithValue("@created_at", DateTime.UtcNow.Ticks);

                command.ExecuteNonQuery();

                CleanupExpiredEntries();
                EnforceMaxEntries();
            }
            catch
            {
                // Silently fail on cache errors
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
            catch
            {
                // Silently fail
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
            catch
            {
                // Silently fail
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
                        ExpiredEntries = reader.GetInt32(2)
                    };
                }
            }
            catch
            {
                // Return empty stats on error
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
                        created_at INTEGER NOT NULL
                    )";
                command.ExecuteNonQuery();

                using var indexCommand = _connection.CreateCommand();
                indexCommand.CommandText = "CREATE INDEX IF NOT EXISTS idx_expires ON cache(expires_at)";
                indexCommand.ExecuteNonQuery();
            }
            catch
            {
                // Database initialization failed - cache will be disabled
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
        catch
        {
            // Silently fail
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
                    ORDER BY created_at ASC
                    LIMIT (SELECT MAX(COUNT(*) - @max, 0) FROM cache)
                )";
            command.Parameters.AddWithValue("@max", _maxEntries);
            command.ExecuteNonQuery();
        }
        catch
        {
            // Silently fail
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
            catch
            {
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
}

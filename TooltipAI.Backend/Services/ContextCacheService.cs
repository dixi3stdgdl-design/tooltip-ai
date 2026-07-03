using System.Collections.Concurrent;
using System.Text.Json;
using TooltipAI.Backend.Models;

namespace TooltipAI.Backend.Services;

public class ContextCacheService
{
    private readonly ILogger<ContextCacheService> _logger;
    private readonly ConcurrentDictionary<string, ContextEntry> _cache = new();
    private readonly TimeSpan _defaultTtl = TimeSpan.FromHours(24);

    public ContextCacheService(ILogger<ContextCacheService> logger)
    {
        _logger = logger;
    }

    public async Task<ContextEntry?> GetAsync(string key)
    {
        if (_cache.TryGetValue(key, out var entry) && entry.CachedAt.Add(_defaultTtl) > DateTime.UtcNow)
        {
            _logger.LogDebug("Cache hit: {Key}", key);
            await Task.CompletedTask;
            return entry;
        }

        _cache.TryRemove(key, out _);
        return null;
    }

    public async Task SetAsync(string key, string value, string source = "local")
    {
        var entry = new ContextEntry
        {
            Key = key,
            Value = value,
            Source = source,
            CachedAt = DateTime.UtcNow,
            HitCount = 0
        };

        _cache[key] = entry;
        _logger.LogDebug("Cache set: {Key} from {Source}", key, source);
        await Task.CompletedTask;
    }

    public async Task<int> GetHitCountAsync(string key)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            await Task.CompletedTask;
            return entry.HitCount;
        }

        await Task.CompletedTask;
        return 0;
    }

    public async Task<Dictionary<string, int>> GetStatsAsync()
    {
        await Task.CompletedTask;
        return new Dictionary<string, int>
        {
            ["total_entries"] = _cache.Count,
            ["active_entries"] = _cache.Values.Count(e => e.CachedAt.Add(_defaultTtl) > DateTime.UtcNow),
            ["expired_entries"] = _cache.Values.Count(e => e.CachedAt.Add(_defaultTtl) <= DateTime.UtcNow)
        };
    }

    public async Task<int> CleanupAsync()
    {
        var expiredKeys = _cache
            .Where(kvp => kvp.Value.CachedAt.Add(_defaultTtl) <= DateTime.UtcNow)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _cache.TryRemove(key, out _);
        }

        _logger.LogInformation("Cleaned up {Count} expired cache entries", expiredKeys.Count);
        await Task.CompletedTask;
        return expiredKeys.Count;
    }
}

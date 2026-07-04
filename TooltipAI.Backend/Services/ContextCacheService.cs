using System.Collections.Concurrent;
using TooltipAI.Backend.Models;

namespace TooltipAI.Backend.Services;

public sealed class ContextCacheService
{
    private readonly ILogger<ContextCacheService> _logger;
    private readonly ConcurrentDictionary<string, ContextEntry> _cache = new();
    private long _totalHits;
    private long _totalMisses;

    public ContextCacheService(ILogger<ContextCacheService> logger)
    {
        _logger = logger;
    }

    public ContextEntry? Get(string key)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.ExpiresAt > DateTime.UtcNow)
            {
                Interlocked.Increment(ref _totalHits);
                return entry;
            }

            _cache.TryRemove(key, out _);
        }

        Interlocked.Increment(ref _totalMisses);
        return null;
    }

    public void Set(ContextCacheRequest request)
    {
        var entry = new ContextEntry
        {
            Key = request.Key,
            ElementName = request.ElementName,
            ElementType = request.ElementType,
            ApplicationName = request.ApplicationName,
            Context = request.Context,
            Tags = request.Tags,
            Confidence = request.Confidence,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddSeconds(request.TtlSeconds)
        };

        _cache.AddOrUpdate(request.Key, entry, (_, _) => entry);
        _logger.LogDebug("Context cached: {Key}, Expires: {Expires}", request.Key, entry.ExpiresAt);
    }

    public ContextCacheStats GetStats()
    {
        CleanupExpired();

        var total = _totalHits + _totalMisses;
        return new ContextCacheStats
        {
            TotalEntries = _cache.Count,
            ActiveEntries = _cache.Values.Count(e => e.ExpiresAt > DateTime.UtcNow),
            ExpiredEntries = _cache.Values.Count(e => e.ExpiresAt <= DateTime.UtcNow),
            TotalHits = _totalHits,
            TotalMisses = _totalMisses,
            HitRate = total > 0 ? (double)_totalHits / total * 100 : 0
        };
    }

    private void CleanupExpired()
    {
        var expiredKeys = _cache
            .Where(kvp => kvp.Value.ExpiresAt <= DateTime.UtcNow)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _cache.TryRemove(key, out _);
        }
    }
}

namespace TooltipAI.Backend.Models;

public sealed class ContextEntry
{
    public string Key { get; init; } = string.Empty;
    public string ElementName { get; init; } = string.Empty;
    public string ElementType { get; init; } = string.Empty;
    public string ApplicationName { get; init; } = string.Empty;
    public string Context { get; init; } = string.Empty;
    public string[] Tags { get; init; } = Array.Empty<string>();
    public int Confidence { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
}

public sealed class ContextCacheRequest
{
    public string Key { get; init; } = string.Empty;
    public string ElementName { get; init; } = string.Empty;
    public string ElementType { get; init; } = string.Empty;
    public string ApplicationName { get; init; } = string.Empty;
    public string Context { get; init; } = string.Empty;
    public string[] Tags { get; init; } = Array.Empty<string>();
    public int Confidence { get; init; }
    public int TtlSeconds { get; init; } = 3600;
}

public sealed class ContextCacheStats
{
    public int TotalEntries { get; init; }
    public int ActiveEntries { get; init; }
    public int ExpiredEntries { get; init; }
    public long TotalHits { get; init; }
    public long TotalMisses { get; init; }
    public double HitRate { get; init; }
}

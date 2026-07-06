using System.ComponentModel.DataAnnotations;

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
    [Required(ErrorMessage = "Key is required")]
    [StringLength(256, MinimumLength = 1, ErrorMessage = "Key must be between 1 and 256 characters")]
    public string Key { get; init; } = string.Empty;

    [StringLength(256, ErrorMessage = "ElementName must be at most 256 characters")]
    public string ElementName { get; init; } = string.Empty;

    [StringLength(64, ErrorMessage = "ElementType must be at most 64 characters")]
    public string ElementType { get; init; } = string.Empty;

    [StringLength(128, ErrorMessage = "ApplicationName must be at most 128 characters")]
    public string ApplicationName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Context is required")]
    [StringLength(4096, MinimumLength = 1, ErrorMessage = "Context must be between 1 and 4096 characters")]
    public string Context { get; init; } = string.Empty;

    public string[] Tags { get; init; } = Array.Empty<string>();

    [Range(0, 100, ErrorMessage = "Confidence must be between 0 and 100")]
    public int Confidence { get; init; }

    [Range(60, 86400, ErrorMessage = "TtlSeconds must be between 60 and 86400")]
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

namespace TooltipAI.Backend.Models;

public record LicenseRequest(
    string MachineId,
    string LicenseKey,
    string AppVersion
);

public record LicenseResponse(
    bool Valid,
    string? LicenseId,
    DateTime? ExpiryDate,
    string? Plan,
    int DailyRequestsRemaining,
    string Message
);

public record LicenseInfo
{
    public string LicenseId { get; init; } = string.Empty;
    public string MachineId { get; init; } = string.Empty;
    public string LicenseKey { get; init; } = string.Empty;
    public string Plan { get; init; } = "free";
    public DateTime CreatedAt { get; init; }
    public DateTime ExpiryDate { get; init; }
    public int DailyRequestLimit { get; init; }
    public bool IsActive { get; init; }
}

public record PluginManifest
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public string DownloadUrl { get; init; } = string.Empty;
    public string Hash { get; init; } = string.Empty;
    public DateTime PublishedAt { get; init; }
    public string[] Tags { get; init; } = [];
    public string MinAppVersion { get; init; } = string.Empty;
}

public record ContextEntry
{
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public DateTime CachedAt { get; init; }
    public int HitCount { get; init; }
}

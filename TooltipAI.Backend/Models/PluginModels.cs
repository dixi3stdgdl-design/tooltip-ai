namespace TooltipAI.Backend.Models;

public sealed class PluginInfo
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public string DownloadUrl { get; init; } = string.Empty;
    public string Sha256Hash { get; init; } = string.Empty;
    public int MinAppVersion { get; init; }
    public DateTime PublishedAt { get; init; }
    public int Downloads { get; init; }
    public bool IsOfficial { get; init; }
}

public sealed class PluginRegisterRequest
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public string DownloadUrl { get; init; } = string.Empty;
    public string Sha256Hash { get; init; } = string.Empty;
    public int MinAppVersion { get; init; }
}

public sealed class PluginRegistryStats
{
    public int TotalPlugins { get; init; }
    public int OfficialPlugins { get; init; }
    public int CommunityPlugins { get; init; }
    public long TotalDownloads { get; init; }
}

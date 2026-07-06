using System.ComponentModel.DataAnnotations;

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
    [Required(ErrorMessage = "Id is required")]
    [StringLength(128, MinimumLength = 1, ErrorMessage = "Id must be between 1 and 128 characters")]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "Id can only contain alphanumeric characters, dots, hyphens, and underscores")]
    public string Id { get; init; } = string.Empty;

    [Required(ErrorMessage = "Name is required")]
    [StringLength(128, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 128 characters")]
    public string Name { get; init; } = string.Empty;

    [StringLength(1024, ErrorMessage = "Description must be at most 1024 characters")]
    public string Description { get; init; } = string.Empty;

    [Required(ErrorMessage = "Version is required")]
    [RegularExpression(@"^\d+\.\d+\.\d+$", ErrorMessage = "Version must be in semver format (e.g., 1.0.0)")]
    public string Version { get; init; } = string.Empty;

    [StringLength(128, ErrorMessage = "Author must be at most 128 characters")]
    public string Author { get; init; } = string.Empty;

    [Required(ErrorMessage = "DownloadUrl is required")]
    [Url(ErrorMessage = "DownloadUrl must be a valid URL")]
    public string DownloadUrl { get; init; } = string.Empty;

    [Required(ErrorMessage = "Sha256Hash is required")]
    [RegularExpression(@"^[a-fA-F0-9]{64}$", ErrorMessage = "Sha256Hash must be a valid SHA-256 hash")]
    public string Sha256Hash { get; init; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "MinAppVersion must be non-negative")]
    public int MinAppVersion { get; init; }
}

public sealed class PluginRegistryStats
{
    public int TotalPlugins { get; init; }
    public int OfficialPlugins { get; init; }
    public int CommunityPlugins { get; init; }
    public long TotalDownloads { get; init; }
}

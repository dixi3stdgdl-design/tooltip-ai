using System.ComponentModel.DataAnnotations;

namespace TooltipAI.Backend.Models;

public sealed class LicenseValidateRequest
{
    [Required(ErrorMessage = "LicenseKey is required")]
    [StringLength(64, MinimumLength = 8, ErrorMessage = "LicenseKey must be between 8 and 64 characters")]
    public string LicenseKey { get; init; } = string.Empty;

    [Required(ErrorMessage = "MachineId is required")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "MachineId must be between 8 and 128 characters")]
    public string MachineId { get; init; } = string.Empty;

    [StringLength(32, ErrorMessage = "AppVersion must be at most 32 characters")]
    public string AppVersion { get; init; } = string.Empty;
}

public sealed class LicenseValidateResponse
{
    public bool Valid { get; init; }
    public string Tier { get; init; } = string.Empty;
    public DateTime? ExpiresAt { get; init; }
    public string? Error { get; init; }
    public int DaysRemaining { get; init; }
}

public sealed record LicenseInfo
{
    public string LicenseId { get; init; } = string.Empty;
    public string LicenseKey { get; init; } = string.Empty;
    public string Tier { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
    public string MachineId { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

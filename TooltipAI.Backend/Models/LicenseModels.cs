namespace TooltipAI.Backend.Models;

public sealed class LicenseValidateRequest
{
    public string LicenseKey { get; init; } = string.Empty;
    public string MachineId { get; init; } = string.Empty;
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

public sealed class LicenseInfo
{
    public string LicenseId { get; init; } = string.Empty;
    public string LicenseKey { get; init; } = string.Empty;
    public string Tier { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
    public string MachineId { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

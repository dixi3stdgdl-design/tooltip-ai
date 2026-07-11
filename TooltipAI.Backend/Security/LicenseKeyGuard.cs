namespace TooltipAI.Backend.Services;

/// <summary>
/// Detects missing or well-known insecure default license HMAC keys.
/// A weak key allows anyone to forge valid, signed license keys.
/// </summary>
public static class LicenseKeyGuard
{
    private static readonly HashSet<string> InsecureKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "dev-hmac-key-change-in-production",
        "CHANGE_ME_TO_A_SECURE_KEY_IN_PRODUCTION",
        "CHANGE_ME",
        "TooltipAI-DefaultKey-Change-Me",
    };

    public static bool IsInsecure(string? key)
        => string.IsNullOrWhiteSpace(key) || InsecureKeys.Contains(key.Trim());
}

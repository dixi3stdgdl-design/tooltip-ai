namespace TooltipAI.Core.Models;

public enum AiComplexityLevel
{
    None = 0,
    Basic = 1,
    Standard = 2,
    Complex = 3
}

public class AiComplexityConfig
{
    public AiComplexityLevel DefaultLevel { get; set; } = AiComplexityLevel.Basic;
    public int BasicThresholdMs { get; set; } = 100;
    public int StandardThresholdMs { get; set; } = 500;
    public int ComplexThresholdMs { get; set; } = 1000;
    public bool EnableCloudApi { get; set; } = true;
    public string? ApiEndpoint { get; set; }
    public string? ApiKey { get; set; }
    public string? ModelName { get; set; }
    public int ApiTimeoutMs { get; set; } = 3000;
    public int CacheExpirationMinutes { get; set; } = 5;
    public int MaxCacheSize { get; set; } = 1000;
}

public class AiComplexityResult
{
    public AiComplexityLevel Level { get; set; }
    public string? LocalContext { get; set; }
    public bool RequiresCloudApi { get; set; }
    public string? CacheKey { get; set; }
}

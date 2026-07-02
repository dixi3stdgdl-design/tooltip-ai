namespace TooltipAI.Core.Models;

/// <summary>
/// Configuration for local context enrichment.
/// NO cloud API settings - all processing is local.
/// </summary>
public class ContextEnricherConfig
{
    public int CacheExpirationMinutes { get; set; } = 5;
    public int MaxCacheSize { get; set; } = 1000;
    public bool EnableEnrichedContext { get; set; } = true;
    public bool EnableGestureHints { get; set; } = true;
    public bool EnableQualityTips { get; set; } = true;
    public bool EnableMoveGuides { get; set; } = true;
    public bool EnableDataInsights { get; set; } = true;
}

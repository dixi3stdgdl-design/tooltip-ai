namespace TooltipAI.Core.Models;

/// <summary>
/// Tooltip data model - contextual intelligence overlay for native tooltips.
/// NO external APIs - all processing is local via UI Automation.
/// </summary>
public class TooltipData
{
    public ElementInfo Element { get; set; } = new();

    // Enriched context fields (replaces AI-specific fields)
    public string? EnrichedContext { get; set; }
    public string? FunctionHint { get; set; }
    public string? UsageContext { get; set; }
    public bool HasEnrichedContext => !string.IsNullOrEmpty(EnrichedContext);

    // Software classification (local, no API)
    public string? SoftwareCategory { get; set; }
    public string? CategoryLabel { get; set; }

    // Interaction hints (local processing)
    public string? GestureHint { get; set; }
    public string? QualityTip { get; set; }
    public string? MoveGuide { get; set; }
    public string? DataInsight { get; set; }

    // Window context (from UI Automation)
    public string? ProcessName { get; set; }
    public string? WindowTitle { get; set; }

    // Visual theme colors (by software category)
    public uint BorderColor { get; set; } = 0x00607D8B;
    public uint AccentColor { get; set; } = 0x00607D8B;
    public uint GlowColor { get; set; } = 0x00455A64;

    // Visual type for renderer dispatch
    public int VisualType { get; set; }
}

namespace TooltipAI.Core.Interfaces;

/// <summary>
/// Plugin interface for custom tooltip providers.
/// Implement this interface to create a plugin that provides
/// contextual tooltips for specific applications.
/// </summary>
public interface ITooltipProvider
{
    /// <summary>Display name of this plugin.</summary>
    string Name { get; }

    /// <summary>Process names this plugin supports (e.g., "ableton", "chrome").</summary>
    string[] SupportedProcesses { get; }

    /// <summary>Priority when multiple plugins match (higher = preferred).</summary>
    int Priority => 0;

    /// <summary>Get tooltip data for the given element.</summary>
    Task<TooltipData?> GetTooltipAsync(ElementInfo element, CancellationToken ct = default);
}

/// <summary>
/// Minimal element info passed to plugins.
/// </summary>
public record ElementInfo
{
    public string ClassName { get; init; } = "";
    public string WindowTitle { get; init; } = "";
    public string ProcessName { get; init; } = "";
    public string ControlType { get; init; } = "Window";
    public bool IsEnabled { get; init; } = true;
    public bool IsKeyboardFocusable { get; init; }
    public string HelpText { get; init; } = "";
}

/// <summary>
/// Tooltip data returned by providers.
/// </summary>
public record TooltipData
{
    public string Category { get; init; } = "Unknown";
    public string ModuleName { get; init; } = "";
    public string ProcessName { get; init; } = "";
    public string Context { get; init; } = "";
    public string Description { get; init; } = "";
    public string? GestureHint { get; init; }
    public string? QualityTip { get; init; }
    public string? MoveGuide { get; init; }
    public string? DataInsight { get; init; }
    public string ColorHex { get; init; } = "#6366F1";
}

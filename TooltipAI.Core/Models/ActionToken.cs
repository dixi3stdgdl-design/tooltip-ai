namespace TooltipAI.Core.Models;

/// <summary>
/// Action token returned by AI inference.
/// Represents an executable UI action.
/// </summary>
public class ActionToken
{
    /// <summary>
    /// Action type: INVOKE, SELECT, TYPE, TOGGLE, NAVIGATE, SCROLL
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Target element identifier (AutomationId, Name, or index).
    /// </summary>
    public string Target { get; set; } = string.Empty;

    /// <summary>
    /// Optional text to type (for TYPE action).
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Optional value to set (for slider, combobox).
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Confidence score (0.0 to 1.0).
    /// </summary>
    public float Confidence { get; set; }

    /// <summary>
    /// Human-readable description of the action.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

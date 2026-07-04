namespace TooltipAI.Core.Models;

/// <summary>
/// Combined context for AI inference.
/// Merges screen context with user intent (voice or gaze).
/// </summary>
public class GazeContext
{
    /// <summary>
    /// Application name (e.g., "Excel.exe").
    /// </summary>
    public string App { get; set; } = string.Empty;

    /// <summary>
    /// UI element role (e.g., "Button", "MenuItem").
    /// </summary>
    public string ElementRole { get; set; } = string.Empty;

    /// <summary>
    /// Element name/label.
    /// </summary>
    public string ElementName { get; set; } = string.Empty;

    /// <summary>
    /// Automation ID for precise targeting.
    /// </summary>
    public string AutomationId { get; set; } = string.Empty;

    /// <summary>
    /// Window title for context.
    /// </summary>
    public string WindowTitle { get; set; } = string.Empty;

    /// <summary>
    /// Help text if available.
    /// </summary>
    public string? HelpText { get; set; }

    /// <summary>
    /// User's voice command or gaze intent.
    /// </summary>
    public string UserIntent { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp of context capture.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Serialize to JSON for AI inference.
    /// </summary>
    public string ToJson()
    {
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            context = new
            {
                app = App,
                element_role = ElementRole,
                element_name = ElementName,
                automation_id = AutomationId,
                window_title = WindowTitle,
                help_text = HelpText
            },
            user_intent = UserIntent
        });
    }
}

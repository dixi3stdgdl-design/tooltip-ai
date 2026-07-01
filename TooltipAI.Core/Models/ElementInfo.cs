namespace TooltipAI.Core.Models;

public class ElementInfo
{
    public string Name { get; set; } = string.Empty;
    public string ControlType { get; set; } = string.Empty;
    public string AutomationId { get; set; } = string.Empty;
    public string HelpText { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public bool IsKeyboardFocusable { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

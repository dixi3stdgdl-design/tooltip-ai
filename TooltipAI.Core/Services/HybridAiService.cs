using System.Text;
using TooltipAI.Core.Interfaces;
using TooltipAI.Core.Models;
using ElementInfo = TooltipAI.Core.Models.ElementInfo;

namespace TooltipAI.Core.Services;

/// <summary>
/// Local context enricher - NO external APIs, NO cloud calls.
/// Enriches tooltip data using only UI Automation information.
/// All processing is local and instant (&lt;10ms).
/// </summary>
public class LocalContextEnricher : IContextEnricher
{
    private readonly SoftwareCategoryClassifier _classifier = new();

    public string GetEnrichedContext(ElementInfo element)
    {
        var sb = new StringBuilder();

        // Type information
        sb.Append($"Type: {element.ControlType}");

        // Class information
        if (!string.IsNullOrEmpty(element.ClassName))
            sb.Append($" | Class: {element.ClassName}");

        // Status
        if (element.IsEnabled)
            sb.Append(" | Status: Enabled");
        else
            sb.Append(" | Status: Disabled");

        // Keyboard focus
        if (element.IsKeyboardFocusable)
            sb.Append(" | Keyboard: Focusable");

        // Help text if available
        if (!string.IsNullOrEmpty(element.HelpText))
            sb.Append($" | Help: {element.HelpText}");

        return sb.ToString();
    }

    public string GetFunctionHint(ElementInfo element)
    {
        var controlType = element.ControlType?.ToLower() ?? "";

        return controlType switch
        {
            "button" => "Interactive button - click to activate",
            "edit" => "Text input field - type to enter data",
            "text" => "Static text display - read only",
            "hyperlink" => "Clickable link - click to navigate",
            "image" => "Image element - click for details",
            "slider" => "Value slider - drag to adjust",
            "checkbox" => "Toggle checkbox - click to change state",
            "combobox" => "Dropdown menu - click to expand options",
            "listitem" => "List item - click to select",
            "treeitem" => "Tree item - click to expand",
            "tab" => "Tab control - click to switch view",
            "menu" => "Menu item - click to open",
            "toolbar" => "Toolbar - contains action buttons",
            "scrollbar" => "Scrollbar - drag to scroll content",
            "progress" => "Progress indicator - shows loading status",
            "statusbar" => "Status bar - shows system information",
            _ => $"UI Element: {element.ControlType}"
        };
    }

    public string GetUsageContext(ElementInfo element)
    {
        if (!string.IsNullOrEmpty(element.HelpText))
            return element.HelpText;

        var category = _classifier.Classify(element.ClassName, element.WindowTitle, element.ProcessName);
        return category switch
        {
            SoftwareCategory.Audio => "Audio processing element",
            SoftwareCategory.Creative => "Design/creative tool",
            SoftwareCategory.Development => "Development tool",
            SoftwareCategory.Terminal => "Command-line interface",
            SoftwareCategory.Browser => "Web browser element",
            SoftwareCategory.Video => "Video/media element",
            SoftwareCategory.Gaming => "Gaming interface",
            SoftwareCategory.Office => "Office productivity",
            SoftwareCategory.Security => "Security-related element",
            _ => "Standard UI element"
        };
    }

    public string GetGestureHint(ElementInfo element, SoftwareCategory category)
    {
        var controlType = element.ControlType?.ToLower() ?? "";

        return controlType switch
        {
            "button" => "Click to activate",
            "edit" => "Type to input text",
            "text" => "Read-only element",
            "hyperlink" => "Click to navigate",
            "image" => "Click or hover for details",
            "slider" => "Drag to adjust value",
            "checkbox" => "Click to toggle",
            "combobox" => "Click to expand options",
            "listitem" => "Click to select",
            "treeitem" => "Click to expand",
            "tab" => "Click to switch view",
            "menu" => "Click to open menu",
            "toolbar" => "Contains action buttons",
            "scrollbar" => "Drag to scroll",
            "progress" => "Shows loading progress",
            "statusbar" => "Shows status information",
            _ => GetCategoryGestureHint(category)
        };
    }

    private string GetCategoryGestureHint(SoftwareCategory category)
    {
        return category switch
        {
            SoftwareCategory.Audio => "Hover for audio analysis",
            SoftwareCategory.Creative => "Hover for design context",
            SoftwareCategory.Development => "Hover for code insights",
            SoftwareCategory.Terminal => "Hover for command info",
            SoftwareCategory.Browser => "Hover for page context",
            SoftwareCategory.Video => "Hover for media details",
            SoftwareCategory.Gaming => "Hover for game info",
            _ => "Hover for element details"
        };
    }

    public string GetQualityTip(ElementInfo element, SoftwareCategory category)
    {
        var controlType = element.ControlType?.ToLower() ?? "";

        if (category == SoftwareCategory.Audio)
        {
            if (element.ClassName?.Contains("slider", StringComparison.OrdinalIgnoreCase) == true)
                return "Adjust slowly for precise tuning";
            if (element.ClassName?.Contains("knob", StringComparison.OrdinalIgnoreCase) == true)
                return "Fine-tune with small movements";
            if (controlType == "button")
                return "Toggle for A/B comparison";
        }

        if (category == SoftwareCategory.Creative)
        {
            if (controlType == "slider")
                return "Use for precise adjustments";
            if (controlType == "edit")
                return "Enter exact values for precision";
        }

        if (category == SoftwareCategory.Development)
        {
            if (controlType == "edit")
                return "Code input — check syntax";
            if (controlType == "button")
                return "Action trigger — verify before click";
        }

        return category switch
        {
            SoftwareCategory.Terminal => "Commands are case-sensitive",
            SoftwareCategory.Browser => "Check URL before entering data",
            SoftwareCategory.Office => "Save frequently to prevent data loss",
            SoftwareCategory.Security => "Verify before granting permissions",
            _ => "Standard interaction"
        };
    }

    public string GetMoveGuide(ElementInfo element, SoftwareCategory category)
    {
        var controlType = element.ControlType?.ToLower() ?? "";

        return controlType switch
        {
            "slider" => "Drag vertically or horizontally",
            "scrollbar" => "Drag along the track",
            "splitter" => "Drag to resize panels",
            "thumb" => "Drag to reposition",
            _ => category switch
            {
                SoftwareCategory.Audio => "Move knobs slowly for smooth transitions",
                SoftwareCategory.Creative => "Click and drag for direct manipulation",
                SoftwareCategory.Development => "Select text to copy or edit",
                SoftwareCategory.Terminal => "Click to focus, then type commands",
                _ => "Click to interact"
            }
        };
    }

    public string GetDataInsight(ElementInfo element, SoftwareCategory category)
    {
        var controlType = element.ControlType?.ToLower() ?? "";

        if (!string.IsNullOrEmpty(element.HelpText))
            return element.HelpText;

        if (category == SoftwareCategory.Audio)
            return "Audio processing active";

        if (category == SoftwareCategory.Development)
            return "Build status: Ready";

        if (category == SoftwareCategory.Browser)
            return "Page loaded";

        return controlType switch
        {
            "progress" => "Loading in progress...",
            "statusbar" => "System operational",
            "text" => "Static content",
            _ => "Element active"
        };
    }
}

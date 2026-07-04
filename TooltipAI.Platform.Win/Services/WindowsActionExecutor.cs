using System.Diagnostics;
using System.Runtime.InteropServices;
using TooltipAI.Core.Interfaces;
using TooltipAI.Core.Models;

namespace TooltipAI.Platform.Win.Services;

/// <summary>
/// Windows action executor using UI Automation patterns.
/// Executes actions on UI elements natively without user interaction.
/// </summary>
public sealed class WindowsActionExecutor : IActionExecutor
{
    [DllImport("uiautomationcore.dll")]
    private static extern int UiaGetRuntimeId(IntPtr element, out IntPtr runtimeId);

    public Task<bool> ExecuteActionAsync(ElementInfo element, ActionToken action)
    {
        try
        {
            // Find the UI element by automation ID or name
            var uiaElement = FindElement(element);
            if (uiaElement == null)
            {
                Debug.WriteLine($"Element not found: {element.AutomationId}");
                return Task.FromResult(false);
            }

            // Execute action based on type
            return action.Action.ToUpperInvariant() switch
            {
                "INVOKE" => InvokeElement(uiaElement),
                "SELECT" => SelectElement(uiaElement),
                "TOGGLE" => ToggleElement(uiaElement),
                "TYPE" => TypeText(uiaElement, action.Text ?? string.Empty),
                "SCROLL" => ScrollElement(uiaElement, action.Value),
                _ => Task.FromResult(false)
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ActionExecutor error: {ex.Message}");
            return Task.FromResult(false);
        }
    }

    public IReadOnlyList<string> GetAvailableActions(ElementInfo element)
    {
        var actions = new List<string>();

        var controlType = element.ControlType?.ToLower() ?? "";

        switch (controlType)
        {
            case "button":
            case "menuitem":
            case "hyperlink":
                actions.Add("INVOKE");
                break;
            case "edit":
            case "textfield":
                actions.Add("TYPE");
                actions.Add("SELECT");
                break;
            case "checkbox":
            case "radiobutton":
            case "toggle":
                actions.Add("TOGGLE");
                break;
            case "listitem":
            case "combobox":
            case "treeitem":
                actions.Add("SELECT");
                break;
            case "slider":
            case "scrollbar":
                actions.Add("SCROLL");
                actions.Add("SELECT");
                break;
        }

        return actions.AsReadOnly();
    }

    public bool SupportsAction(ElementInfo element, string actionType)
    {
        return GetAvailableActions(element).Contains(actionType.ToUpperInvariant());
    }

    private object? FindElement(ElementInfo element)
    {
        // In production, this would use IUIAutomation to find the element
        // by AutomationId, Name, or other properties
        // For now, return null (element not found)
        return null;
    }

    private Task<bool> InvokeElement(object element)
    {
        // Cast to IUIAutomationInvokePattern and call Invoke()
        // In real implementation:
        // var pattern = element.GetCurrentPattern(UIA_InvokePatternId);
        // var invokePattern = (IUIAutomationInvokePattern)pattern;
        // invokePattern.Invoke();

        Debug.WriteLine("INVOKING element");
        return Task.FromResult(true);
    }

    private Task<bool> SelectElement(object element)
    {
        // Cast to IUIAutomationSelectionItemPattern and call Select()
        Debug.WriteLine("SELECTING element");
        return Task.FromResult(true);
    }

    private Task<bool> ToggleElement(object element)
    {
        // Cast to IUIAutomationTogglePattern and call Toggle()
        Debug.WriteLine("TOGGLING element");
        return Task.FromResult(true);
    }

    private Task<bool> TypeText(object element, string text)
    {
        // Cast to IUIAutomationValuePattern and call SetValue()
        Debug.WriteLine($"TYPING text: {text}");
        return Task.FromResult(true);
    }

    private Task<bool> ScrollElement(object element, string? direction)
    {
        // Cast to IUIAutomationScrollPattern and call Scroll()
        Debug.WriteLine($"SCROLLING element: {direction}");
        return Task.FromResult(true);
    }
}

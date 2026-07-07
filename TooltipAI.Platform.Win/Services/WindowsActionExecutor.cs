using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Automation;
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
        try
        {
            var root = AutomationElement.RootElement;

            // Search by AutomationId first
            if (!string.IsNullOrEmpty(element.AutomationId))
            {
                var condition = new PropertyCondition(AutomationElement.AutomationIdProperty, element.AutomationId);
                var found = root.FindFirst(TreeScope.Subtree, condition);
                if (found != null) return found;
            }

            // Fallback: search by Name
            if (!string.IsNullOrEmpty(element.Name) && element.Name != "Unknown")
            {
                var condition = new PropertyCondition(AutomationElement.NameProperty, element.Name);
                var found = root.FindFirst(TreeScope.Subtree, condition);
                if (found != null) return found;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private Task<bool> InvokeElement(object element)
    {
        try
        {
            if (element is AutomationElement el && el.TryGetCurrentPattern(InvokePattern.Pattern, out var pattern))
            {
                ((InvokePattern)pattern).Invoke();
                return Task.FromResult(true);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Invoke failed: {ex.Message}");
        }
        return Task.FromResult(false);
    }

    private Task<bool> SelectElement(object element)
    {
        try
        {
            if (element is AutomationElement el && el.TryGetCurrentPattern(SelectionItemPattern.Pattern, out var pattern))
            {
                ((SelectionItemPattern)pattern).Select();
                return Task.FromResult(true);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Select failed: {ex.Message}");
        }
        return Task.FromResult(false);
    }

    private Task<bool> ToggleElement(object element)
    {
        try
        {
            if (element is AutomationElement el && el.TryGetCurrentPattern(TogglePattern.Pattern, out var pattern))
            {
                ((TogglePattern)pattern).Toggle();
                return Task.FromResult(true);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Toggle failed: {ex.Message}");
        }
        return Task.FromResult(false);
    }

    private Task<bool> TypeText(object element, string text)
    {
        try
        {
            if (element is AutomationElement el && el.TryGetCurrentPattern(ValuePattern.Pattern, out var pattern))
            {
                ((ValuePattern)pattern).SetValue(text);
                return Task.FromResult(true);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"TypeText failed: {ex.Message}");
        }
        return Task.FromResult(false);
    }

    private Task<bool> ScrollElement(object element, string? direction)
    {
        try
        {
            if (element is AutomationElement el && el.TryGetCurrentPattern(ScrollPattern.Pattern, out var pattern))
            {
                var scroll = (ScrollPattern)pattern;
                var horizontal = direction?.ToLower() switch
                {
                    "left" => -1.0,
                    "right" => 1.0,
                    _ => 0.0
                };
                var vertical = direction?.ToLower() switch
                {
                    "up" => -1.0,
                    "down" => 1.0,
                    _ => 0.0
                };
                scroll.Scroll(horizontal, vertical);
                return Task.FromResult(true);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Scroll failed: {ex.Message}");
        }
        return Task.FromResult(false);
    }
}

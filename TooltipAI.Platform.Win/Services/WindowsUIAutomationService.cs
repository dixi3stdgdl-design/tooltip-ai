using TooltipAI.Core.Interfaces;
using TooltipAI.Core.Models;
using TooltipAI.Platform.Win.Interop;

namespace TooltipAI.Platform.Win.Services;

/// <summary>
/// Windows UI Automation implementation using raw COM Interop with UIAutomationCore.dll.
/// Extracts real element information (name, control type, automation ID, help text)
/// from the UI element under the cursor.
/// </summary>
public sealed class WindowsUIAutomationService : IUIAutomationService
{
    public bool IsAvailable => true;

    public ElementInfo? GetElementFromPoint(int x, int y)
    {
        try
        {
            var element = UIAutomationInterop.ElementFromPoint(x, y);
            if (element == null)
                return null;

            return ExtractElementInfo(element);
        }
        catch
        {
            return null;
        }
    }

    public ElementInfo? GetFocusedElement()
    {
        try
        {
            var element = UIAutomationInterop.GetFocusedElement();
            if (element == null)
                return null;

            return ExtractElementInfo(element);
        }
        catch
        {
            return null;
        }
    }

    private static ElementInfo ExtractElementInfo(UIAutomationInterop.IUIAutomationElement element)
    {
        var name = UIAutomationInterop.GetStringProperty(
            element, UIAutomationInterop.UIA_NamePropertyId);

        var className = UIAutomationInterop.GetStringProperty(
            element, UIAutomationInterop.UIA_ClassNamePropertyId);

        var automationId = UIAutomationInterop.GetStringProperty(
            element, UIAutomationInterop.UIA_AutomationIdPropertyId);

        var helpText = UIAutomationInterop.GetStringProperty(
            element, UIAutomationInterop.UIA_HelpTextPropertyId);

        var isEnabled = UIAutomationInterop.GetBoolProperty(
            element, UIAutomationInterop.UIA_IsEnabledPropertyId);

        var isKeyboardFocusable = UIAutomationInterop.GetBoolProperty(
            element, UIAutomationInterop.UIA_IsKeyboardFocusablePropertyId);

        var controlTypeId = 0;
        try
        {
            var hr = element.get_CurrentControlType(out controlTypeId);
            if (hr != 0)
                controlTypeId = 0;
        }
        catch
        {
            controlTypeId = 0;
        }

        var controlType = UIAutomationInterop.MapControlType(controlTypeId);

        var processName = string.Empty;
        try
        {
            var hr = element.get_CurrentProcessId(out var processId);
            if (hr == 0 && processId > 0)
            {
                var proc = System.Diagnostics.Process.GetProcessById(processId);
                processName = proc.ProcessName;
            }
        }
        catch
        {
            // Process may have exited or access denied
        }

        return new ElementInfo
        {
            Name = name,
            ControlType = controlType,
            ClassName = className,
            AutomationId = automationId,
            HelpText = helpText,
            IsEnabled = isEnabled,
            IsKeyboardFocusable = isKeyboardFocusable,
            ProcessName = processName
        };
    }
}

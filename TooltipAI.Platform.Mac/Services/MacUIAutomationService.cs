using System.Diagnostics;
using System.Runtime.InteropServices;
using TooltipAI.Core.Interfaces;
using TooltipAI.Core.Models;

namespace TooltipAI.Platform.Mac.Services;

/// <summary>
/// macOS UI Automation using AXUIElement (Accessibility API).
/// Extracts element information from the UI tree at a given point.
/// </summary>
public sealed class MacUIAutomationService : IUIAutomationService
{
    [DllImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
    private static extern IntPtr AXUIElementCreateSystemWide();

    [DllImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
    private static extern int AXUIElementCopyElementAtPosition(
        IntPtr application,
        float x,
        float y,
        out IntPtr element);

    [DllImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
    private static extern int AXUIElementCopyAttributeValue(
        IntPtr element,
        IntPtr attribute,
        out IntPtr value);

    [DllImport("/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices")]
    private static extern int AXUIElementGetPid(
        IntPtr element,
        out int pid);

    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static extern IntPtr CFStringCreateWithCString(
        IntPtr allocator,
        string cStr,
        int encoding);

    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static extern bool CFStringGetCString(
        IntPtr theString,
        byte[] buffer,
        int bufferSize,
        int encoding);

    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static extern long CFRelease(IntPtr cf);

    private const int kCFStringEncodingUTF8 = 0x08000100;

    public ElementInfo? GetElementFromPoint(int x, int y)
    {
        try
        {
            var systemWide = AXUIElementCreateSystemWide();
            var result = AXUIElementCopyElementAtPosition(systemWide, x, y, out var element);
            CFRelease(systemWide);

            if (result != 0 || element == IntPtr.Zero)
                return null;

            var name = GetStringAttribute(element, "AXTitle")
                       ?? GetStringAttribute(element, "AXDescription")
                       ?? string.Empty;
            var role = GetStringAttribute(element, "AXRole") ?? string.Empty;
            var subrole = GetStringAttribute(element, "AXSubrole") ?? string.Empty;
            var description = GetStringAttribute(element, "AXHelp") ?? string.Empty;
            var pid = 0;
            AXUIElementGetPid(element, out pid);
            CFRelease(element);

            var processName = GetProcessName(pid);

            return new ElementInfo
            {
                Name = name,
                ControlType = role,
                HelpText = description,
                ClassName = subrole,
                ProcessId = pid,
                ProcessName = processName,
                AutomationId = string.Empty,
                IsEnabled = true,
                IsKeyboardFocusable = role == "AXTextField" || role == "AXTextArea"
            };
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
            var systemWide = AXUIElementCreateSystemWide();
            var result = AXUIElementCopyAttributeValue(systemWide,
                CFStringCreateWithCString(IntPtr.Zero, "AXFocusedUIElement", kCFStringEncodingUTF8),
                out var focusedElement);
            CFRelease(systemWide);

            if (result != 0 || focusedElement == IntPtr.Zero)
                return null;

            var name = GetStringAttribute(focusedElement, "AXTitle") ?? string.Empty;
            var role = GetStringAttribute(focusedElement, "AXRole") ?? string.Empty;
            var pid = 0;
            AXUIElementGetPid(focusedElement, out pid);
            CFRelease(focusedElement);

            return new ElementInfo
            {
                Name = name,
                ControlType = role,
                ProcessId = pid,
                ProcessName = GetProcessName(pid)
            };
        }
        catch
        {
            return null;
        }
    }

    public List<ElementInfo> GetChildren(ElementInfo parent)
    {
        return new List<ElementInfo>();
    }

    private string? GetStringAttribute(IntPtr element, string attributeName)
    {
        var attributePtr = CFStringCreateWithCString(IntPtr.Zero, attributeName, kCFStringEncodingUTF8);
        var result = AXUIElementCopyAttributeValue(element, attributePtr, out var value);
        CFRelease(attributePtr);

        if (result != 0 || value == IntPtr.Zero)
            return null;

        var buffer = new byte[512];
        if (CFStringGetCString(value, buffer, buffer.Length, kCFStringEncodingUTF8))
        {
            CFRelease(value);
            return System.Text.Encoding.UTF8.GetString(buffer).TrimEnd('\0');
        }

        CFRelease(value);
        return null;
    }

    private string GetProcessName(int pid)
    {
        try
        {
            var process = Process.GetProcessById(pid);
            return process.ProcessName;
        }
        catch
        {
            return string.Empty;
        }
    }
}

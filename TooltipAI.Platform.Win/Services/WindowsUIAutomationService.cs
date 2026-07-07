using System.Runtime.InteropServices;
using TooltipAI.Core.Interfaces;
using TooltipAI.Core.Models;

namespace TooltipAI.Platform.Win.Services;

/// <summary>
/// Windows UI Automation implementation using Win32 UI Automation COM APIs.
/// </summary>
public sealed class WindowsUIAutomationService : IUIAutomationService
{
    private const int UIA_ElementCreatedEventId = 20001;

    [DllImport("oleacc.dll")]
    private static extern int LibleoleaccAccessibleObjectFromPoint(POINT pt, [MarshalAs(UnmanagedType.IUnknown)] out object? ppacc, out object? pvarChildren);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    public bool IsAvailable => true;

    public ElementInfo? GetElementFromPoint(int x, int y)
    {
        try
        {
            var point = new POINT { X = x, Y = y };
            var hr = LibleoleaccAccessibleObjectFromPoint(point, out var accessible, out _);

            if (hr != 0 || accessible == null)
                return null;

            return ExtractElementInfo(accessible);
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
            GetCursorPos(out var point);
            return GetElementFromPoint(point.X, point.Y);
        }
        catch
        {
            return null;
        }
    }

    private ElementInfo ExtractElementInfo(object accessible)
    {
        try
        {
            dynamic acc = accessible;

            var name = TryGet(() => acc.CurrentName as string) ?? "Unknown";
            var controlType = TryGet(() => acc.CurrentControlType.ToString()) ?? "Unknown";
            var className = TryGet(() => acc.CurrentClassName as string) ?? "";
            var automationId = TryGet(() => acc.CurrentAutomationId as string) ?? "";

            return new ElementInfo
            {
                Name = name,
                ControlType = controlType,
                ClassName = className,
                AutomationId = automationId
            };
        }
        catch
        {
            return new ElementInfo { Name = "Unknown", ControlType = "Unknown" };
        }
    }

    private T? TryGet<T>(Func<T> getter) where T : class
    {
        try { return getter(); } catch { return null; }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }
}
using System.Runtime.InteropServices;
using TooltipAI.Core.Interfaces;
using TooltipAI.Core.Models;
using TooltipAI.Core.Services;

namespace TooltipAI.Service.Services;

public class UIAutomationService : IUIAutomationService
{
    private readonly SoftwareCategoryClassifier _classifier = new();

    public bool IsAvailable => true;

    public ElementInfo? GetElementFromPoint(int x, int y)
    {
        try
        {
            var hWnd = WindowFromPoint(new POINT { X = x, Y = y });
            if (hWnd == IntPtr.Zero)
                return null;

            var nameLength = GetWindowTextLength(hWnd);
            var nameBuilder = new System.Text.StringBuilder(nameLength + 1);
            GetWindowText(hWnd, nameBuilder, nameBuilder.Capacity);

            var classNameBuilder = new System.Text.StringBuilder(256);
            GetClassName(hWnd, classNameBuilder, classNameBuilder.Capacity);

            var processName = _classifier.GetProcessNameFromHwnd(hWnd);

            return new ElementInfo
            {
                Name = nameBuilder.ToString(),
                ControlType = "Window",
                ClassName = classNameBuilder.ToString(),
                IsEnabled = IsWindowEnabled(hWnd),
                HelpText = string.Empty,
                AutomationId = string.Empty,
                IsKeyboardFocusable = true
            };
        }
        catch
        {
            return null;
        }
    }

    public SoftwareCategory ClassifyElement(IntPtr hwnd, string windowTitle)
    {
        var classNameBuilder = new System.Text.StringBuilder(256);
        GetClassName(hwnd, classNameBuilder, classNameBuilder.Capacity);

        var processName = _classifier.GetProcessNameFromHwnd(hwnd);

        return _classifier.Classify(classNameBuilder.ToString(), windowTitle, processName ?? "");
    }

    public SoftwareCategory ClassifyElement(int x, int y)
    {
        var hWnd = WindowFromPoint(new POINT { X = x, Y = y });
        if (hWnd == IntPtr.Zero)
            return SoftwareCategory.Unknown;

        var nameLength = GetWindowTextLength(hWnd);
        var nameBuilder = new System.Text.StringBuilder(nameLength + 1);
        GetWindowText(hWnd, nameBuilder, nameBuilder.Capacity);

        return ClassifyElement(hWnd, nameBuilder.ToString());
    }

    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(POINT point);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindowEnabled(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X; public int Y; }
}

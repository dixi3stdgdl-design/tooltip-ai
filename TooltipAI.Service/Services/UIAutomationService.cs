using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
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

            var nameBuilder = new StringBuilder(512);
            GetWindowText(hWnd, nameBuilder, nameBuilder.Capacity);

            var classNameBuilder = new StringBuilder(256);
            GetClassName(hWnd, classNameBuilder, classNameBuilder.Capacity);

            var processName = GetProcessNameFromHwnd(hWnd);
            var windowTitle = nameBuilder.ToString();
            var className = classNameBuilder.ToString();

            var controlType = DetectControlType(hWnd, className, windowTitle);

            return new ElementInfo
            {
                Name = GetElementName(hWnd, windowTitle, className),
                ControlType = controlType,
                ClassName = className,
                ProcessName = processName ?? "",
                WindowTitle = windowTitle,
                IsEnabled = IsWindowEnabled(hWnd),
                HelpText = GetHelpText(hWnd),
                AutomationId = GetAutomationId(hWnd),
                IsKeyboardFocusable = IsWindowVisible(hWnd),
                Timestamp = DateTime.UtcNow
            };
        }
        catch
        {
            return CreateFallbackElement(x, y);
        }
    }

    private ElementInfo CreateFallbackElement(int x, int y)
    {
        try
        {
            var hWnd = WindowFromPoint(new POINT { X = x, Y = y });
            if (hWnd == IntPtr.Zero)
                return CreateUnknownElement(x, y);

            var processName = GetProcessNameFromHwnd(hWnd);

            return new ElementInfo
            {
                Name = "(element detected)",
                ControlType = "Unknown",
                ClassName = "",
                ProcessName = processName ?? "",
                WindowTitle = "",
                IsEnabled = true,
                IsKeyboardFocusable = false,
                Timestamp = DateTime.UtcNow
            };
        }
        catch
        {
            return CreateUnknownElement(x, y);
        }
    }

    private ElementInfo CreateUnknownElement(int x, int y)
    {
        return new ElementInfo
        {
            Name = $"(no element at {x},{y})",
            ControlType = "Unknown",
            ClassName = "",
            ProcessName = "",
            WindowTitle = "",
            IsEnabled = true,
            IsKeyboardFocusable = false,
            Timestamp = DateTime.UtcNow
        };
    }

    private string DetectControlType(IntPtr hWnd, string className, string windowTitle)
    {
        var lowerClassName = className.ToLowerInvariant();
        var lowerTitle = windowTitle.ToLowerInvariant();

        if (lowerClassName.Contains("button") || lowerClassName.Contains("btn"))
            return "Button";
        if (lowerClassName.Contains("edit") || lowerClassName.Contains("text"))
            return "TextBox";
        if (lowerClassName.Contains("combo") || lowerClassName.Contains("dropdown"))
            return "ComboBox";
        if (lowerClassName.Contains("check"))
            return "CheckBox";
        if (lowerClassName.Contains("radio"))
            return "RadioButton";
        if (lowerClassName.Contains("list"))
            return "ListBox";
        if (lowerClassName.Contains("tree"))
            return "TreeView";
        if (lowerClassName.Contains("tab"))
            return "TabControl";
        if (lowerClassName.Contains("toolbar") || lowerClassName.Contains("rebar"))
            return "ToolBar";
        if (lowerClassName.Contains("status"))
            return "StatusBar";
        if (lowerClassName.Contains("progress"))
            return "ProgressBar";
        if (lowerClassName.Contains("static") || lowerClassName.Contains("label"))
            return "Label";
        if (lowerClassName.Contains("scroll"))
            return "ScrollBar";
        if (lowerClassName.Contains("menu"))
            return "Menu";
        if (lowerClassName.Contains("dialog"))
            return "Dialog";

        if (IsToolbarButton(hWnd))
            return "Button";

        return "Window";
    }

    private bool IsToolbarButton(IntPtr hWnd)
    {
        try
        {
            var parent = GetParent(hWnd);
            if (parent == IntPtr.Zero)
                return false;

            var parentClass = new StringBuilder(256);
            GetClassName(parent, parentClass, parentClass.Capacity);

            return parentClass.ToString().ToLowerInvariant().Contains("toolbar");
        }
        catch
        {
            return false;
        }
    }

    private string GetElementName(IntPtr hWnd, string windowTitle, string className)
    {
        if (!string.IsNullOrEmpty(windowTitle))
            return windowTitle;

        var name = GetButtonName(hWnd);
        if (!string.IsNullOrEmpty(name))
            return name;

        name = GetTooltipText(hWnd);
        if (!string.IsNullOrEmpty(name))
            return name;

        return className;
    }

    private string GetButtonName(IntPtr hWnd)
    {
        try
        {
            var text = new StringBuilder(256);
            var length = SendMessage(hWnd, WM_GETTEXT, text.Capacity, text);
            return length > 0 ? text.ToString() : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private string GetTooltipText(IntPtr hWnd)
    {
        try
        {
            var tooltipWnd = SendMessage(hWnd, WM_GETTOOLTIP, 0, IntPtr.Zero);
            if (tooltipWnd == IntPtr.Zero)
                return string.Empty;

            var text = new StringBuilder(512);
            SendMessage(tooltipWnd, WM_GETTEXT, text.Capacity, text);
            return text.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }

    private string GetHelpText(IntPtr hWnd)
    {
        try
        {
            var helpText = new StringBuilder(256);
            SendMessage(hWnd, WM_GETTEXT, helpText.Capacity, helpText);
            return helpText.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }

    private string GetAutomationId(IntPtr hWnd)
    {
        try
        {
            var processName = GetProcessNameFromHwnd(hWnd);
            return $"{processName}_{hWnd.ToInt64():X}";
        }
        catch
        {
            return string.Empty;
        }
    }

    private string? GetProcessNameFromHwnd(IntPtr hWnd)
    {
        try
        {
            GetWindowThreadProcessId(hWnd, out var processId);
            if (processId == 0)
                return null;

            var process = Process.GetProcessById((int)processId);
            return process.ProcessName;
        }
        catch
        {
            return null;
        }
    }

    public SoftwareCategory ClassifyElement(IntPtr hwnd, string windowTitle)
    {
        var classNameBuilder = new StringBuilder(256);
        GetClassName(hwnd, classNameBuilder, classNameBuilder.Capacity);

        var processName = GetProcessNameFromHwnd(hwnd);

        return _classifier.Classify(classNameBuilder.ToString(), windowTitle, processName ?? "");
    }

    public SoftwareCategory ClassifyElement(int x, int y)
    {
        var hWnd = WindowFromPoint(new POINT { X = x, Y = y });
        if (hWnd == IntPtr.Zero)
            return SoftwareCategory.Unknown;

        var nameBuilder = new StringBuilder(512);
        GetWindowText(hWnd, nameBuilder, nameBuilder.Capacity);

        return ClassifyElement(hWnd, nameBuilder.ToString());
    }

    #region P/Invoke

    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(POINT point);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindowEnabled(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern IntPtr GetParent(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, StringBuilder lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

    private const int WM_GETTEXT = 0x000D;
    private const int WM_GETTOOLTIP = 0x0423;

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X; public int Y; }

    #endregion
}

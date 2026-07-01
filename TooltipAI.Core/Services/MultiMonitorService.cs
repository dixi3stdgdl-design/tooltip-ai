using System.Runtime.InteropServices;

namespace TooltipAI.Core.Services;

public class MultiMonitorService
{
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    private const uint MONITOR_DEFAULTTONEAREST = 2;

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X; public int Y; }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left, Top, Right, Bottom; }

    [StructLayout(LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    public MonitorInfo? GetCurrentMonitor()
    {
        if (!GetCursorPos(out var point))
            return null;

        var hMonitor = MonitorFromPoint(point, MONITOR_DEFAULTTONEAREST);
        var info = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };

        if (!GetMonitorInfo(hMonitor, ref info))
            return null;

        return new MonitorInfo
        {
            X = info.rcMonitor.Left,
            Y = info.rcMonitor.Top,
            Width = info.rcMonitor.Right - info.rcMonitor.Left,
            Height = info.rcMonitor.Bottom - info.rcMonitor.Top,
            WorkX = info.rcWork.Left,
            WorkY = info.rcWork.Top,
            WorkWidth = info.rcWork.Right - info.rcWork.Left,
            WorkHeight = info.rcWork.Bottom - info.rcWork.Top,
            IsPrimary = (info.dwFlags & 1) != 0
        };
    }

    public Point CalculateTooltipPosition(int cursorX, int cursorY, int tooltipWidth, int tooltipHeight)
    {
        var monitor = GetCurrentMonitor();
        if (monitor is null)
            return new Point { X = cursorX + 20, Y = cursorY };

        int x = cursorX + 20;
        int y = cursorY;

        // Right edge detection
        if (x + tooltipWidth > monitor.WorkX + monitor.WorkWidth)
            x = cursorX - tooltipWidth - 10;

        // Bottom edge detection
        if (y + tooltipHeight > monitor.WorkY + monitor.WorkHeight)
            y = monitor.WorkY + monitor.WorkHeight - tooltipHeight;

        // Top edge detection
        if (y < monitor.WorkY)
            y = monitor.WorkY;

        // Left edge detection
        if (x < monitor.WorkX)
            x = monitor.WorkX;

        return new Point { X = x, Y = y };
    }

    public bool IsCursorAtScreenEdge(int margin = 10)
    {
        var monitor = GetCurrentMonitor();
        if (monitor is null)
            return false;

        if (!GetCursorPos(out var point))
            return false;

        return point.X <= monitor.WorkX + margin ||
               point.X >= monitor.WorkX + monitor.WorkWidth - margin ||
               point.Y <= monitor.WorkY + margin ||
               point.Y >= monitor.WorkY + monitor.WorkHeight - margin;
    }
}

public class MonitorInfo
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int WorkX { get; set; }
    public int WorkY { get; set; }
    public int WorkWidth { get; set; }
    public int WorkHeight { get; set; }
    public bool IsPrimary { get; set; }
}

public class Point
{
    public int X { get; set; }
    public int Y { get; set; }
}

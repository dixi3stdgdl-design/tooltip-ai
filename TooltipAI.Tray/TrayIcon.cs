using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TooltipAI.Tray;

public class TrayIcon : IDisposable
{
    private const int NIM_ADD = 0x00000000;
    private const int NIM_MODIFY = 0x00000001;
    private const int NIM_DELETE = 0x00000002;
    private const int NIF_MESSAGE = 0x00000001;
    private const int NIF_ICON = 0x00000002;
    private const int NIF_TIP = 0x00000004;
    private const int NIF_INFO = 0x00000010;
    private const int WM_LBUTTONDBLCLK = 0x0203;
    private const int WM_RBUTTONUP = 0x0205;
    private const int WM_COMMAND = 0x0111;

    private NOTIFYICONDATA _nid;
    private IntPtr _hIcon;
    private IntPtr _hWnd;
    private bool _visible;

    public event Action? OnDoubleClick;
    public event Action? OnExit;

    public void SetWindowHandle(IntPtr hWnd)
    {
        _hWnd = hWnd;
        _nid.hWnd = hWnd;
    }

    public TrayIcon()
    {
        _hIcon = NativeMethods.LoadIcon(IntPtr.Zero, (IntPtr)32512);
        _nid = new NOTIFYICONDATA
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
            uID = 1,
            uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
            uCallbackMessage = 0x0401,
            hIcon = _hIcon,
            szTip = "Tooltip AI"
        };
    }

    public void Show()
    {
        if (_hWnd == IntPtr.Zero)
            throw new InvalidOperationException("SetWindowHandle must be called before Show");

        _nid.hWnd = _hWnd;
        var result = NativeMethods.Shell_NotifyIcon(NIM_ADD, ref _nid);
        _visible = result != 0;
    }

    public void Hide()
    {
        if (_visible)
        {
            NativeMethods.Shell_NotifyIcon(NIM_DELETE, ref _nid);
            _visible = false;
        }
    }

    public void ShowNotification(string title, string message, bool isError = false)
    {
        _nid.szInfoTitle = title;
        _nid.szInfo = message;
        _nid.dwInfoFlags = isError ? 0x00000003u : 0x00000001u;
        _nid.uFlags = NIF_INFO;
        NativeMethods.Shell_NotifyIcon(NIM_MODIFY, ref _nid);
        _nid.uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP;
    }

    public void UpdateTooltip(string text)
    {
        _nid.szTip = text;
        _nid.uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP;
        NativeMethods.Shell_NotifyIcon(NIM_MODIFY, ref _nid);
    }

    public void SetStatus(bool serviceRunning)
    {
        UpdateTooltip(serviceRunning ? "Tooltip AI - Running" : "Tooltip AI - Stopped");
    }

    public IntPtr ProcessMessage(uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == 0x0401 && _visible)
        {
            var eventMsg = (int)lParam;
            if (eventMsg == WM_LBUTTONDBLCLK)
                OnDoubleClick?.Invoke();
            else if (eventMsg == WM_RBUTTONUP)
                ShowContextMenu();
        }
        return IntPtr.Zero;
    }

    private void ShowContextMenu()
    {
        var menu = NativeMethods.CreatePopupMenu();
        NativeMethods.AppendMenu(menu, 0x00000000, 1001, "Enable Tooltips");
        NativeMethods.AppendMenu(menu, 0x00000000, 1002, "Settings...");
        NativeMethods.AppendMenu(menu, 0x00000008, 0, null);
        NativeMethods.AppendMenu(menu, 0x00000000, 1003, "Exit");

        NativeMethods.GetCursorPos(out var point);
        NativeMethods.SetForegroundWindow(_hWnd);
        NativeMethods.TrackPopupMenu(menu, 0x0000, point.X, point.Y, 0, _hWnd, IntPtr.Zero);
        NativeMethods.PostMessage(_hWnd, 0x0000, IntPtr.Zero, IntPtr.Zero);
        NativeMethods.DestroyMenu(menu);
    }

    public void Dispose()
    {
        Hide();
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NOTIFYICONDATA
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uID;
        public uint uFlags;
        public uint uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        public uint dwState;
        public uint dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;
        public uint uTimeoutOrVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;
        public uint dwInfoFlags;
        public Guid guidItem;
        public IntPtr hBalloonIcon;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X, Y; }

    private static class NativeMethods
    {
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int Shell_NotifyIcon(uint dwMessage, ref NOTIFYICONDATA lpData);

        [DllImport("user32.dll")]
        public static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

        [DllImport("user32.dll")]
        public static extern IntPtr CreatePopupMenu();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool AppendMenu(IntPtr hMenu, uint uFlags, uint uIDNewItem, string? lpNewItem);

        [DllImport("user32.dll")]
        public static extern bool DestroyMenu(IntPtr hMenu);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
    }
}

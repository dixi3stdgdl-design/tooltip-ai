using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TooltipAI.Tray;

static class Program
{
    private const int WM_DESTROY = 0x0002;
    private const int WM_COMMAND = 0x0111;

    private static TrayIcon? _trayIcon;
    private static Process? _serviceProcess;
    private static IntPtr _hwnd;
    private static readonly NativeMethods.WndProc _wndProc = WndProc;

    [STAThread]
    static void Main()
    {
        _hwnd = CreateMessageWindow();
        if (_hwnd == IntPtr.Zero)
        {
            NativeMethods.MessageBox(IntPtr.Zero, "Failed to create message window", "Tooltip AI", 0x10);
            return;
        }

        _trayIcon = new TrayIcon();
        _trayIcon.SetWindowHandle(_hwnd);
        _trayIcon.OnDoubleClick += ToggleService;
        _trayIcon.OnExit += ExitApplication;
        _trayIcon.Show();
        _trayIcon.SetStatus(false);

        StartService();

        var msg = new NativeMethods.MSG();
        while (NativeMethods.GetMessage(out msg, IntPtr.Zero, 0, 0))
        {
            if (msg.message == WM_COMMAND)
            {
                var menuId = msg.wParam.ToInt32() & 0xFFFF;
                switch (menuId)
                {
                    case 1001: ToggleService(); break;
                    case 1002: OpenSettings(); break;
                    case 1003: ExitApplication(); break;
                }
            }
            NativeMethods.TranslateMessage(ref msg);
            NativeMethods.DispatchMessage(ref msg);
        }
    }

    private static IntPtr CreateMessageWindow()
    {
        var hInstance = NativeMethods.GetModuleHandle(null);

        var wc = new NativeMethods.WNDCLASSEX
        {
            cbSize = (uint)Marshal.SizeOf<NativeMethods.WNDCLASSEX>(),
            lpfnWndProc = _wndProc,
            hInstance = hInstance,
            lpszClassName = "TooltipAI_TrayMsg",
            lpszMenuName = ""
        };

        NativeMethods.RegisterClassEx(ref wc);

        return NativeMethods.CreateWindowEx(
            0, "TooltipAI_TrayMsg", "", 0,
            0, 0, 0, 0,
            (IntPtr)(-3), IntPtr.Zero, hInstance, IntPtr.Zero);
    }

    private static IntPtr WndProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (_trayIcon is not null)
        {
            var result = _trayIcon.ProcessMessage(msg, wParam, lParam);
            if (result != IntPtr.Zero)
                return result;
        }

        switch (msg)
        {
            case WM_DESTROY:
                NativeMethods.PostQuitMessage(0);
                return IntPtr.Zero;
        }

        return NativeMethods.DefWindowProc(hwnd, msg, wParam, lParam);
    }

    private static void StartService()
    {
        try
        {
            var servicePath = Path.Combine(AppContext.BaseDirectory, "TooltipAI.Service");
            if (File.Exists(servicePath + ".exe"))
            {
                _serviceProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = servicePath + ".exe",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                _trayIcon?.SetStatus(true);
                _trayIcon?.ShowNotification("Tooltip AI", "Service started");
            }
            else
            {
                _trayIcon?.ShowNotification("Tooltip AI", "Service not found", true);
            }
        }
        catch (Exception ex)
        {
            _trayIcon?.ShowNotification("Tooltip AI", $"Start failed: {ex.Message}", true);
        }
    }

    private static void StopService()
    {
        try
        {
            if (_serviceProcess is { HasExited: false })
            {
                _serviceProcess.Kill();
                _serviceProcess.WaitForExit(5000);
            }
            _serviceProcess = null;
            _trayIcon?.SetStatus(false);
        }
        catch (Exception ex)
        {
            _trayIcon?.SetStatus(_serviceProcess is { HasExited: false });
            _trayIcon?.ShowNotification("Tooltip AI", $"Stop failed: {ex.Message}", true);
        }
    }

    private static void ToggleService()
    {
        if (_serviceProcess is { HasExited: false })
        {
            StopService();
            _trayIcon?.ShowNotification("Tooltip AI", "Service stopped");
        }
        else
        {
            StartService();
        }
    }

    private static void OpenSettings()
    {
        var settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TooltipAI", "settings.json");
        var directory = Path.GetDirectoryName(settingsPath);

        if (directory != null && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        if (!File.Exists(settingsPath))
        {
            var defaultSettings = @"{
  ""IsEnabled"": true,
  ""ShowAiContext"": true,
  ""TooltipDelayMs"": 100,
  ""TooltipMaxWidth"": 400,
  ""TooltipMaxHeight"": 250,
  ""Theme"": ""System"",
  ""Language"": ""en"",
  ""EnableNotifications"": true,
  ""EnableSound"": false,
  ""EnableTelemetry"": false
}";
            File.WriteAllText(settingsPath, defaultSettings);
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = settingsPath,
                UseShellExecute = true
            });
            _trayIcon?.ShowNotification("Tooltip AI", "Settings opened");
        }
        catch (Exception ex)
        {
            _trayIcon?.ShowNotification("Tooltip AI", $"Could not open settings: {ex.Message}", true);
        }
    }

    private static void ExitApplication()
    {
        StopService();
        _trayIcon?.Dispose();
        if (_hwnd != IntPtr.Zero)
            NativeMethods.DestroyWindow(_hwnd);
    }

    private static class NativeMethods
    {
        public delegate IntPtr WndProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WNDCLASSEX
        {
            public uint cbSize;
            public uint style;
            public WndProc lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT { public int X, Y; }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern ushort RegisterClassEx(ref WNDCLASSEX lpwcx);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateWindowEx(
            int dwExStyle, string lpClassName, string lpWindowName,
            int dwStyle, int x, int y, int nWidth, int nHeight,
            IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll")]
        public static extern bool DestroyWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern IntPtr DefWindowProc(IntPtr hwnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        public static extern bool TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        public static extern IntPtr DispatchMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        public static extern void PostQuitMessage(int nExitCode);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string? lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);
    }
}

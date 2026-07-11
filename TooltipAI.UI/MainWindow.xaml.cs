using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using TooltipAI.Core.Models;
using TooltipAI.UI.Services;
using TooltipAI.UI.Views;
using Windows.Graphics;
using Windows.UI;

namespace TooltipAI.UI;

public partial class MainWindow : Window
{
    private readonly NamedPipeClient _pipeClient;
    private readonly DispatcherTimer _positionTimer;
    private readonly DispatcherTimer _hideTimer;
    private bool _clickThroughSet;
    private bool _isTooltipVisible;
    private AppWindow? _appWindow;

    private const int WS_EX_LAYERED = 0x00080000;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_TOPMOST = 0x00000008;
    private const int WS_EX_NOACTIVATE = 0x08000000;
    private const int GWL_EXSTYLE = -20;

    private const int TOOLTIP_WIDTH = 420;
    private const int TOOLTIP_HEIGHT = 360;
    private const int CURSOR_OFFSET = 18;
    private const int HIDESHOW_DELAY_MS = 300;

    public MainWindow()
    {
        this.InitializeComponent();

        _appWindow = this.AppWindow;

        if (_appWindow is not null)
        {
            _appWindow.IsShownInSwitchers = false;

            var presenter = _appWindow.Presenter as OverlappedPresenter;
            if (presenter is not null)
            {
                presenter.IsAlwaysOnTop = true;
                presenter.IsResizable = false;
                presenter.IsMaximizable = false;
                presenter.IsMinimizable = false;
                presenter.SetBorderAndTitleBar(false, false);
            }

            var size = new Windows.Graphics.SizeInt32 { Width = TOOLTIP_WIDTH, Height = TOOLTIP_HEIGHT };
            _appWindow.Resize(size);
        }

        this.Activated += OnWindowActivated;
        this.Closed += OnWindowClosed;

        _pipeClient = new NamedPipeClient();
        _pipeClient.DataReceived += OnDataReceived;
        _pipeClient.Disconnected += OnDisconnected;

        _positionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33)
        };
        _positionTimer.Tick += OnPositionTimerTick;
        _positionTimer.Start();

        _hideTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(HIDESHOW_DELAY_MS)
        };
        _hideTimer.Tick += OnHideTimerTick;
        _hideTimer.Stop();

        Title = "TooltipAI";

        _ = ConnectToServiceAsync();
    }

    private void OnWindowActivated(object sender, WindowActivatedEventArgs args)
    {
        if (_clickThroughSet) return;

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        if (hwnd != IntPtr.Zero)
        {
            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            int newStyle = exStyle
                | WS_EX_LAYERED
                | WS_EX_TRANSPARENT
                | WS_EX_TOOLWINDOW
                | WS_EX_TOPMOST
                | WS_EX_NOACTIVATE;
            SetWindowLong(hwnd, GWL_EXSTYLE, newStyle);
            SetLayeredWindowAttributes(hwnd, 0, 252, 2);
            _clickThroughSet = true;
        }
    }

    private void OnWindowClosed(object sender, WindowEventArgs args)
    {
        _positionTimer.Stop();
        _hideTimer.Stop();
        _pipeClient.DataReceived -= OnDataReceived;
        _pipeClient.Disconnected -= OnDisconnected;
        _pipeClient.Dispose();
    }

    private async System.Threading.Tasks.Task ConnectToServiceAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[UI] Connecting to pipe...");
            await _pipeClient.ConnectAsync(System.Threading.CancellationToken.None);
            System.Diagnostics.Debug.WriteLine("[UI] Connected to pipe!");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UI] Pipe connection failed: {ex.Message}");
        }
    }

    private void OnDataReceived(TooltipData data)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            System.Diagnostics.Debug.WriteLine($"[UI] Data received: {data.Element?.Name} | {data.Element?.ControlType} | {data.SoftwareCategory}");
            _hideTimer.Stop();
            _isTooltipVisible = true;
            TooltipOverlay.UpdateFromData(data);
        });
    }

    private void OnDisconnected()
    {
        DispatcherQueue.TryEnqueue(async () =>
        {
            TooltipOverlay.Hide();
            _isTooltipVisible = false;
            await System.Threading.Tasks.Task.Delay(2000);
            _ = ConnectToServiceAsync();
        });
    }

    private void OnPositionTimerTick(object? sender, object e)
    {
        if (!_isTooltipVisible || _appWindow is null) return;

        GetCursorPos(out var cursorPos);

        var displayArea = DisplayArea.GetFromPoint(
            new PointInt32(cursorPos.X, cursorPos.Y), DisplayAreaFallback.Nearest);

        int x = cursorPos.X + CURSOR_OFFSET;
        int y = cursorPos.Y + CURSOR_OFFSET;

        var bounds = displayArea.OuterBounds;
        int tooltipW = _appWindow.Size.Width;
        int tooltipH = _appWindow.Size.Height;

        if (x + tooltipW > bounds.X + bounds.Width)
            x = cursorPos.X - tooltipW - CURSOR_OFFSET;
        if (y + tooltipH > bounds.Y + bounds.Height)
            y = bounds.Y + bounds.Height - tooltipH - 8;
        if (x < bounds.X)
            x = bounds.X + 8;
        if (y < bounds.Y)
            y = bounds.Y + 8;

        _appWindow.Move(new PointInt32(x, y));
    }

    private void OnHideTimerTick(object? sender, object e)
    {
        _hideTimer.Stop();
        if (_isTooltipVisible)
        {
            TooltipOverlay.Hide();
            _isTooltipVisible = false;
        }
    }

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

    [DllImport("user32.dll")]
    private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X, Y; }
}

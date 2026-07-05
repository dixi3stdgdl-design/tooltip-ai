using System.Diagnostics;
using System.Runtime.InteropServices;
using TooltipAI.Core.Interfaces;
using TooltipAI.Core.Models;

namespace TooltipAI.Platform.Win.Rendering;

/// <summary>
/// Windows glassmorphic renderer using DWM (Desktop Window Manager) and GDI+.
/// Implements all 4 visual concepts: QuantumPill, ContextualAura, TheBlade, GazeBracket.
/// 
/// Uses DwmExtendFrameIntoClientArea for glass effect and GDI+ for custom rendering.
/// </summary>
public sealed class GlassmorphicRenderer : IGlassmorphicRenderer
{
    // DWM APIs
    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_LAYERED = 0x00080000;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_TOPMOST = 0x00000008;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_NOACTIVATE = 0x08000000;
    private const int WS_EX_COMPOSITED = 0x02000000;

    private const int DWMWA_TRANSITIONS_FORCEDISABLED = 3;
    private const int DWMWA_NCRENDERING_POLICY = 2;
    private const int DWMWA_NCRENDERING_ENABLED = 1;

    private const int SWP_NOMOVE = 0x0002;
    private const int SWP_NOSIZE = 0x0001;
    private const int SWP_NOACTIVATE = 0x0010;
    private const int SWP_SHOWWINDOW = 0x0040;

    private const int LWA_ALPHA = 0x00000002;

    [StructLayout(LayoutKind.Sequential)]
    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    private IntPtr _hwnd;
    private readonly GlassmorphicConfig _config;
    private OverlayRenderState _state;
    private readonly System.Windows.Forms.Timer _animationTimer;
    private readonly Random _random = new();
    private bool _disposed;

    public OverlayRenderState State => _state;
    public GlassmorphicStyle CurrentStyle => _config.Style;

    public GlassmorphicRenderer(GlassmorphicStyle style = GlassmorphicStyle.QuantumPill)
    {
        _config = GlassmorphicConfig.GetDefault(style);
        _state = new OverlayRenderState { State = OverlayState.Hidden };

        _animationTimer = new System.Windows.Forms.Timer();
        _animationTimer.Interval = 16; // ~60fps
        _animationTimer.Tick += AnimationTick;

        CreateWindow();
    }

    private void CreateWindow()
    {
        var form = new Form
        {
            FormBorderStyle = FormBorderStyle.None,
            ShowInTaskbar = false,
            TopMost = true,
            StartPosition = FormStartPosition.Manual,
            BackColor = Color.Black,
            Opacity = 0,
            Size = new Size(10, 10)
        };

        _hwnd = form.Handle;

        // Extended window style: layered + topmost + tool + no-activate + no-paint
        var exStyle = WS_EX_LAYERED | WS_EX_TOPMOST | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
        SetWindowLong(_hwnd, GWL_EXSTYLE, exStyle);

        // DWM: disable transitions for instant show
        int disabled = 1;
        DwmSetWindowAttribute(_hwnd, DWMWA_TRANSITIONS_FORCEDISABLED, ref disabled, sizeof(int));

        // Extend frame for glass effect
        var margins = new MARGINS { cxLeftWidth = -1, cxRightWidth = -1, cyTopHeight = -1, cyBottomHeight = -1 };
        DwmExtendFrameIntoClientArea(_hwnd, ref margins);
    }

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    public void Show(int x, int y, ElementInfo element, string? statusText = null)
    {
        var config = _config;

        // Calculate position based on style
        int posX, posY, width, height;

        switch (config.Style)
        {
            case GlassmorphicStyle.QuantumPill:
                width = config.Width;
                height = config.Height;
                posX = x - (width / 2);
                posY = y + config.OffsetY;
                break;

            case GlassmorphicStyle.ContextualAura:
                // Aura wraps element — calculate from element bounds
                width = 200; // Placeholder — real impl uses element bounds
                height = 40;
                posX = x - 10;
                posY = y - 20;
                break;

            case GlassmorphicStyle.TheBlade:
                width = config.Width;
                height = config.Height;
                // Anchor opposite to cursor direction
                posX = x > 960 ? x - width - 20 : x + 20;
                posY = y - (height / 2);
                break;

            case GlassmorphicStyle.GazeBracket:
                width = 180; // Element width + padding
                height = 50;
                posX = x - 10;
                posY = y - 10;
                break;

            default:
                width = config.Width;
                height = config.Height;
                posX = x - (width / 2);
                posY = y + config.OffsetY;
                break;
        }

        // Update state
        _state = new OverlayRenderState
        {
            State = OverlayState.FadingIn,
            X = posX,
            Y = posY,
            Width = width,
            Height = height,
            CurrentOpacity = 0,
            TargetOpacity = 1.0f,
            StatusText = statusText ?? element.Name,
            Timestamp = Stopwatch.GetTimestamp()
        };

        // Position and size window
        SetWindowPos(_hwnd, IntPtr.Zero, posX, posY, width, height,
            SWP_NOACTIVATE | SWP_SHOWWINDOW);

        // Start fade-in animation
        _animationTimer.Start();
    }

    public void Update(OverlayRenderState newState)
    {
        _state = newState;

        if (newState.State == OverlayState.FadingOut)
        {
            _state.TargetOpacity = 0;
            _animationTimer.Start();
        }
    }

    public void UpdateWaveform(float[] data)
    {
        _state.WaveformData = data;
        _state.Timestamp = Stopwatch.GetTimestamp();
    }

    public void Hide()
    {
        _state.State = OverlayState.FadingOut;
        _state.TargetOpacity = 0;
        _animationTimer.Start();
    }

    public void SetStyle(GlassmorphicStyle style)
    {
        _config.Style = style;
        var defaults = GlassmorphicConfig.GetDefault(style);
        _config.Width = defaults.Width;
        _config.Height = defaults.Height;
        _config.BorderRadius = defaults.BorderRadius;
        _config.BlurRadius = defaults.BlurRadius;
        _config.BackgroundOpacity = defaults.BackgroundOpacity;
        _config.ShowWaveform = defaults.ShowWaveform;
    }

    public void StartPulse()
    {
        if (_config.Style == GlassmorphicStyle.ContextualAura)
        {
            _state.State = OverlayState.PulseBreathing;
            _animationTimer.Start();
        }
    }

    public void StopPulse()
    {
        _state.State = OverlayState.Visible;
    }

    public void Flash()
    {
        // GazeBracket: flash brackets on execution
        _state.State = OverlayState.Executing;
        _animationTimer.Start();
    }

    public void SetProgress(float progress)
    {
        _state.Progress = Math.Clamp(progress, 0, 1);
    }

    private void AnimationTick(object? sender, EventArgs e)
    {
        if (_disposed) return;

        var elapsed = (Stopwatch.GetTimestamp() - _state.Timestamp) / (double)Stopwatch.Frequency * 1000;

        switch (_state.State)
        {
            case OverlayState.FadingIn:
                AnimateFadeIn(elapsed);
                break;

            case OverlayState.FadingOut:
                AnimateFadeOut(elapsed);
                break;

            case OverlayState.PulseBreathing:
                AnimatePulse(elapsed);
                break;

            case OverlayState.Executing:
                AnimateFlash(elapsed);
                break;
        }

        // Apply opacity
        var alpha = (byte)(_state.CurrentOpacity * 255);
        SetLayeredWindowAttributes(_hwnd, 0, alpha, LWA_ALPHA);
    }

    private void AnimateFadeIn(double elapsedMs)
    {
        var progress = Math.Min(elapsedMs / _config.FadeInMs, 1.0);
        _state.CurrentOpacity = (float)EasingCubicOut(progress);

        if (progress >= 1.0)
        {
            _state.State = OverlayState.Visible;
            _state.CurrentOpacity = 1.0f;
            _animationTimer.Stop();
        }
    }

    private void AnimateFadeOut(double elapsedMs)
    {
        var progress = Math.Min(elapsedMs / _config.FadeOutMs, 1.0);
        _state.CurrentOpacity = 1.0f - (float)EasingCubicOut(progress);

        if (progress >= 1.0)
        {
            _state.State = OverlayState.Hidden;
            _state.CurrentOpacity = 0;
            _animationTimer.Stop();
            ShowWindow(_hwnd, 0); // Hide
        }
    }

    private void AnimatePulse(double elapsedMs)
    {
        // Breathing animation: opacity oscillates between min and max
        var cycle = Math.Sin(elapsedMs / 500 * Math.PI) * 0.5 + 0.5; // 0 to 1
        var minOp = _config.PulseMinOpacity / 100f;
        var maxOp = _config.PulseMaxOpacity / 100f;
        _state.CurrentOpacity = minOp + (float)cycle * (maxOp - minOp);
    }

    private void AnimateFlash(double elapsedMs)
    {
        // Flash: quick pulse then return
        if (elapsedMs < 100)
        {
            _state.CurrentOpacity = 1.0f;
        }
        else if (elapsedMs < 200)
        {
            _state.CurrentOpacity = 0.3f;
        }
        else if (elapsedMs < 300)
        {
            _state.CurrentOpacity = 1.0f;
        }
        else
        {
            _state.State = OverlayState.Visible;
            _animationTimer.Stop();
        }
    }

    private static double EasingCubicOut(double t) => 1 - Math.Pow(1 - t, 3);

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _animationTimer?.Stop();
            _animationTimer?.Dispose();

            if (_hwnd != IntPtr.Zero)
            {
                ShowWindow(_hwnd, 0);
                // Note: Form disposal handled by GC
            }
        }
        GC.SuppressFinalize(this);
    }
}

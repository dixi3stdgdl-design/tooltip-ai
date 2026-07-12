using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using TooltipAI.Core.Models;
using TooltipAI.Core.Services;
using TooltipAI.UI.Services;

namespace TooltipAI.UI;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        EnsureServiceRunning();

        var overlay = new OverlayForm();
        Application.Run(overlay);
    }

    static void EnsureServiceRunning()
    {
        var serviceProcess = Process.GetProcessesByName("TooltipAI.Service").FirstOrDefault();
        if (serviceProcess == null)
        {
            var servicePath = Path.Combine(AppContext.BaseDirectory, "TooltipAI.Service.exe");
            if (File.Exists(servicePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = servicePath,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                Thread.Sleep(2000);
            }
        }
    }
}

public class OverlayForm : Form
{
    private readonly NamedPipeClient _pipeClient;
    private readonly System.Windows.Forms.Timer _renderTimer;
    private TooltipData? _currentData;

    // Animation state
    private float _opacity = 0f;       // Current opacity (0-1)
    private float _targetOpacity = 0f;  // Target opacity
    private const float FADE_SPEED = 0.12f; // Fade speed per tick
    private bool _hasData;

    // Win32
    private const int WS_EX_LAYERED = 0x00080000;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_TOPMOST = 0x00000008;
    private const int WS_EX_NOACTIVATE = 0x08000000;
    private const int GWL_EXSTYLE = -20;
    private const int LWA_ALPHA = 0x00000002;

    [DllImport("user32.dll")] static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
    [DllImport("user32.dll")] static extern int GetWindowLong(IntPtr hwnd, int index);
    [DllImport("user32.dll")] static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);
    [DllImport("user32.dll")] static extern bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    struct POINT { public int X, Y; }

    public OverlayForm()
    {
        Text = "TooltipAI";
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        BackColor = Color.FromArgb(1, 2, 3); // Near-black, not pure black (needed for layered window)
        Opacity = 1; // CRITICAL: must be 1 for layered window to work
        Size = new Size(380, 280);
        StartPosition = FormStartPosition.Manual;
        Location = new System.Drawing.Point(-1000, -1000);

        DoubleBuffered = true;

        _pipeClient = new NamedPipeClient();
        _pipeClient.DataReceived += OnDataReceived;
        _pipeClient.Disconnected += OnDisconnected;

        // Render timer - 60fps for smooth animation
        _renderTimer = new System.Windows.Forms.Timer();
        _renderTimer.Interval = 16;
        _renderTimer.Tick += OnRenderTick;
        _renderTimer.Start();

        _ = ConnectToServiceAsync();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        var hwnd = Handle;
        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE,
            exStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_TOPMOST | WS_EX_NOACTIVATE);
        UpdateLayeredAlpha();
    }

    private void UpdateLayeredAlpha()
    {
        if (Handle != IntPtr.Zero)
            SetLayeredWindowAttributes(Handle, 0, (byte)(_opacity * 255), LWA_ALPHA);
    }

    private async Task ConnectToServiceAsync()
    {
        await _pipeClient.ConnectAsync(CancellationToken.None);
    }

    private void OnDataReceived(TooltipData data)
    {
        if (InvokeRequired) { Invoke(() => OnDataReceived(data)); return; }
        _currentData = data;
        _hasData = true;
        _targetOpacity = 1f; // Fade in
    }

    private void OnDisconnected()
    {
        if (InvokeRequired) { Invoke(OnDisconnected); return; }
        _hasData = false;
        _targetOpacity = 0f; // Fade out
        _ = Task.Run(async () => { await Task.Delay(2000); await ConnectToServiceAsync(); });
    }

    private void OnRenderTick(object? sender, EventArgs e)
    {
        // Smooth fade animation
        if (_opacity < _targetOpacity)
            _opacity = Math.Min(_opacity + FADE_SPEED, _targetOpacity);
        else if (_opacity > _targetOpacity)
            _opacity = Math.Max(_opacity - FADE_SPEED, _targetOpacity);

        // Hide when fully transparent
        if (_opacity < 0.01f && !_hasData)
        {
            if (Location.X != -1000) Location = new System.Drawing.Point(-1000, -1000);
            UpdateLayeredAlpha();
            return;
        }

        // Position near cursor
        if (_hasData && _currentData != null)
        {
            GetCursorPos(out var cp);
            int x = cp.X + 16;
            int y = cp.Y + 16;
            var screen = Screen.FromPoint(new System.Drawing.Point(cp.X, cp.Y));
            if (x + Width > screen.WorkingArea.Right) x = cp.X - Width - 16;
            if (y + Height > screen.WorkingArea.Bottom) y = screen.WorkingArea.Bottom - Height - 8;
            if (x < screen.WorkingArea.Left) x = screen.WorkingArea.Left + 8;
            if (y < screen.WorkingArea.Top) y = screen.WorkingArea.Top + 8;
            Location = new System.Drawing.Point(x, y);
        }

        UpdateLayeredAlpha();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (_currentData == null || _opacity < 0.01f) return;

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

        DrawTooltip(g, _currentData);
    }

    private void DrawTooltip(Graphics g, TooltipData data)
    {
        var category = Enum.TryParse<SoftwareCategory>(data.SoftwareCategory, out var cat)
            ? cat : SoftwareCategory.Unknown;
        var theme = TooltipColorTheme.GetTheme(category);
        var accent = FromUint(theme.AccentColor);

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);

        // --- BACKGROUND: translucent dark glass ---
        using (var bgPath = RoundedRect(rect, 14))
        using (var bgBrush = new SolidBrush(Color.FromArgb(22, 22, 28))) // Very translucent
            g.FillPath(bgBrush, bgPath);

        // --- BORDER: subtle accent glow ---
        using (var borderPath = RoundedRect(rect, 14))
        using (var borderPen = new Pen(Color.FromArgb(50, accent.R, accent.G, accent.B), 1.5f))
            g.DrawPath(borderPen, borderPath);

        // --- OUTER GLOW ---
        using (var glowPath = RoundedRect(new Rectangle(-1, -1, Width + 1, Height + 1), 15))
        using (var glowPen = new Pen(Color.FromArgb(20, accent.R, accent.G, accent.B), 1f))
            g.DrawPath(glowPen, glowPath);

        int y = 14;
        int pad = 16;

        // --- CATEGORY BADGE ---
        using (var badgeFont = new Font("Segoe UI Semibold", 7.5f))
        {
            var label = theme.CategoryLabel;
            var sz = g.MeasureString(label, badgeFont);
            using (var badgeBg = new SolidBrush(Color.FromArgb(35, accent.R, accent.G, accent.B)))
            using (var badgePath = RoundedRect(new Rectangle(pad, y, (int)sz.Width + 10, (int)sz.Height + 4), 4))
                g.FillPath(badgeBg, badgePath);
            using (var badgeFg = new SolidBrush(accent))
                g.DrawString(label, badgeFont, badgeFg, pad + 5, y + 2);
            y += (int)sz.Height + 10;
        }

        // --- WINDOW TITLE ---
        using (var titleFont = new Font("Segoe UI Semibold", 9.5f))
        {
            var title = data.ProcessName ?? "Unknown";
            if (!string.IsNullOrEmpty(data.WindowTitle))
                title += $" — {data.WindowTitle}";
            if (title.Length > 45) title = title[..42] + "...";
            g.DrawString(title, titleFont, Brushes.White, pad, y);
            y += (int)g.MeasureString(title, titleFont).Height + 6;
        }

        // --- SEPARATOR ---
        using (var sepPen = new Pen(Color.FromArgb(30, accent.R, accent.G, accent.B), 0.5f))
            g.DrawLine(sepPen, pad, y, Width - pad, y);
        y += 8;

        // --- ELEMENT INFO ---
        if (data.Element != null)
        {
            using (var infoFont = new Font("Segoe UI", 8.5f))
            using (var infoBrush = new SolidBrush(Color.FromArgb(160, 160, 170)))
            {
                var info = $"{data.Element.ControlType}";
                if (!string.IsNullOrEmpty(data.Element.Name) && data.Element.Name != "Unknown")
                    info += $"  ·  {data.Element.Name}";
                g.DrawString(info, infoFont, infoBrush, pad, y);
                y += (int)g.MeasureString(info, infoFont, Width - pad * 2).Height + 2;

                if (!string.IsNullOrEmpty(data.Element.ClassName))
                {
                    var cls = $"Class: {data.Element.ClassName}";
                    g.DrawString(cls, infoFont, new SolidBrush(Color.FromArgb(100, 100, 110)), pad, y);
                    y += (int)g.MeasureString(cls, infoFont).Height + 6;
                }
            }
        }

        // --- HINTS ---
        y = DrawHint(g, y, pad, "GESTO", data.GestureHint, accent);
        y = DrawHint(g, y, pad, "TIP", data.QualityTip, Color.FromArgb(255, 214, 0));
        y = DrawHint(g, y, pad, "GUIA", data.MoveGuide, Color.FromArgb(0, 168, 255));
        y = DrawHint(g, y, pad, "DATOS", data.DataInsight, Color.FromArgb(255, 107, 107));

        // --- AI CONTEXT ---
        if (!string.IsNullOrEmpty(data.AiContext))
        {
            y += 2;
            using (var ctxFont = new Font("Segoe UI", 8f))
            using (var ctxBrush = new SolidBrush(Color.FromArgb(90, 90, 100)))
            {
                var ctx = data.AiContext;
                if (ctx.Length > 70) ctx = ctx[..67] + "...";
                g.DrawString(ctx, ctxFont, ctxBrush, pad, y);
            }
        }
    }

    private int DrawHint(Graphics g, int y, int pad, string label, string? value, Color color)
    {
        if (string.IsNullOrEmpty(value)) return y;

        using var labelFont = new Font("Segoe UI Semibold", 7f);
        using var valueFont = new Font("Segoe UI", 8f);

        var labelSize = g.MeasureString(label, labelFont);
        using (var labelBg = new SolidBrush(Color.FromArgb(25, color.R, color.G, color.B)))
        using (var labelPath = RoundedRect(new Rectangle(pad, y, (int)labelSize.Width + 8, (int)labelSize.Height + 2), 3))
            g.FillPath(labelBg, labelPath);

        using (var labelFg = new SolidBrush(color))
            g.DrawString(label, labelFont, labelFg, pad + 4, y + 1);

        using (var valueFg = new SolidBrush(Color.FromArgb(180, 180, 190)))
        {
            var displayValue = value.Length > 55 ? value[..52] + "..." : value;
            g.DrawString(displayValue, valueFont, valueFg, pad + (int)labelSize.Width + 12, y);
        }

        return y + (int)labelSize.Height + 6;
    }

    static Color FromUint(uint c) => Color.FromArgb((int)((c >> 16) & 0xFF), (int)((c >> 8) & 0xFF), (int)(c & 0xFF));

    static GraphicsPath RoundedRect(Rectangle r, int radius)
    {
        var path = new GraphicsPath();
        int d = radius * 2;
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_TOPMOST | WS_EX_NOACTIVATE;
            return cp;
        }
    }
}

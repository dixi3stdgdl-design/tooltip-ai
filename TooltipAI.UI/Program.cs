using System.Diagnostics;
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

        // Ensure Service is running
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
                Thread.Sleep(2000); // Wait for service to start
            }
        }
    }
}

public class OverlayForm : Form
{
    private readonly NamedPipeClient _pipeClient;
    private readonly System.Windows.Forms.Timer _positionTimer;
    private TooltipData? _currentData;
    private bool _isVisible;
    private string _pipeStatus = "Connecting...";

    // Win32 constants for click-through
    private const int WS_EX_LAYERED = 0x00080000;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_TOPMOST = 0x00000008;
    private const int WS_EX_NOACTIVATE = 0x08000000;
    private const int GWL_EXSTYLE = -20;
    private const int LWA_ALPHA = 0x00000002;

    [DllImport("user32.dll")]
    static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

    [DllImport("user32.dll")]
    static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll")]
    static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("user32.dll")]
    static extern bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    struct POINT { public int X, Y; }

    public OverlayForm()
    {
        // Form setup - transparent, click-through, topmost
        Text = "TooltipAI";
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        BackColor = Color.Black;
        Opacity = 0;
        Size = new Size(420, 360);
        StartPosition = FormStartPosition.Manual;
        Location = new System.Drawing.Point(-1000, -1000); // Off-screen initially

        // Named pipe client
        _pipeClient = new NamedPipeClient();
        _pipeClient.DataReceived += OnDataReceived;
        _pipeClient.Disconnected += OnDisconnected;
        _pipeClient.StatusChanged += (msg) => _pipeStatus = msg;

        // Position timer - 30fps cursor tracking
        _positionTimer = new System.Windows.Forms.Timer();
        _positionTimer.Interval = 33;
        _positionTimer.Tick += OnPositionTick;
        _positionTimer.Start();

        // Start pipe connection
        _ = ConnectToServiceAsync();

        Console.WriteLine("[UI] Overlay started");
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        // Make click-through after window is shown
        var hwnd = Handle;
        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE,
            exStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_TOPMOST | WS_EX_NOACTIVATE);
        SetLayeredWindowAttributes(hwnd, 0, 252, LWA_ALPHA);
    }

    private async Task ConnectToServiceAsync()
    {
        Console.WriteLine("[UI] Connecting to pipe...");
        await _pipeClient.ConnectAsync(CancellationToken.None);
    }

    private void OnDataReceived(TooltipData data)
    {
        if (InvokeRequired)
        {
            Invoke(() => OnDataReceived(data));
            return;
        }

        _currentData = data;
        _isVisible = true;
        Console.WriteLine($"[UI] Data: {data.Element?.Name} | {data.Element?.ControlType} | {data.SoftwareCategory}");
        Invalidate(); // Trigger repaint
    }

    private void OnDisconnected()
    {
        if (InvokeRequired)
        {
            Invoke(OnDisconnected);
            return;
        }

        _isVisible = false;
        _currentData = null;
        Location = new System.Drawing.Point(-1000, -1000);
        Console.WriteLine("[UI] Disconnected from pipe, reconnecting...");

        _ = Task.Run(async () =>
        {
            await Task.Delay(2000);
            await ConnectToServiceAsync();
        });
    }

    private void OnPositionTick(object? sender, EventArgs e)
    {
        if (!_isVisible || _currentData == null)
        {
            if (Location.X != -1000) Location = new System.Drawing.Point(-1000, -1000);
            return;
        }

        GetCursorPos(out var cursorPos);

        int x = cursorPos.X + 18;
        int y = cursorPos.Y + 18;

        // Clamp to screen bounds
        var screen = Screen.FromPoint(new System.Drawing.Point(cursorPos.X, cursorPos.Y));
        if (x + Width > screen.WorkingArea.Right)
            x = cursorPos.X - Width - 18;
        if (y + Height > screen.WorkingArea.Bottom)
            y = screen.WorkingArea.Bottom - Height - 8;
        if (x < screen.WorkingArea.Left)
            x = screen.WorkingArea.Left + 8;
        if (y < screen.WorkingArea.Top)
            y = screen.WorkingArea.Top + 8;

        Location = new System.Drawing.Point(x, y);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

        if (_currentData == null)
        {
            DrawStatus(g);
            return;
        }

        DrawGlassmorphicTooltip(g, _currentData);
    }

    private void DrawStatus(Graphics g)
    {
        using var bg = new SolidBrush(Color.FromArgb(40, 40, 40));
        using var path = CreateRoundedRect(new Rectangle(0, 0, Width, Height), 12);
        g.FillPath(bg, path);

        using var font = new Font("Segoe UI", 10f);
        using var brush = new SolidBrush(Color.FromArgb(200, 200, 200));
        g.DrawString($"TooltipAI\n\nStatus: {_pipeStatus}", font, brush, 20, 20);
    }

    private void DrawGlassmorphicTooltip(Graphics g, TooltipData data)
    {
        var category = Enum.TryParse<SoftwareCategory>(data.SoftwareCategory, out var cat)
            ? cat : SoftwareCategory.Unknown;

        // Get theme colors
        var theme = TooltipColorTheme.GetTheme(category);
        var accent = Color.FromArgb(
            (int)((theme.AccentColor >> 16) & 0xFF),
            (int)((theme.AccentColor >> 8) & 0xFF),
            (int)(theme.AccentColor & 0xFF));
        var border = Color.FromArgb(
            (int)((theme.BorderPrimary >> 16) & 0xFF),
            (int)((theme.BorderPrimary >> 8) & 0xFF),
            (int)(theme.BorderPrimary & 0xFF));

        // Background - dark glassmorphic
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var bgPath = CreateRoundedRect(rect, 12);
        using var bgBrush = new SolidBrush(Color.FromArgb(30, 30, 35));
        g.FillPath(bgBrush, bgPath);

        // Border glow
        using var borderPen = new Pen(Color.FromArgb(60, accent.R, accent.G, accent.B), 2f);
        g.DrawPath(borderPen, bgPath);

        // Outer glow
        using var glowPath = CreateRoundedRect(new Rectangle(-2, -2, Width + 3, Height + 3), 14);
        using var glowPen = new Pen(Color.FromArgb(30, accent.R, accent.G, accent.B), 1f);
        g.DrawPath(glowPen, glowPath);

        int y = 16;

        // Category badge
        using var badgeFont = new Font("Segoe UI", 8f, FontStyle.Bold);
        var catLabel = theme.CategoryLabel;
        var catSize = g.MeasureString(catLabel, badgeFont);
        using var badgeBg = new SolidBrush(Color.FromArgb(40, accent.R, accent.G, accent.B));
        var badgeRect = new Rectangle(16, y, (int)catSize.Width + 12, (int)catSize.Height + 4);
        using var badgePath = CreateRoundedRect(badgeRect, 4);
        g.FillPath(badgeBg, badgePath);
        using var badgeFg = new SolidBrush(accent);
        g.DrawString(catLabel, badgeFont, badgeFg, 22, y + 2);
        y += (int)catSize.Height + 12;

        // Process name + window title
        using var titleFont = new Font("Segoe UI", 10f, FontStyle.Bold);
        var title = data.ProcessName ?? "Unknown";
        if (!string.IsNullOrEmpty(data.WindowTitle))
            title += $" - {data.WindowTitle}";
        if (title.Length > 50) title = title[..47] + "...";
        g.DrawString(title, titleFont, Brushes.White, 16, y);
        y += (int)g.MeasureString(title, titleFont).Height + 4;

        // Separator line
        using var sepPen = new Pen(Color.FromArgb(40, accent.R, accent.G, accent.B), 1f);
        g.DrawLine(sepPen, 16, y, Width - 16, y);
        y += 10;

        // Element info
        if (data.Element != null)
        {
            using var infoFont = new Font("Segoe UI", 9f);
            var info = $"Type: {data.Element.ControlType}";
            if (!string.IsNullOrEmpty(data.Element.Name))
                info += $" | Name: {data.Element.Name}";
            if (!string.IsNullOrEmpty(data.Element.ClassName))
                info += $"\nClass: {data.Element.ClassName}";
            if (data.Element.IsEnabled)
                info += " | Enabled";
            if (data.Element.IsKeyboardFocusable)
                info += " | Focusable";

            using var infoBrush = new SolidBrush(Color.FromArgb(180, 180, 180));
            g.DrawString(info, infoFont, infoBrush, 16, y);
            y += (int)g.MeasureString(info, infoFont, Width - 32).Height + 8;
        }

        // Hints
        DrawHint(g, ref y, "GESTO", data.GestureHint, accent);
        DrawHint(g, ref y, "TIP", data.QualityTip, Color.FromArgb(255, 214, 0));
        DrawHint(g, ref y, "GUIA", data.MoveGuide, Color.FromArgb(0, 168, 255));
        DrawHint(g, ref y, "DATOS", data.DataInsight, Color.FromArgb(255, 107, 107));

        // AI Context
        if (!string.IsNullOrEmpty(data.AiContext))
        {
            y += 4;
            using var ctxFont = new Font("Segoe UI", 8.5f);
            using var ctxBrush = new SolidBrush(Color.FromArgb(120, 120, 120));
            var ctx = data.AiContext;
            if (ctx.Length > 80) ctx = ctx[..77] + "...";
            g.DrawString(ctx, ctxFont, ctxBrush, 16, y);
        }
    }

    private void DrawHint(Graphics g, ref int y, string label, string? value, Color color)
    {
        if (string.IsNullOrEmpty(value)) return;

        using var labelFont = new Font("Segoe UI", 7.5f, FontStyle.Bold);
        using var valueFont = new Font("Segoe UI", 8.5f);

        // Label badge
        var labelSize = g.MeasureString(label, labelFont);
        using var labelBg = new SolidBrush(Color.FromArgb(30, color.R, color.G, color.B));
        var labelRect = new Rectangle(16, y, (int)labelSize.Width + 8, (int)labelSize.Height + 2);
        using var labelPath = CreateRoundedRect(labelRect, 3);
        g.FillPath(labelBg, labelPath);
        using var labelFg = new SolidBrush(color);
        g.DrawString(label, labelFont, labelFg, 20, y + 1);

        // Value
        using var valueFg = new SolidBrush(Color.FromArgb(200, 200, 200));
        var displayValue = value;
        if (displayValue.Length > 60) displayValue = displayValue[..57] + "...";
        g.DrawString(displayValue, valueFont, valueFg, 16 + (int)labelSize.Width + 14, y);

        y += (int)labelSize.Height + 8;
    }

    private static System.Drawing.Drawing2D.GraphicsPath CreateRoundedRect(Rectangle rect, int radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        int d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
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

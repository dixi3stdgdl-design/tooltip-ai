using System.Drawing;
using System.Drawing.Drawing2D;

namespace TooltipAI.Platform.Win.Rendering;

/// <summary>
/// The Blade renderer — asymmetric vertical panel for enterprise.
/// 
/// Layout: 280x140px panel anchored opposite to cursor direction.
/// Style: Light frosted glass, high contrast, Inter/Segoe typography.
/// Voice: Progress bar fills left→right, shows product icon on completion.
/// </summary>
public static class TheBladeRenderer
{
    /// <summary>
    /// Draw The Blade overlay.
    /// </summary>
    public static void Draw(Graphics g, int width, int height, float opacity, float progress,
        string elementName, string context, string? actionText)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

        var rect = new Rectangle(0, 0, width, height);

        // Background: light frosted glass
        using var bgBrush = new SolidBrush(Color.FromArgb((int)(opacity * 230), 248, 249, 250));
        using var bgPath = CreateRoundedRect(rect, 12);
        g.FillPath(bgBrush, bgPath);

        // Border: subtle gray
        using var borderPen = new Pen(Color.FromArgb((int)(opacity * 100), 200, 200, 200), 1f);
        g.DrawPath(borderPen, bgPath);

        // Element name (top)
        using var nameFont = new Font("Segoe UI Semibold", 11f, FontStyle.Bold);
        using var nameBrush = new SolidBrush(Color.FromArgb((int)(opacity * 255), 31, 41, 55));
        g.DrawString(elementName, nameFont, nameBrush, 16, 16);

        // Context (middle)
        using var contextFont = new Font("Segoe UI", 9f, FontStyle.Regular);
        using var contextBrush = new SolidBrush(Color.FromArgb((int)(opacity * 200), 107, 114, 128));
        var contextRect = new RectangleF(16, 44, width - 32, 60);
        var contextFormat = new StringFormat
        {
            Trimming = StringTrimming.EllipsisWord,
            FormatFlags = StringFormatFlags.LineLimit
        };
        g.DrawString(context, contextFont, contextBrush, contextRect, contextFormat);

        // Action text (if present)
        if (!string.IsNullOrEmpty(actionText))
        {
            using var actionFont = new Font("Segoe UI Semibold", 10f, FontStyle.Bold);
            using var actionBrush = new SolidBrush(Color.FromArgb((int)(opacity * 255), 59, 130, 246));
            g.DrawString(actionText, actionFont, actionBrush, 16, height - 40);
        }

        // Progress bar (at bottom)
        DrawProgressBar(g, width, height, opacity, progress);
    }

    private static void DrawProgressBar(Graphics g, int width, int height, float opacity, float progress)
    {
        var barY = height - 12;
        var barHeight = 4;
        var barWidth = width - 32;
        var barX = 16;

        // Background track
        using var trackBrush = new SolidBrush(Color.FromArgb((int)(opacity * 50), 200, 200, 200));
        g.FillRectangle(trackBrush, barX, barY, barWidth, barHeight);

        // Progress fill
        if (progress > 0)
        {
            var fillWidth = (int)(barWidth * Math.Clamp(progress, 0, 1));
            using var fillBrush = new LinearGradientBrush(
                new Rectangle(barX, barY, fillWidth, barHeight),
                Color.FromArgb((int)(opacity * 200), 59, 130, 246),
                Color.FromArgb((int)(opacity * 200), 139, 92, 246),
                LinearGradientMode.Horizontal);
            g.FillRectangle(fillBrush, barX, barY, fillWidth, barHeight);
        }
    }

    private static GraphicsPath CreateRoundedRect(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        var diameter = radius * 2;

        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }
}

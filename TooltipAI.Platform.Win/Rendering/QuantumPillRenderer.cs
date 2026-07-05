using System.Drawing;
using System.Drawing.Drawing2D;

namespace TooltipAI.Platform.Win.Rendering;

/// <summary>
/// Quantum Pill renderer — compact, centered, Isla Dinámica inspired.
/// 
/// Layout: Symmetric horizontal pill below cursor (15px Y offset).
/// Style: Dark mica glass, 80% blur, neon gradient border (blue → violet).
/// Voice: Expands horizontally, shows waveform visualization.
/// </summary>
public static class QuantumPillRenderer
{
    /// <summary>
    /// Draw the Quantum Pill overlay.
    /// </summary>
    public static void Draw(Graphics g, int width, int height, float opacity, float[]? waveform, string text)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

        var rect = new Rectangle(0, 0, width, height);
        var pillRect = new Rectangle(4, 4, width - 8, height - 8);

        // Background: dark glass
        using var bgBrush = new SolidBrush(Color.FromArgb((int)(opacity * 204), 26, 26, 46)); // 80% opacity
        using var path = CreateRoundedRect(pillRect, 24);
        g.FillPath(bgBrush, path);

        // Border: neon gradient blue → violet
        using var borderPen = CreateGradientPen(pillRect, 0xFF3B82F6, 0xFF8B5CF6, opacity);
        g.DrawPath(borderPen, path);

        // Text
        if (!string.IsNullOrEmpty(text))
        {
            using var font = new Font("Segoe UI", 10f, FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.FromArgb((int)(opacity * 255), 255, 255, 255));
            var textSize = g.MeasureString(text, font);
            var textX = (width - textSize.Width) / 2;
            var textY = (height - textSize.Height) / 2;
            g.DrawString(text, font, textBrush, textX, textY);
        }

        // Waveform (when listening)
        if (waveform != null && waveform.Length > 0)
        {
            DrawWaveform(g, width, height, opacity, waveform);
        }
    }

    private static void DrawWaveform(Graphics g, int width, int height, float opacity, float[] data)
    {
        var barWidth = 2;
        var barGap = 2;
        var maxBars = width / (barWidth + barGap);
        var bars = Math.Min(data.Length, maxBars);

        var startX = 10;
        var centerY = height / 2;

        for (int i = 0; i < bars; i++)
        {
            var amplitude = data[i] * 12; // Scale to max 12px
            var barHeight = Math.Max(2, (int)amplitude);
            var x = startX + i * (barWidth + barGap);
            var y = centerY - barHeight / 2;

            using var barBrush = new SolidBrush(Color.FromArgb(
                (int)(opacity * 180),
                59, 130, 246)); // Blue
            g.FillRectangle(barBrush, x, y, barWidth, barHeight);
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

    private static Pen CreateGradientPen(Rectangle rect, uint colorStart, uint colorEnd, float opacity)
    {
        var startColor = Color.FromArgb((int)(opacity * 255),
            (int)((colorStart >> 16) & 0xFF),
            (int)((colorStart >> 8) & 0xFF),
            (int)(colorStart & 0xFF));

        var endColor = Color.FromArgb((int)(opacity * 255),
            (int)((colorEnd >> 16) & 0xFF),
            (int)((colorEnd >> 8) & 0xFF),
            (int)(colorEnd & 0xFF));

        var brush = new LinearGradientBrush(rect, startColor, endColor, LinearGradientMode.Horizontal);
        return new Pen(brush, 1.5f);
    }
}

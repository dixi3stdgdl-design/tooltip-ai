using System.Drawing;
using System.Drawing.Drawing2D;

namespace TooltipAI.Platform.Win.Rendering;

/// <summary>
/// Contextual Aura renderer — focus perimetral, no floating element.
/// 
/// Layout: Glowing frame that wraps the target UI element.
/// Style: No solid background, radial translucent blur, floating typography.
/// Voice: Frame "breathes" (pulsating opacity 40-90%), flash on execution.
/// </summary>
public static class ContextualAuraRenderer
{
    /// <summary>
    /// Draw the Contextual Aura overlay.
    /// </summary>
    public static void Draw(Graphics g, int x, int y, int elementWidth, int elementHeight,
        float opacity, float pulsePhase, string text)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

        var padding = 8;
        var auraRect = new Rectangle(
            x - padding,
            y - padding,
            elementWidth + padding * 2,
            elementHeight + padding * 2);

        // Outer glow (radial gradient)
        DrawGlow(g, auraRect, opacity, pulsePhase);

        // Border frame
        DrawBorderFrame(g, auraRect, opacity);

        // Floating text above
        if (!string.IsNullOrEmpty(text))
        {
            DrawFloatingText(g, x + elementWidth / 2, y - 20, text, opacity);
        }
    }

    private static void DrawGlow(Graphics g, Rectangle rect, float opacity, float phase)
    {
        // Create radial gradient glow
        var center = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        var radius = Math.Max(rect.Width, rect.Height) / 2 + 20;

        var colorStart = Color.FromArgb((int)(opacity * 80), 59, 130, 246); // Blue
        var colorEnd = Color.FromArgb(0, 59, 130, 246); // Transparent

        using var brush = new PathGradientBrush(CreateCirclePath(center, radius));
        brush.CenterColor = colorStart;
        brush.SurroundColors = new[] { colorEnd };

        g.FillEllipse(brush, center.X - radius, center.Y - radius, radius * 2, radius * 2);
    }

    private static void DrawBorderFrame(Graphics g, Rectangle rect, float opacity)
    {
        // Draw rounded rectangle border with gradient
        using var path = CreateRoundedRect(rect, 8);
        using var pen = new Pen(Color.FromArgb((int)(opacity * 150), 59, 130, 246), 2f);
        g.DrawPath(pen, path);
    }

    private static void DrawFloatingText(Graphics g, int centerX, int y, string text, float opacity)
    {
        using var font = new Font("Segoe UI", 9f, FontStyle.Regular);
        using var brush = new SolidBrush(Color.FromArgb((int)(opacity * 220), 255, 255, 255));

        var size = g.MeasureString(text, font);
        var x = centerX - size.Width / 2;

        // Text shadow
        using var shadowBrush = new SolidBrush(Color.FromArgb((int)(opacity * 100), 0, 0, 0));
        g.DrawString(text, font, shadowBrush, x + 1, y + 1);

        // Text
        g.DrawString(text, font, brush, x, y);
    }

    private static GraphicsPath CreateCirclePath(Point center, int radius)
    {
        var path = new GraphicsPath();
        path.AddEllipse(center.X - radius, center.Y - radius, radius * 2, radius * 2);
        return path;
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

    /// <summary>
    /// Draw execution flash effect.
    /// </summary>
    public static void DrawFlash(Graphics g, int x, int y, int width, int height, float intensity)
    {
        var rect = new Rectangle(x - 4, y - 4, width + 8, height + 8);

        using var brush = new SolidBrush(Color.FromArgb(
            (int)(intensity * 200),
            255, 255, 255));

        using var path = CreateRoundedRect(rect, 12);
        g.FillPath(brush, path);
    }
}

using System.Drawing;
using System.Drawing.Drawing2D;

namespace TooltipAI.Platform.Win.Rendering;

/// <summary>
/// Gaze Bracket renderer — minimal, high-performance, cyberpunk.
/// 
/// Layout: Four corner brackets [ ] framing the target element.
/// Style: Monochrome white, 75% opacity, zero textures, GDI native.
/// Voice: Brackets close magnetically, single blink on execution.
/// </summary>
public static class GazeBracketRenderer
{
    private const int BRACKET_SIZE = 12;
    private const int BRACKET_THICKNESS = 2;
    private const int PADDING = 8;

    /// <summary>
    /// Draw the Gaze Bracket overlay.
    /// </summary>
    public static void Draw(Graphics g, int elementWidth, int elementHeight, float opacity, string? text)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var color = Color.FromArgb((int)(opacity * 192), 255, 255, 255); // 75% white
        using var pen = new Pen(color, BRACKET_THICKNESS);

        var x = PADDING;
        var y = PADDING;
        var w = elementWidth;
        var h = elementHeight;

        // Top-left bracket
        DrawBracket(g, pen, x, y, BRACKET_SIZE, true, true);

        // Top-right bracket
        DrawBracket(g, pen, x + w - BRACKET_SIZE, y, BRACKET_SIZE, true, false);

        // Bottom-left bracket
        DrawBracket(g, pen, x, y + h - BRACKET_SIZE, BRACKET_SIZE, false, true);

        // Bottom-right bracket
        DrawBracket(g, pen, x + w - BRACKET_SIZE, y + h - BRACKET_SIZE, BRACKET_SIZE, false, false);

        // Status text (translucent)
        if (!string.IsNullOrEmpty(text))
        {
            using var font = new Font("Consolas", 8f, FontStyle.Regular);
            using var brush = new SolidBrush(Color.FromArgb((int)(opacity * 128), 255, 255, 255));
            g.DrawString(text, font, brush, x, y + h + 4);
        }
    }

    private static void DrawBracket(Graphics g, Pen pen, int x, int y, int size, bool isTop, bool isLeft)
    {
        int endX, endY;

        if (isTop && isLeft)
        {
            // ┌
            endX = x + size;
            endY = y + size;
            g.DrawLine(pen, x, y + size, x, y); // Vertical
            g.DrawLine(pen, x, y, x + size, y); // Horizontal
        }
        else if (isTop && !isLeft)
        {
            // ┐
            endX = x;
            endY = y + size;
            g.DrawLine(pen, x + size, y + size, x + size, y); // Vertical
            g.DrawLine(pen, x + size, y, x, y); // Horizontal
        }
        else if (!isTop && isLeft)
        {
            // └
            endX = x + size;
            endY = y;
            g.DrawLine(pen, x, y, x, y + size); // Vertical
            g.DrawLine(pen, x, y + size, x + size, y + size); // Horizontal
        }
        else
        {
            // ┘
            endX = x;
            endY = y;
            g.DrawLine(pen, x + size, y, x + size, y + size); // Vertical
            g.DrawLine(pen, x + size, y + size, x, y + size); // Horizontal
        }
    }

    /// <summary>
    /// Draw close animation — brackets move inward.
    /// </summary>
    public static void DrawClosing(Graphics g, int elementWidth, int elementHeight, float progress)
    {
        var offset = (int)(4 * (1 - progress)); // Move 4px inward
        var color = Color.FromArgb(192, 255, 255, 255);
        using var pen = new Pen(color, BRACKET_THICKNESS);

        var x = PADDING + offset;
        var y = PADDING + offset;
        var w = elementWidth - offset * 2;
        var h = elementHeight - offset * 2;

        DrawBracket(g, pen, x, y, BRACKET_SIZE, true, true);
        DrawBracket(g, pen, x + w - BRACKET_SIZE, y, BRACKET_SIZE, true, false);
        DrawBracket(g, pen, x, y + h - BRACKET_SIZE, BRACKET_SIZE, false, true);
        DrawBracket(g, pen, x + w - BRACKET_SIZE, y + h - BRACKET_SIZE, BRACKET_SIZE, false, false);
    }

    /// <summary>
    /// Draw blink effect on execution.
    /// </summary>
    public static void DrawBlink(Graphics g, int elementWidth, int elementHeight, float intensity)
    {
        var color = Color.FromArgb((int)(intensity * 255), 255, 255, 255);
        using var pen = new Pen(color, BRACKET_THICKNESS + 1);

        var x = PADDING;
        var y = PADDING;
        var w = elementWidth;
        var h = elementHeight;

        DrawBracket(g, pen, x, y, BRACKET_SIZE, true, true);
        DrawBracket(g, pen, x + w - BRACKET_SIZE, y, BRACKET_SIZE, true, false);
        DrawBracket(g, pen, x, y + h - BRACKET_SIZE, BRACKET_SIZE, false, true);
        DrawBracket(g, pen, x + w - BRACKET_SIZE, y + h - BRACKET_SIZE, BRACKET_SIZE, false, false);
    }
}

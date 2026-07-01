using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI;

namespace TooltipAI.UI.Rendering;

public class NoiseTextureRenderer
{
    private readonly Random _random = new();

    public void ApplyNoiseBackground(Canvas canvas, Color baseColor, float opacity = 0.05f)
    {
        var width = (float)canvas.ActualWidth;
        var height = (float)canvas.ActualHeight;
        if (width <= 0 || height <= 0) return;

        int dotCount = (int)(width * height * 0.002f);

        for (int i = 0; i < dotCount; i++)
        {
            float x = (float)(_random.NextDouble() * width);
            float y = (float)(_random.NextDouble() * height);
            float size = 1 + (float)(_random.NextDouble() * 2);

            float alpha = (float)(_random.NextDouble() * opacity);
            var dotColor = Color.FromArgb(
                (byte)(alpha * 255),
                baseColor.R,
                baseColor.G,
                baseColor.B
            );

            var dot = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = new SolidColorBrush(dotColor)
            };

            Canvas.SetLeft(dot, x);
            Canvas.SetTop(dot, y);
            canvas.Children.Add(dot);
        }
    }

    public void ApplyScanLines(Canvas canvas, Color lineColor, float opacity = 0.08f, int spacing = 4)
    {
        var width = (float)canvas.ActualWidth;
        var height = (float)canvas.ActualHeight;
        if (width <= 0 || height <= 0) return;

        var brush = new SolidColorBrush(Color.FromArgb(
            (byte)(opacity * 255),
            lineColor.R,
            lineColor.G,
            lineColor.B
        ));

        for (int y = 0; y < height; y += spacing)
        {
            var line = new Line
            {
                X1 = 0,
                Y1 = y,
                X2 = width,
                Y2 = y,
                Stroke = brush,
                StrokeThickness = 1
            };
            canvas.Children.Add(line);
        }
    }

    public void ApplyVignette(Canvas canvas, Color edgeColor, float intensity = 0.3f)
    {
        var width = (float)canvas.ActualWidth;
        var height = (float)canvas.ActualHeight;
        if (width <= 0 || height <= 0) return;

        int steps = 20;
        float stepWidth = width / steps / 2;
        float stepHeight = height / steps / 2;

        for (int i = 0; i < steps; i++)
        {
            float progress = (float)i / steps;
            float alpha = progress * progress * intensity;

            var brush = new SolidColorBrush(Color.FromArgb(
                (byte)(alpha * 255),
                edgeColor.R,
                edgeColor.G,
                edgeColor.B
            ));

            float currentWidth = width - i * stepWidth * 2;
            float currentHeight = height - i * stepHeight * 2;

            if (currentWidth <= 0 || currentHeight <= 0) break;

            var rect = new Rectangle
            {
                Width = currentWidth,
                Height = currentHeight,
                Stroke = brush,
                StrokeThickness = stepWidth * 2
            };

            Canvas.SetLeft(rect, i * stepWidth);
            Canvas.SetTop(rect, i * stepHeight);
            canvas.Children.Add(rect);
        }
    }
}

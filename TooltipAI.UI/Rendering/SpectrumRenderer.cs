using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI;

namespace TooltipAI.UI.Rendering;

public class SpectrumRenderer
{
    private const int BarCount = 32;

    public void Render(Canvas canvas, float[] data, Color color)
    {
        canvas.Children.Clear();

        var width = (float)canvas.ActualWidth;
        var height = (float)canvas.ActualHeight;
        if (width <= 0 || height <= 0) return;

        DrawGridLines(canvas, width, height);

        float[] normalized = NormalizeData(data, BarCount);
        float barWidth = (width - (BarCount - 1) * 2) / BarCount;
        float maxBarHeight = height - 16;

        for (int i = 0; i < BarCount; i++)
        {
            float barHeight = normalized[i] * maxBarHeight;
            float x = i * (barWidth + 2) + 4;
            float y = height - barHeight - 4;

            float intensity = normalized[i];
            var barColor = InterpolateColor(color, intensity);

            var glowRect = new Rectangle
            {
                Width = barWidth,
                Height = barHeight,
                Fill = new SolidColorBrush(barColor) { Opacity = 0.3f }
            };
            Canvas.SetLeft(glowRect, x - 1);
            Canvas.SetTop(glowRect, y - 1);
            canvas.Children.Add(glowRect);

            var rect = new Rectangle
            {
                Width = barWidth,
                Height = barHeight,
                Fill = new SolidColorBrush(barColor)
            };
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            canvas.Children.Add(rect);
        }

        DrawFrequencyLabels(canvas, width, height);
    }

    private float[] NormalizeData(float[] data, int targetCount)
    {
        var result = new float[targetCount];

        if (data.Length == 0) return result;

        float max = data.Max();
        if (max <= 0) max = 1f;

        for (int i = 0; i < targetCount; i++)
        {
            int srcIdx = (int)((float)i / targetCount * data.Length);
            srcIdx = Math.Min(srcIdx, data.Length - 1);
            result[i] = Math.Clamp(data[srcIdx] / max, 0f, 1f);
        }

        return result;
    }

    private Color InterpolateColor(Color baseColor, float intensity)
    {
        float r = baseColor.R * (0.4f + intensity * 0.6f);
        float g = baseColor.G * (0.4f + intensity * 0.6f);
        float b = baseColor.B * (0.4f + intensity * 0.6f);
        return Color.FromArgb(255, (byte)Math.Clamp(r, 0, 255), (byte)Math.Clamp(g, 0, 255), (byte)Math.Clamp(b, 0, 255));
    }

    private void DrawGridLines(Canvas canvas, float width, float height)
    {
        var gridBrush = new SolidColorBrush(Colors.Gray) { Opacity = 0.1f };

        for (int i = 1; i < 4; i++)
        {
            float y = height * i / 4;
            var line = new Line
            {
                X1 = 0, Y1 = y, X2 = width, Y2 = y,
                Stroke = gridBrush,
                StrokeThickness = 1
            };
            canvas.Children.Add(line);
        }
    }

    private void DrawFrequencyLabels(Canvas canvas, float width, float height)
    {
        var labelBrush = new SolidColorBrush(Colors.Gray) { Opacity = 0.4f };
        string[] labels = { "20", "200", "1k", "5k", "10k" };

        for (int i = 0; i < labels.Length; i++)
        {
            float x = width * i / (labels.Length - 1);
            var label = new TextBlock
            {
                Text = labels[i],
                Foreground = labelBrush,
                FontSize = 8,
                FontFamily = new FontFamily("Consolas")
            };
            Canvas.SetLeft(label, x - 8);
            Canvas.SetTop(label, height - 14);
            canvas.Children.Add(label);
        }
    }
}

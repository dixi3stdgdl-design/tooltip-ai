using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;
using Windows.UI;

namespace TooltipAI.UI.Rendering;

using Path = Microsoft.UI.Xaml.Shapes.Path;

public class CVRoutingRenderer
{
    public void Render(Canvas canvas, string source, string target, Color color)
    {
        canvas.Children.Clear();

        var width = (float)canvas.ActualWidth;
        var height = (float)canvas.ActualHeight;
        if (width <= 0 || height <= 0) return;

        var brush = new SolidColorBrush(color);
        var textBrush = new SolidColorBrush(Colors.White);

        float midY = height / 2;
        float leftX = 20;
        float rightX = width - 20;
        float boxWidth = 100;
        float boxHeight = 40;

        DrawNode(canvas, leftX, midY - boxHeight / 2, boxWidth, boxHeight, source, brush, textBrush);
        DrawNode(canvas, rightX - boxWidth, midY - boxHeight / 2, boxWidth, boxHeight, target, brush, textBrush);

        float lineStart = leftX + boxWidth + 8;
        float lineEnd = rightX - boxWidth - 8;

        var dashBrush = new SolidColorBrush(color) { Opacity = 0.6f };
        var line = new Line
        {
            X1 = lineStart, Y1 = midY,
            X2 = lineEnd - 12, Y2 = midY,
            Stroke = dashBrush,
            StrokeThickness = 2,
            StrokeDashArray = new DoubleCollection { 4, 4 }
        };
        canvas.Children.Add(line);

        DrawArrow(canvas, lineEnd - 12, midY, lineEnd, midY, brush);

        var label = new TextBlock
        {
            Text = "CV",
            Foreground = brush,
            FontSize = 10,
            FontFamily = new FontFamily("Consolas")
        };
        Canvas.SetLeft(label, (lineStart + lineEnd) / 2 - 8);
        Canvas.SetTop(label, midY - 20);
        canvas.Children.Add(label);
    }

    private void DrawNode(Canvas canvas, double x, double y, double width, double height,
        string text, SolidColorBrush borderBrush, SolidColorBrush textBrush)
    {
        var rect = new Border
        {
            Width = width,
            Height = height,
            BorderBrush = borderBrush,
            BorderThickness = new Thickness(1.5),
            Background = new SolidColorBrush(Colors.Transparent),
            CornerRadius = new CornerRadius(4)
        };
        Canvas.SetLeft(rect, x);
        Canvas.SetTop(rect, y);
        canvas.Children.Add(rect);

        var label = new TextBlock
        {
            Text = text,
            Foreground = textBrush,
            FontSize = 9,
            FontFamily = new FontFamily("Consolas"),
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = width - 8,
            TextAlignment = TextAlignment.Center
        };
        Canvas.SetLeft(label, x + 4);
        Canvas.SetTop(label, y + height / 2 - 8);
        canvas.Children.Add(label);
    }

    private void DrawArrow(Canvas canvas, double x1, double y1, double x2, double y2, SolidColorBrush brush)
    {
        var path = new Path
        {
            Stroke = brush,
            StrokeThickness = 2,
            Data = CreateArrowGeometry(x1, y1, x2, y2, 8)
        };
        canvas.Children.Add(path);
    }

    private Geometry CreateArrowGeometry(double x1, double y1, double x2, double y2, double size)
    {
        double angle = Math.Atan2(y2 - y1, x2 - x1);
        double a1 = angle + Math.PI * 0.8;
        double a2 = angle - Math.PI * 0.8;

        var figure = new PathFigure { StartPoint = new Point(x2, y2), IsClosed = true };
        figure.Segments.Add(new LineSegment { Point = new Point(x2 + size * Math.Cos(a1), y2 + size * Math.Sin(a1)) });
        figure.Segments.Add(new LineSegment { Point = new Point(x2 + size * Math.Cos(a2), y2 + size * Math.Sin(a2)) });

        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);
        return geometry;
    }
}

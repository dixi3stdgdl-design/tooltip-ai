using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;
using Windows.UI;

namespace TooltipAI.UI.Rendering;

using Path = Microsoft.UI.Xaml.Shapes.Path;

public class WaveformRenderer
{
    public void Render(Canvas canvas, float[] data, Color color)
    {
        canvas.Children.Clear();

        if (data.Length < 2) return;

        var width = (float)canvas.ActualWidth;
        var height = (float)canvas.ActualHeight;
        if (width <= 0 || height <= 0) return;

        var brush = new SolidColorBrush(color);
        var glowBrush = new SolidColorBrush(color) { Opacity = 0.4f };

        var points = new Point[data.Length];
        float midY = height / 2;

        for (int i = 0; i < data.Length; i++)
        {
            float x = (float)i / (data.Length - 1) * width;
            float y = midY + data[i] * midY * 0.8f;
            points[i] = new Point(x, y);
        }

        var glowPath = new Path
        {
            Stroke = glowBrush,
            StrokeThickness = 4,
            StrokeLineJoin = PenLineJoin.Round,
            Data = CreatePolylineGeometry(points)
        };
        canvas.Children.Add(glowPath);

        var mainPath = new Path
        {
            Stroke = brush,
            StrokeThickness = 2,
            StrokeLineJoin = PenLineJoin.Round,
            Data = CreatePolylineGeometry(points)
        };
        canvas.Children.Add(mainPath);

        DrawGridLines(canvas, width, height, brush);
    }

    private Geometry CreatePolylineGeometry(Point[] points)
    {
        var figure = new PathFigure
        {
            StartPoint = points[0],
            IsClosed = false
        };

        for (int i = 1; i < points.Length; i++)
        {
            figure.Segments.Add(new LineSegment { Point = points[i] });
        }

        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);
        return geometry;
    }

    private void DrawGridLines(Canvas canvas, float width, float height, SolidColorBrush brush)
    {
        var gridBrush = new SolidColorBrush(Colors.Gray) { Opacity = 0.15f };

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
}

using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using TooltipAI.Core.Models;
using TooltipAI.Core.Services;
using TooltipAI.UI.Rendering;

namespace TooltipAI.UI.Views;

public sealed partial class TooltipOverlay : UserControl
{
    private readonly WaveformRenderer _waveformRenderer = new();
    private readonly CVRoutingRenderer _cvRenderer = new();
    private readonly SpectrumRenderer _spectrumRenderer = new();
    private readonly NoiseTextureRenderer _noiseRenderer = new();

    private TooltipData? _currentData;
    private bool _isVisible;

    public TooltipOverlay()
    {
        this.InitializeComponent();
    }

    public void UpdateFromData(TooltipData data)
    {
        _currentData = data;
        _isVisible = true;

        var category = Enum.TryParse<SoftwareCategory>(data.SoftwareCategory, out var cat)
            ? cat : SoftwareCategory.Unknown;
        var theme = TooltipColorTheme.GetTheme(category);

        ApplyColorTheme(theme);
        ApplyHeader(data);
        ApplyInfoPanels(data);
        ApplyFooter(data);
        RenderVisualization(data, theme);

        this.Visibility = Visibility.Visible;
    }

    public void Hide()
    {
        _isVisible = false;
        this.Visibility = Visibility.Collapsed;
        VizCanvas.Children.Clear();
        NoiseOverlay.Children.Clear();
    }

    public bool IsVisibleState => _isVisible;

    private void ApplyColorTheme(TooltipColorTheme theme)
    {
        var primary = ColorFromUint(theme.BorderPrimary);
        var accent = ColorFromUint(theme.AccentColor);
        var glow = ColorFromUint(theme.GlowOuter);
        var bgStart = ColorFromUint(theme.BackgroundStart);
        var bgEnd = ColorFromUint(theme.BackgroundEnd);

        // Main border
        MainBorder.BorderBrush = new SolidColorBrush(primary) { Opacity = 0.7f };
        MainBorder.Background = new LinearGradientBrush
        {
            StartPoint = new Windows.Foundation.Point(0, 0),
            EndPoint = new Windows.Foundation.Point(0, 1),
            GradientStops =
            {
                new GradientStop { Color = bgStart, Offset = 0 },
                new GradientStop { Color = bgEnd, Offset = 1 }
            }
        };

        // Glow outer border (layered behind)
        GlowBorder.BorderBrush = new SolidColorBrush(glow) { Opacity = 0.6f };
        GlowBorder.Background = new SolidColorBrush(Colors.Transparent);

        // Category badge
        CategoryBadge.Background = new SolidColorBrush(accent) { Opacity = 0.15f };
        CategoryBadge.BorderBrush = new SolidColorBrush(accent) { Opacity = 0.4f };
        CategoryBadge.BorderThickness = new Thickness(1);

        // Separator
        SepColor1.Color = accent;
        SepColor2.Color = accent;

        // Info row badges and text
        GestureBadge.Background = new SolidColorBrush(accent) { Opacity = 0.15f };
        GestureText.Foreground = new SolidColorBrush(accent) { Opacity = 0.9f };

        var yellow = ColorFromUint(0x00FFD600);
        TipBadge.Background = new SolidColorBrush(yellow) { Opacity = 0.12f };
        TipText.Foreground = new SolidColorBrush(yellow) { Opacity = 0.9f };

        var blue = ColorFromUint(0x0000A8FF);
        GuideBadge.Background = new SolidColorBrush(blue) { Opacity = 0.12f };
        GuideText.Foreground = new SolidColorBrush(blue) { Opacity = 0.9f };

        var red = ColorFromUint(0x00FF6B6B);
        InsightBadge.Background = new SolidColorBrush(red) { Opacity = 0.12f };
        InsightText.Foreground = new SolidColorBrush(red) { Opacity = 0.9f };

        // Viz border
        VizBorder.BorderBrush = new SolidColorBrush(primary) { Opacity = 0.2f };

        // Noise overlay
        ApplyNoiseOverlay(accent);
    }

    private void ApplyHeader(TooltipData data)
    {
        var category = Enum.TryParse<SoftwareCategory>(data.SoftwareCategory, out var cat)
            ? cat : SoftwareCategory.Unknown;
        var theme = TooltipColorTheme.GetTheme(category);

        CategoryIcon.Text = theme.CategoryIcon;
        CategoryIcon.Foreground = new SolidColorBrush(ColorFromUint(theme.AccentColor));
        CategoryLabel.Text = theme.CategoryLabel;
        CategoryLabel.Foreground = new SolidColorBrush(ColorFromUint(theme.AccentColor));

        var moduleName = !string.IsNullOrEmpty(data.ProcessName) ? data.ProcessName : "Unknown";
        TitleText.Text = moduleName;

        if (!string.IsNullOrEmpty(data.WindowTitle))
        {
            TitleText.Text += $" \u2022 {data.WindowTitle}";
        }
    }

    private void ApplyInfoPanels(TooltipData data)
    {
        SetInfoRow(GestureRow, GestureText, data.GestureHint);
        SetInfoRow(TipRow, TipText, data.QualityTip);
        SetInfoRow(GuideRow, GuideText, data.MoveGuide);
        SetInfoRow(InsightRow, InsightText, data.DataInsight);
    }

    private void SetInfoRow(Grid row, TextBlock textBlock, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            row.Visibility = Visibility.Collapsed;
            return;
        }
        textBlock.Text = value;
        row.Visibility = Visibility.Visible;
    }

    private void ApplyFooter(TooltipData data)
    {
        if (!string.IsNullOrEmpty(data.AiContext))
        {
            ContextText.Text = data.AiContext;
            ContextText.Visibility = Visibility.Visible;
        }
        else
        {
            ContextText.Visibility = Visibility.Collapsed;
        }

        if (!string.IsNullOrEmpty(data.AiDescription))
        {
            DescriptionText.Text = data.AiDescription;
            DescriptionText.Visibility = Visibility.Visible;
        }
        else
        {
            DescriptionText.Visibility = Visibility.Collapsed;
        }
    }

    private void RenderVisualization(TooltipData data, TooltipColorTheme theme)
    {
        VizCanvas.Children.Clear();

        if (data.VisualType == 0 && data.WaveformData is null && data.SpectrumData is null
            && string.IsNullOrEmpty(data.CVSource))
        {
            VizBorder.Visibility = Visibility.Collapsed;
            return;
        }

        VizBorder.Visibility = Visibility.Visible;

        var accent = ColorFromUint(theme.AccentColor);

        switch (data.VisualType)
        {
            case 1:
                if (data.WaveformData is { Length: > 1 })
                    _waveformRenderer.Render(VizCanvas, data.WaveformData, accent);
                break;

            case 2:
                if (!string.IsNullOrEmpty(data.CVSource) && !string.IsNullOrEmpty(data.CVTarget))
                    _cvRenderer.Render(VizCanvas, data.CVSource, data.CVTarget, accent);
                break;

            case 3:
                if (data.SpectrumData is { Length: > 0 })
                    _spectrumRenderer.Render(VizCanvas, data.SpectrumData, accent);
                break;
        }
    }

    private void ApplyNoiseOverlay(Color color)
    {
        NoiseOverlay.Children.Clear();
        if (VizBorder.ActualWidth > 0 && VizBorder.ActualHeight > 0)
        {
            _noiseRenderer.ApplyScanLines(NoiseOverlay, color, 0.04f, 3);
        }
    }

    private static Color ColorFromUint(uint color)
    {
        byte r = (byte)((color >> 16) & 0xFF);
        byte g = (byte)((color >> 8) & 0xFF);
        byte b = (byte)(color & 0xFF);
        return Color.FromArgb(255, r, g, b);
    }
}

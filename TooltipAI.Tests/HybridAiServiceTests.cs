using TooltipAI.Core.Interfaces;
using TooltipAI.Core.Models;
using TooltipAI.Core.Services;
using Xunit;

namespace TooltipAI.Tests;

/// <summary>
/// Tests for LocalContextEnricher - NO cloud API dependencies.
/// All tests verify local-only context enrichment.
/// </summary>
public class LocalContextEnricherTests
{
    private readonly LocalContextEnricher _enricher;

    public LocalContextEnricherTests()
    {
        _enricher = new LocalContextEnricher();
    }

    [Fact]
    public void GetEnrichedContext_WithBasicElement_ReturnsFormattedString()
    {
        var element = new ElementInfo
        {
            ControlType = "Button",
            ClassName = "QPushButton",
            IsEnabled = true,
            IsKeyboardFocusable = true
        };

        var result = _enricher.GetEnrichedContext(element);

        Assert.Contains("Button", result);
        Assert.Contains("QPushButton", result);
        Assert.Contains("Enabled", result);
        Assert.Contains("Focusable", result);
    }

    [Fact]
    public void GetEnrichedContext_WithHelpText_IncludesHelpText()
    {
        var element = new ElementInfo
        {
            ControlType = "Button",
            HelpText = "Click to save"
        };

        var result = _enricher.GetEnrichedContext(element);

        Assert.Contains("Click to save", result);
    }

    [Fact]
    public void GetFunctionHint_ForButton_ReturnsClickInstruction()
    {
        var element = new ElementInfo { ControlType = "Button" };

        var result = _enricher.GetFunctionHint(element);

        Assert.Contains("click", result.ToLower());
    }

    [Fact]
    public void GetFunctionHint_ForEdit_ReturnsInputInstruction()
    {
        var element = new ElementInfo { ControlType = "Edit" };

        var result = _enricher.GetFunctionHint(element);

        Assert.Contains("type", result.ToLower());
    }

    [Fact]
    public void GetGestureHint_ForSlider_ReturnsDragInstruction()
    {
        var element = new ElementInfo { ControlType = "Slider" };

        var result = _enricher.GetGestureHint(element, SoftwareCategory.Unknown);

        Assert.Contains("Drag", result);
    }

    [Fact]
    public void GetGestureHint_ForButton_ReturnsClickInstruction()
    {
        var element = new ElementInfo { ControlType = "Button" };

        var result = _enricher.GetGestureHint(element, SoftwareCategory.Unknown);

        Assert.Contains("Click", result);
    }

    [Fact]
    public void GetQualityTip_ForAudioSlider_ReturnsTuningHint()
    {
        var element = new ElementInfo
        {
            ControlType = "Slider",
            ClassName = "VolumeSlider"
        };

        var result = _enricher.GetQualityTip(element, SoftwareCategory.Audio);

        Assert.Contains("slowly", result.ToLower());
    }

    [Fact]
    public void GetMoveGuide_ForSlider_ReturnsDragInstruction()
    {
        var element = new ElementInfo { ControlType = "Slider" };

        var result = _enricher.GetMoveGuide(element, SoftwareCategory.Unknown);

        Assert.Contains("Drag", result);
    }

    [Fact]
    public void GetDataInsight_WithHelpText_ReturnsHelpText()
    {
        var element = new ElementInfo { HelpText = "Loading 50%" };

        var result = _enricher.GetDataInsight(element, SoftwareCategory.Unknown);

        Assert.Equal("Loading 50%", result);
    }

    [Fact]
    public void GetDataInsight_ForProgressBar_ReturnsLoadingMessage()
    {
        var element = new ElementInfo { ControlType = "progress" };

        var result = _enricher.GetDataInsight(element, SoftwareCategory.Unknown);

        Assert.Contains("Loading", result);
    }
}

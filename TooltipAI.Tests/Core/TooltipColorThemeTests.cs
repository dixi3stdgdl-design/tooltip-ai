using FluentAssertions;
using TooltipAI.Core.Services;
using Xunit;

namespace TooltipAI.Tests.Core;

public class TooltipColorThemeTests
{
    [Theory]
    [InlineData(SoftwareCategory.WindowsSystem, "WINDOWS")]
    [InlineData(SoftwareCategory.WindowsShell, "SHELL")]
    [InlineData(SoftwareCategory.Development, "DEV")]
    [InlineData(SoftwareCategory.Creative, "CREATIVE")]
    [InlineData(SoftwareCategory.Audio, "AUDIO")]
    [InlineData(SoftwareCategory.Video, "VIDEO")]
    [InlineData(SoftwareCategory.Browser, "BROWSER")]
    [InlineData(SoftwareCategory.Terminal, "TERMINAL")]
    [InlineData(SoftwareCategory.Office, "OFFICE")]
    [InlineData(SoftwareCategory.Gaming, "GAMING")]
    [InlineData(SoftwareCategory.Security, "SECURITY")]
    [InlineData(SoftwareCategory.Network, "NETWORK")]
    [InlineData(SoftwareCategory.FileExplorer, "FILES")]
    [InlineData(SoftwareCategory.Settings, "SETTINGS")]
    [InlineData(SoftwareCategory.TextEditor, "EDITOR")]
    [InlineData(SoftwareCategory.Unknown, "APP")]
    public void GetTheme_KnownCategory_ReturnsExpectedLabel(SoftwareCategory category, string expectedLabel)
    {
        var theme = TooltipColorTheme.GetTheme(category);

        theme.Should().NotBeNull();
        theme.CategoryLabel.Should().Be(expectedLabel);
    }

    [Theory]
    [InlineData(SoftwareCategory.WindowsSystem)]
    [InlineData(SoftwareCategory.Development)]
    [InlineData(SoftwareCategory.Gaming)]
    [InlineData(SoftwareCategory.Unknown)]
    public void GetTheme_AnyKnownCategory_HasNonEmptyIconAndFont(SoftwareCategory category)
    {
        var theme = TooltipColorTheme.GetTheme(category);

        theme.CategoryIcon.Should().NotBeNullOrEmpty();
        theme.FontFamily.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetTheme_UndefinedCategory_FallsBackToUnknownTheme()
    {
        var undefined = (SoftwareCategory)9999;

        var theme = TooltipColorTheme.GetTheme(undefined);

        theme.Should().BeSameAs(TooltipColorTheme.GetTheme(SoftwareCategory.Unknown));
        theme.CategoryLabel.Should().Be("APP");
    }

    [Fact]
    public void GetTheme_SameCategory_ReturnsCachedInstance()
    {
        var first = TooltipColorTheme.GetTheme(SoftwareCategory.Audio);
        var second = TooltipColorTheme.GetTheme(SoftwareCategory.Audio);

        first.Should().BeSameAs(second);
    }

    [Fact]
    public void DefaultFontFamily_IsConsolas()
    {
        var theme = new TooltipColorTheme();

        theme.FontFamily.Should().Be("Consolas");
    }

    [Fact]
    public void LerpColor_TZero_ReturnsFirstColorRgb()
    {
        // Alpha channel is dropped by LerpColor; compare RGB only.
        uint result = TooltipColorTheme.LerpColor(0xFF102030, 0xFF405060, 0f);

        result.Should().Be(0x102030);
    }

    [Fact]
    public void LerpColor_TOne_ReturnsSecondColorRgb()
    {
        uint result = TooltipColorTheme.LerpColor(0xFF102030, 0xFF405060, 1f);

        result.Should().Be(0x405060);
    }

    [Fact]
    public void LerpColor_TMidpoint_ReturnsBlendedChannels()
    {
        // Halfway between 0x00 and 0x40 (64) is 0x20 (32) per channel.
        uint result = TooltipColorTheme.LerpColor(0xFF000000, 0xFF404040, 0.5f);

        var r = (result >> 16) & 0xFF;
        var g = (result >> 8) & 0xFF;
        var b = result & 0xFF;

        r.Should().Be(0x20);
        g.Should().Be(0x20);
        b.Should().Be(0x20);
    }

    [Fact]
    public void LerpColor_IdenticalColors_ReturnsSameRgb()
    {
        uint result = TooltipColorTheme.LerpColor(0xFFAABBCC, 0xFFAABBCC, 0.42f);

        result.Should().Be(0xAABBCC);
    }
}

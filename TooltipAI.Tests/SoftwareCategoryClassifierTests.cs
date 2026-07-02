using TooltipAI.Core.Services;
using Xunit;

namespace TooltipAI.Tests;

public class SoftwareCategoryClassifierTests
{
    private readonly SoftwareCategoryClassifier _classifier;

    public SoftwareCategoryClassifierTests()
    {
        _classifier = new SoftwareCategoryClassifier();
    }

    [Theory]
    [InlineData("Ableton Live", "Ableton", SoftwareCategory.Audio)]
    [InlineData("FL Studio", "FL", SoftwareCategory.Audio)]
    [InlineData("Adobe Audition", "Audition", SoftwareCategory.Audio)]
    [InlineData("Cubase", "Cubase", SoftwareCategory.Audio)]
    [InlineData("Logic Pro", "Logic", SoftwareCategory.Audio)]
    [InlineData("Reaper", "Reaper", SoftwareCategory.Audio)]
    public void Classify_AudioSoftware_ReturnsAudio(string title, string process, SoftwareCategory expected)
    {
        var result = _classifier.Classify("", title, process);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Adobe Photoshop", "Photoshop", SoftwareCategory.Creative)]
    [InlineData("Blender", "Blender", SoftwareCategory.Creative)]
    [InlineData("Figma", "Figma", SoftwareCategory.Creative)]
    [InlineData("Canva", "Canva", SoftwareCategory.Creative)]
    [InlineData("Inkscape", "Inkscape", SoftwareCategory.Creative)]
    public void Classify_CreativeSoftware_ReturnsCreative(string title, string process, SoftwareCategory expected)
    {
        var result = _classifier.Classify("", title, process);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Visual Studio Code", "Code", SoftwareCategory.Development)]
    [InlineData("IntelliJ IDEA", "idea64", SoftwareCategory.Development)]
    [InlineData("Android Studio", "studio", SoftwareCategory.Development)]
    [InlineData("PyCharm", "pycharm", SoftwareCategory.Development)]
    public void Classify_DevSoftware_ReturnsDevelopment(string title, string process, SoftwareCategory expected)
    {
        var result = _classifier.Classify("", title, process);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Windows Terminal", "WindowsTerminal", SoftwareCategory.Terminal)]
    [InlineData("cmd.exe", "cmd", SoftwareCategory.Terminal)]
    [InlineData("PowerShell", "pwsh", SoftwareCategory.Terminal)]
    public void Classify_TerminalSoftware_ReturnsTerminal(string title, string process, SoftwareCategory expected)
    {
        var result = _classifier.Classify("", title, process);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Google Chrome", "chrome", SoftwareCategory.Browser)]
    [InlineData("Mozilla Firefox", "firefox", SoftwareCategory.Browser)]
    [InlineData("Microsoft Edge", "msedge", SoftwareCategory.Browser)]
    [InlineData("Brave Browser", "brave", SoftwareCategory.Browser)]
    public void Classify_BrowserSoftware_ReturnsBrowser(string title, string process, SoftwareCategory expected)
    {
        var result = _classifier.Classify("", title, process);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("VLC media player", "vlc", SoftwareCategory.Video)]
    [InlineData("OBS Studio", "obs64", SoftwareCategory.Video)]
    [InlineData("DaVinci Resolve", "resolve", SoftwareCategory.Video)]
    public void Classify_VideoSoftware_ReturnsVideo(string title, string process, SoftwareCategory expected)
    {
        var result = _classifier.Classify("", title, process);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Classify_UnknownSoftware_ReturnsUnknown()
    {
        var result = _classifier.Classify("RandomWindow", "RandomApp", "random.exe");
        Assert.Equal(SoftwareCategory.Unknown, result);
    }

    [Fact]
    public void Classify_EmptyInputs_ReturnsUnknown()
    {
        var result = _classifier.Classify("", "", "");
        Assert.Equal(SoftwareCategory.Unknown, result);
    }

    [Fact]
    public void GetProcessNameFromHwnd_WithInvalidHwnd_ReturnsNull()
    {
        var result = _classifier.GetProcessNameFromHwnd(IntPtr.Zero);
        Assert.Null(result);
    }
}

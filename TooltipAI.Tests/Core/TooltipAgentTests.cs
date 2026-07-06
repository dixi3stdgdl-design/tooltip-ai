using FluentAssertions;
using Xunit;
using TooltipAI.Core.Agent;
using TooltipAI.Core.Models;

namespace TooltipAI.Tests.Core;

public class TooltipAgentTests : IDisposable
{
    private readonly TooltipAgent _agent;
    private readonly string _tempSettingsPath;
    private readonly string _tempRulesPath;
    private readonly string _tempCachePath;

    public TooltipAgentTests()
    {
        _tempSettingsPath = Path.Combine(Path.GetTempPath(), $"settings_test_{Guid.NewGuid()}.json");
        _tempRulesPath = Path.Combine(Path.GetTempPath(), $"rules_test_{Guid.NewGuid()}.json");
        _tempCachePath = Path.Combine(Path.GetTempPath(), $"cache_test_{Guid.NewGuid()}.db");

        File.Copy(
            Path.Combine(AppContext.BaseDirectory, "Rules", "rules.json"),
            _tempRulesPath,
            true);

        _agent = new TooltipAgent(_tempSettingsPath, _tempRulesPath, _tempCachePath);
    }

    public void Dispose()
    {
        _agent.Dispose();
        if (File.Exists(_tempSettingsPath))
            File.Delete(_tempSettingsPath);
        if (File.Exists(_tempRulesPath))
            File.Delete(_tempRulesPath);
        if (File.Exists(_tempCachePath))
            File.Delete(_tempCachePath);
    }

    [Fact]
    public void ShouldInitializeWithDefaultSettings()
    {
        _agent.IsEnabled.Should().BeTrue();
        _agent.RuleCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ShouldProcessElementAndReturnTooltipData()
    {
        var element = CreateTestElement("Save", "excel", "Button");

        var result = _agent.ProcessElement(element);

        result.Should().NotBeNull();
        result!.Element.Name.Should().Be("Save");
        result.EnrichedContext.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ShouldReturnCachedResultForSameElement()
    {
        var element = CreateTestElement("Save", "excel", "Button");

        var result1 = _agent.ProcessElement(element);
        var result2 = _agent.ProcessElement(element);

        result1.Should().NotBeNull();
        result2.Should().BeNull();
    }

    [Fact]
    public void ShouldReturnNullWhenDisabled()
    {
        _agent.Disable();

        var element = CreateTestElement("Save", "excel", "Button");
        var result = _agent.ProcessElement(element);

        result.Should().BeNull();
    }

    [Fact]
    public void ShouldEnableAfterDisable()
    {
        _agent.Disable();
        _agent.Enable();

        var element = CreateTestElement("Save", "excel", "Button");
        var result = _agent.ProcessElement(element);

        result.Should().NotBeNull();
    }

    [Fact]
    public void ShouldProcessDifferentElements()
    {
        var element1 = CreateTestElement("Save", "excel", "Button");
        var element2 = CreateTestElement("Open", "excel", "Button");

        var result1 = _agent.ProcessElement(element1);
        var result2 = _agent.ProcessElement(element2);

        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1!.Element.Name.Should().Be("Save");
        result2!.Element.Name.Should().Be("Open");
    }

    [Fact]
    public void ShouldClearCache()
    {
        var element = CreateTestElement("Test", "app", "Button");
        _agent.ProcessElement(element);

        _agent.ClearCache();

        var stats = _agent.GetCacheStats();
        stats.TotalEntries.Should().Be(0);
    }

    [Fact]
    public void ShouldReturnCacheStats()
    {
        _agent.ProcessElement(CreateTestElement("Test1", "app", "Button"));
        _agent.ProcessElement(CreateTestElement("Test2", "app", "Button"));

        var stats = _agent.GetCacheStats();

        stats.TotalEntries.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ShouldHandleNullElement()
    {
        var result = _agent.ProcessElement(null!);

        result.Should().NotBeNull();
        result!.Element.Name.Should().Be("(unknown)");
    }

    [Fact]
    public void ShouldHandleEmptyNameElement()
    {
        var element = new ElementInfo
        {
            Name = "",
            ProcessName = "app",
            ControlType = "Button"
        };

        var result = _agent.ProcessElement(element);

        result.Should().NotBeNull();
        result!.Element.Name.Should().Be("(unknown)");
    }

    [Fact]
    public void ShouldClassifyOfficeApps()
    {
        var element = CreateTestElement("Test", "excel", "Button");

        var result = _agent.ProcessElement(element);

        result!.SoftwareCategory.Should().Be("Office");
    }

    [Fact]
    public void ShouldClassifyBrowserApps()
    {
        var element = CreateTestElement("Test", "chrome", "Button");

        var result = _agent.ProcessElement(element);

        result!.SoftwareCategory.Should().Be("Browser");
    }

    [Fact]
    public void ShouldClassifyIDEApps()
    {
        var element = CreateTestElement("Test", "code", "Button");

        var result = _agent.ProcessElement(element);

        result!.SoftwareCategory.Should().Be("IDE");
    }

    [Fact]
    public void ShouldFireTooltipReadyEvent()
    {
        TooltipData? receivedData = null;
        _agent.TooltipReady += data => receivedData = data;

        var element = CreateTestElement("Save", "excel", "Button");
        _agent.ProcessElement(element);

        receivedData.Should().NotBeNull();
        receivedData!.Element.Name.Should().Be("Save");
    }

    [Fact]
    public void ShouldFireTooltipHiddenEvent()
    {
        bool hiddenFired = false;
        _agent.TooltipHidden += () => hiddenFired = true;

        _agent.HideTooltip();

        hiddenFired.Should().BeTrue();
    }

    [Fact]
    public void ShouldUpdateSettings()
    {
        _agent.UpdateSettings(s => s.TooltipDelayMs = 200);

        var settings = _agent.GetSettings();
        settings.TooltipDelayMs.Should().Be(200);
    }

    [Fact]
    public void ShouldReturnDefaultContextForUnknownApp()
    {
        var element = CreateTestElement("CustomControl", "unknown_app", "Custom");

        var result = _agent.ProcessElement(element);

        result.Should().NotBeNull();
        result!.EnrichedContext.Should().NotBeNullOrEmpty();
    }

    private ElementInfo CreateTestElement(string name, string processName, string controlType)
    {
        return new ElementInfo
        {
            Name = name,
            ProcessName = processName,
            ControlType = controlType,
            ClassName = "TestClass",
            WindowTitle = "Test Window",
            IsEnabled = true,
            IsKeyboardFocusable = true,
            Timestamp = DateTime.UtcNow
        };
    }
}

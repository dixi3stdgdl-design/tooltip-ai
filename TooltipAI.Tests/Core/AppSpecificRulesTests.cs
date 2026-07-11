using FluentAssertions;
using Xunit;
using TooltipAI.Core.Models;
using TooltipAI.Core.Rules;

namespace TooltipAI.Tests.Core;

public class AppSpecificRulesTests : IDisposable
{
    private readonly AppSpecificRules _rules;
    private readonly string _tempRulesPath;

    public AppSpecificRulesTests()
    {
        _tempRulesPath = Path.Combine(Path.GetTempPath(), $"rules_test_{Guid.NewGuid()}.json");
        File.Copy(
            Path.Combine(AppContext.BaseDirectory, "Rules", "rules.json"),
            _tempRulesPath,
            true);
        _rules = new AppSpecificRules(_tempRulesPath);
    }

    public void Dispose()
    {
        _rules.Dispose();
        if (File.Exists(_tempRulesPath))
            File.Delete(_tempRulesPath);
    }

    [Fact]
    public void ShouldLoadRulesFromJson()
    {
        _rules.RuleCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ShouldMatchExcelFormatCells()
    {
        var element = new ElementInfo
        {
            Name = "Format Cells",
            ProcessName = "excel",
            ControlType = "Button",
            ClassName = ""
        };

        var context = _rules.GetContextForElement(element);

        context.Should().NotBeNullOrEmpty();
        context.Should().Contain("Format Cells");
    }

    [Fact]
    public void ShouldMatchChromeAddressBar()
    {
        var element = new ElementInfo
        {
            Name = "Address bar",
            ProcessName = "chrome",
            ControlType = "Edit",
            ClassName = "Edit"
        };

        var context = _rules.GetContextForElement(element);

        context.Should().NotBeNullOrEmpty();
        context.Should().Contain("URL");
    }

    [Fact]
    public void ShouldMatchVSCodeTerminal()
    {
        var element = new ElementInfo
        {
            Name = "Terminal",
            ProcessName = "code",
            ControlType = "Window",
            ClassName = ""
        };

        var context = _rules.GetContextForElement(element);

        context.Should().NotBeNullOrEmpty();
        context.Should().Contain("terminal");
    }

    [Fact]
    public void ShouldReturnShortcutForKnownElement()
    {
        var element = new ElementInfo
        {
            Name = "Save",
            ProcessName = "excel",
            ControlType = "Button",
            ClassName = ""
        };

        var shortcut = _rules.GetShortcutForElement(element);

        shortcut.Should().Be("Ctrl+S");
    }

    [Fact]
    public void ShouldReturnNullShortcutForUnknownElement()
    {
        var element = new ElementInfo
        {
            Name = "Unknown Control",
            ProcessName = "unknown_app",
            ControlType = "Window",
            ClassName = ""
        };

        var shortcut = _rules.GetShortcutForElement(element);

        shortcut.Should().BeNull();
    }

    [Fact]
    public void ShouldMatchGenericButton()
    {
        var element = new ElementInfo
        {
            Name = "Submit",
            ProcessName = "any_app",
            ControlType = "Button",
            ClassName = ""
        };

        var context = _rules.GetContextForElement(element);

        context.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ShouldGenerateDefaultContextForNoMatch()
    {
        var element = new ElementInfo
        {
            Name = "CustomControl",
            ProcessName = "custom_app",
            ControlType = "Custom",
            ClassName = "CustomClass",
            IsEnabled = true,
            IsKeyboardFocusable = true
        };

        var context = _rules.GetContextForElement(element);

        context.Should().NotBeNullOrEmpty();
        context.Should().Contain("Custom");
        context.Should().Contain("Enabled");
    }

    [Fact]
    public void ShouldGetRulesForSpecificApp()
    {
        var excelRules = _rules.GetRulesForApp("excel");

        excelRules.Should().NotBeEmpty();
        excelRules.Should().Contain(r => r.Apps.Contains("excel"));
    }

    [Fact]
    public void ShouldGetWildcardRulesForAnyApp()
    {
        var unknownAppRules = _rules.GetRulesForApp("unknown_app");

        unknownAppRules.Should().NotBeEmpty();
        unknownAppRules.Should().Contain(r => r.Apps.Contains("*"));
    }

    [Fact]
    public void ShouldReloadRules()
    {
        var initialCount = _rules.RuleCount;

        _rules.ReloadRules().Should().BeTrue();
        _rules.RuleCount.Should().Be(initialCount);
    }

    [Fact]
    public void FailedReloadShouldPreserveCurrentRules()
    {
        var initialCount = _rules.RuleCount;
        File.WriteAllText(_tempRulesPath, "{invalid json");

        _rules.ReloadRules().Should().BeFalse();
        _rules.RuleCount.Should().Be(initialCount);
    }

    [Fact]
    public void ShouldHandleSpanishNames()
    {
        var element = new ElementInfo
        {
            Name = "Guardar",
            ProcessName = "excel",
            ControlType = "Button",
            ClassName = ""
        };

        var context = _rules.GetContextForElement(element);

        context.Should().NotBeNullOrEmpty();
        context.Should().Contain("Save current document");
    }
}

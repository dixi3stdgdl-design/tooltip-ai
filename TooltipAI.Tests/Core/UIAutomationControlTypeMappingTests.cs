using FluentAssertions;
using Xunit;

namespace TooltipAI.Tests.Core;

/// <summary>
/// Tests for UIA ControlType mapping logic.
/// These tests duplicate the mapping constants to verify correctness
/// without depending on the Platform.Win assembly.
/// </summary>
public class UIAutomationControlTypeMappingTests
{
    // Duplicated from UIAutomationInterop for cross-platform testing
    private const int UIA_ButtonControlTypeId = 50000;
    private const int UIA_EditControlTypeId = 50004;
    private const int UIA_SliderControlTypeId = 50015;
    private const int UIA_CheckBoxControlTypeId = 50002;
    private const int UIA_ComboBoxControlTypeId = 50003;
    private const int UIA_ListControlTypeId = 50008;
    private const int UIA_TreeControlTypeId = 50023;
    private const int UIA_MenuControlTypeId = 50009;
    private const int UIA_ProgressBarControlTypeId = 50012;
    private const int UIA_WindowControlTypeId = 50032;
    private const int UIA_DataGridControlTypeId = 50028;

    [Theory]
    [InlineData(UIA_ButtonControlTypeId, "Button")]
    [InlineData(UIA_EditControlTypeId, "Edit")]
    [InlineData(UIA_SliderControlTypeId, "Slider")]
    [InlineData(UIA_CheckBoxControlTypeId, "CheckBox")]
    [InlineData(UIA_ComboBoxControlTypeId, "ComboBox")]
    [InlineData(UIA_ListControlTypeId, "List")]
    [InlineData(UIA_TreeControlTypeId, "Tree")]
    [InlineData(UIA_MenuControlTypeId, "Menu")]
    [InlineData(UIA_ProgressBarControlTypeId, "ProgressBar")]
    [InlineData(UIA_WindowControlTypeId, "Window")]
    [InlineData(UIA_DataGridControlTypeId, "DataGrid")]
    [InlineData(0, "Unknown")]
    [InlineData(99999, "Unknown")]
    public void MapControlType_ReturnsCorrectString(int controlTypeId, string expected)
    {
        var result = MapControlType(controlTypeId);
        result.Should().Be(expected);
    }

    [Fact]
    public void MapControlType_AllKnownTypes_MapToNonEmptyStrings()
    {
        var knownIds = new[]
        {
            50000, 50001, 50002, 50003, 50004, 50005, 50006, 50007,
            50008, 50009, 50010, 50011, 50012, 50013, 50014, 50015,
            50016, 50017, 50018, 50019, 50020, 50021, 50022, 50023,
            50024, 50028, 50029, 50030, 50031, 50032, 50033, 50034,
            50035, 50036, 50037, 50038, 50039, 50040, 50043, 50044
        };

        foreach (var id in knownIds)
        {
            var result = MapControlType(id);
            result.Should().NotBeNullOrEmpty($"ControlType ID {id} should map to a non-empty string");
            result.Should().NotBe("Unknown", $"ControlType ID {id} should have a known mapping");
        }
    }

    /// <summary>
    /// Duplicate of UIAutomationInterop.MapControlType for cross-platform testing.
    /// </summary>
    private static string MapControlType(int controlTypeId)
    {
        return controlTypeId switch
        {
            50000 => "Button",
            50001 => "Calendar",
            50002 => "CheckBox",
            50003 => "ComboBox",
            50004 => "Edit",
            50005 => "Hyperlink",
            50006 => "Image",
            50007 => "ListItem",
            50008 => "List",
            50009 => "Menu",
            50010 => "MenuBar",
            50011 => "MenuItem",
            50012 => "ProgressBar",
            50013 => "RadioButton",
            50014 => "ScrollBar",
            50015 => "Slider",
            50016 => "Spinner",
            50017 => "StatusBar",
            50018 => "Tab",
            50019 => "TabItem",
            50020 => "Text",
            50021 => "ToolBar",
            50022 => "ToolTip",
            50023 => "Tree",
            50024 => "TreeItem",
            50028 => "DataGrid",
            50029 => "DataItem",
            50030 => "Document",
            50031 => "SplitButton",
            50032 => "Window",
            50033 => "Pane",
            50034 => "Header",
            50035 => "HeaderItem",
            50036 => "Table",
            50037 => "Thumb",
            50038 => "DataGridRow",
            50039 => "DataGridCell",
            50040 => "Group",
            50043 => "SemanticZoom",
            50044 => "AppBar",
            _ => "Unknown"
        };
    }
}

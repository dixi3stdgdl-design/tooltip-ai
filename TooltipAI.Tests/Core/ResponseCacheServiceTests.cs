using FluentAssertions;
using Xunit;
using TooltipAI.Core.Models;
using TooltipAI.Core.Services;

namespace TooltipAI.Tests.Core;

public class ResponseCacheServiceTests : IDisposable
{
    private readonly ResponseCacheService _cache;
    private readonly string _tempDbPath;

    public ResponseCacheServiceTests()
    {
        _tempDbPath = Path.Combine(Path.GetTempPath(), $"cache_test_{Guid.NewGuid()}.db");
        _cache = new ResponseCacheService(_tempDbPath);
    }

    public void Dispose()
    {
        _cache.Dispose();
        if (File.Exists(_tempDbPath))
            File.Delete(_tempDbPath);
    }

    [Fact]
    public void ShouldStoreAndRetrieveTooltipData()
    {
        var key = "test:key:1";
        var data = CreateTestTooltipData("Test Element", "Button");

        _cache.Set(key, data);

        var retrieved = _cache.Get(key);

        retrieved.Should().NotBeNull();
        retrieved!.Element.Name.Should().Be("Test Element");
        retrieved.Element.ControlType.Should().Be("Button");
    }

    [Fact]
    public void ShouldReturnNullForMissingKey()
    {
        var result = _cache.Get("nonexistent:key");

        result.Should().BeNull();
    }

    [Fact]
    public void ShouldRemoveEntry()
    {
        var key = "test:remove:1";
        var data = CreateTestTooltipData("Remove Me", "Button");

        _cache.Set(key, data);
        _cache.Remove(key);

        var result = _cache.Get(key);
        result.Should().BeNull();
    }

    [Fact]
    public void ShouldClearAllEntries()
    {
        _cache.Set("key1", CreateTestTooltipData("Element 1", "Button"));
        _cache.Set("key2", CreateTestTooltipData("Element 2", "TextBox"));

        _cache.Clear();

        _cache.Get("key1").Should().BeNull();
        _cache.Get("key2").Should().BeNull();
    }

    [Fact]
    public void ShouldReturnStats()
    {
        _cache.Set("key1", CreateTestTooltipData("Element 1", "Button"));
        _cache.Set("key2", CreateTestTooltipData("Element 2", "TextBox"));

        var stats = _cache.GetStats();

        stats.TotalEntries.Should().Be(2);
        stats.ActiveEntries.Should().Be(2);
    }

    [Fact]
    public void ShouldGenerateConsistentKey()
    {
        var element = new ElementInfo
        {
            ProcessName = "excel",
            Name = "Save",
            ControlType = "Button",
            ClassName = "ToolbarButton"
        };

        var key1 = _cache.GenerateKey(element);
        var key2 = _cache.GenerateKey(element);

        key1.Should().Be(key2);
    }

    [Fact]
    public void ShouldGenerateDifferentKeysForDifferentElements()
    {
        var element1 = new ElementInfo
        {
            ProcessName = "excel",
            Name = "Save",
            ControlType = "Button",
            ClassName = ""
        };

        var element2 = new ElementInfo
        {
            ProcessName = "excel",
            Name = "Open",
            ControlType = "Button",
            ClassName = ""
        };

        var key1 = _cache.GenerateKey(element1);
        var key2 = _cache.GenerateKey(element2);

        key1.Should().NotBe(key2);
    }

    [Fact]
    public void ShouldHandleExpiredEntries()
    {
        var key = "test:expire:1";
        var data = CreateTestTooltipData("Expire Me", "Button");

        _cache.Set(key, data, TimeSpan.FromMilliseconds(1));

        Thread.Sleep(50);

        var result = _cache.Get(key);
        result.Should().BeNull();
    }

    [Fact]
    public void ShouldOverwriteExistingEntry()
    {
        var key = "test:overwrite:1";
        var data1 = CreateTestTooltipData("Original", "Button");
        var data2 = CreateTestTooltipData("Updated", "TextBox");

        _cache.Set(key, data1);
        _cache.Set(key, data2);

        var result = _cache.Get(key);

        result.Should().NotBeNull();
        result!.Element.Name.Should().Be("Updated");
        result.Element.ControlType.Should().Be("TextBox");
    }

    [Fact]
    public void ShouldTrackEntryCount()
    {
        var initialCount = _cache.EntryCount;

        _cache.Set("key1", CreateTestTooltipData("Element 1", "Button"));
        _cache.Set("key2", CreateTestTooltipData("Element 2", "TextBox"));

        _cache.EntryCount.Should().Be(initialCount + 2);
    }

    [Fact]
    public void ShouldHandleSpecialCharactersInKey()
    {
        var key = "test:special:chars:abc/123:xyz";
        var data = CreateTestTooltipData("Special", "Button");

        _cache.Set(key, data);

        var result = _cache.Get(key);
        result.Should().NotBeNull();
    }

    private TooltipData CreateTestTooltipData(string name, string controlType)
    {
        return new TooltipData
        {
            Element = new ElementInfo
            {
                Name = name,
                ControlType = controlType,
                ProcessName = "test_app",
                ClassName = "TestClass",
                IsEnabled = true
            },
            EnrichedContext = $"Context for {name}",
            FunctionHint = "Ctrl+T",
            SoftwareCategory = "Test"
        };
    }
}

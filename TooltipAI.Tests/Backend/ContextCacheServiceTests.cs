using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TooltipAI.Backend.Models;
using TooltipAI.Backend.Services;
using Xunit;

namespace TooltipAI.Tests.Backend;

public class ContextCacheServiceTests
{
    private readonly ContextCacheService _service;

    public ContextCacheServiceTests()
    {
        var logger = Mock.Of<ILogger<ContextCacheService>>();
        _service = new ContextCacheService(logger);
    }

    [Fact]
    public void Get_WhenEmpty_ReturnsNull()
    {
        var result = _service.Get("nonexistent-key");
        result.Should().BeNull();
    }

    [Fact]
    public void Set_ThenGet_ReturnsEntry()
    {
        _service.Set(new ContextCacheRequest
        {
            Key = "test-key",
            ElementName = "Button",
            ElementType = "Button",
            ApplicationName = "Notepad",
            Context = "A button that saves the document",
            Tags = new[] { "save", "document" },
            Confidence = 85,
            TtlSeconds = 60
        });

        var result = _service.Get("test-key");

        result.Should().NotBeNull();
        result!.Key.Should().Be("test-key");
        result.ElementName.Should().Be("Button");
        result.ApplicationName.Should().Be("Notepad");
        result.Context.Should().Be("A button that saves the document");
        result.Tags.Should().BeEquivalentTo(new[] { "save", "document" });
        result.Confidence.Should().Be(85);
    }

    [Fact]
    public void Get_WhenExpired_ReturnsNull()
    {
        _service.Set(new ContextCacheRequest
        {
            Key = "expiring-key",
            ElementName = "Button",
            ElementType = "Button",
            ApplicationName = "Notepad",
            Context = "Context",
            Tags = Array.Empty<string>(),
            Confidence = 50,
            TtlSeconds = 1
        });

        Thread.Sleep(1100);

        var result = _service.Get("expiring-key");
        result.Should().BeNull();
    }

    [Fact]
    public void Set_UpdateExistingKey()
    {
        _service.Set(new ContextCacheRequest
        {
            Key = "update-key",
            ElementName = "Button",
            ElementType = "Button",
            ApplicationName = "Notepad",
            Context = "Old context",
            Tags = Array.Empty<string>(),
            Confidence = 50,
            TtlSeconds = 60
        });

        _service.Set(new ContextCacheRequest
        {
            Key = "update-key",
            ElementName = "Button",
            ElementType = "Button",
            ApplicationName = "Notepad",
            Context = "New context",
            Tags = Array.Empty<string>(),
            Confidence = 90,
            TtlSeconds = 60
        });

        var result = _service.Get("update-key");
        result!.Context.Should().Be("New context");
        result.Confidence.Should().Be(90);
    }

    [Fact]
    public void GetStats_EmptyCache_ReturnsZeroes()
    {
        var stats = _service.GetStats();

        stats.TotalEntries.Should().Be(0);
        stats.ActiveEntries.Should().Be(0);
        stats.TotalHits.Should().Be(0);
        stats.HitRate.Should().Be(0);
    }

    [Fact]
    public void GetStats_WithEntries_ReturnsCorrectCounts()
    {
        _service.Set(new ContextCacheRequest
        {
            Key = "key-1",
            ElementName = "Button",
            ElementType = "Button",
            ApplicationName = "App",
            Context = "Context 1",
            Tags = Array.Empty<string>(),
            Confidence = 50,
            TtlSeconds = 60
        });

        _service.Set(new ContextCacheRequest
        {
            Key = "key-2",
            ElementName = "TextBox",
            ElementType = "Edit",
            ApplicationName = "App",
            Context = "Context 2",
            Tags = Array.Empty<string>(),
            Confidence = 60,
            TtlSeconds = 60
        });

        _service.Get("key-1");
        _service.Get("key-1");

        var stats = _service.GetStats();

        stats.TotalEntries.Should().Be(2);
        stats.ActiveEntries.Should().Be(2);
        stats.TotalHits.Should().Be(2);
    }

    [Fact]
    public void Get_HitRate_ComputedCorrectly()
    {
        _service.Set(new ContextCacheRequest
        {
            Key = "hit-key",
            ElementName = "Button",
            ElementType = "Button",
            ApplicationName = "App",
            Context = "Context",
            Tags = Array.Empty<string>(),
            Confidence = 50,
            TtlSeconds = 60
        });

        _service.Get("hit-key");
        _service.Get("hit-key");
        _service.Get("miss-key");

        var stats = _service.GetStats();

        stats.TotalHits.Should().Be(2);
        stats.TotalMisses.Should().Be(1);
        stats.HitRate.Should().BeApproximately(66.67, 0.1);
    }

    [Fact]
    public void Set_MultipleEntries_AllRetrievable()
    {
        for (int i = 0; i < 10; i++)
        {
            _service.Set(new ContextCacheRequest
            {
                Key = $"key-{i}",
                ElementName = $"Element{i}",
                ElementType = "Button",
                ApplicationName = "App",
                Context = $"Context {i}",
                Tags = new[] { $"tag{i}" },
                Confidence = i * 10,
                TtlSeconds = 60
            });
        }

        for (int i = 0; i < 10; i++)
        {
            var result = _service.Get($"key-{i}");
            result.Should().NotBeNull();
            result!.ElementName.Should().Be($"Element{i}");
        }
    }
}

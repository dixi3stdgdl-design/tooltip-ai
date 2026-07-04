using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TooltipAI.Backend.Controllers;
using TooltipAI.Backend.Models;
using TooltipAI.Backend.Services;
using Xunit;

namespace TooltipAI.Tests.Backend;

public class ContextControllerTests
{
    private readonly ContextController _controller;
    private readonly ContextCacheService _service;

    public ContextControllerTests()
    {
        var logger = Mock.Of<ILogger<ContextCacheService>>();
        _service = new ContextCacheService(logger);
        _controller = new ContextController(_service);
    }

    [Fact]
    public void Get_NonExistingKey_ReturnsNotFound()
    {
        var result = _controller.Get("nonexistent");

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void Set_ValidRequest_ReturnsOk()
    {
        var result = _controller.Set(new ContextCacheRequest
        {
            Key = "test-key",
            ElementName = "Button",
            ElementType = "Button",
            ApplicationName = "Notepad",
            Context = "Save button",
            Tags = new[] { "save" },
            Confidence = 80,
            TtlSeconds = 60
        });

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public void Set_ThenGet_ReturnsEntry()
    {
        _controller.Set(new ContextCacheRequest
        {
            Key = "roundtrip-key",
            ElementName = "TextBox",
            ElementType = "Edit",
            ApplicationName = "Word",
            Context = "Text input field",
            Tags = new[] { "input", "text" },
            Confidence = 90,
            TtlSeconds = 60
        });

        var getResult = _controller.Get("roundtrip-key");

        var okResult = getResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var entry = okResult.Value.Should().BeOfType<ContextEntry>().Subject;
        entry.Key.Should().Be("roundtrip-key");
        entry.ElementName.Should().Be("TextBox");
        entry.Context.Should().Be("Text input field");
    }

    [Fact]
    public void Stats_EmptyCache_ReturnsZeroes()
    {
        var result = _controller.Stats();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var stats = okResult.Value.Should().BeOfType<ContextCacheStats>().Subject;
        stats.TotalEntries.Should().Be(0);
    }

    [Fact]
    public void Stats_WithEntries_ReturnsCorrectCounts()
    {
        _controller.Set(new ContextCacheRequest
        {
            Key = "stats-key-1",
            ElementName = "Button",
            ElementType = "Button",
            ApplicationName = "App",
            Context = "Context",
            Tags = Array.Empty<string>(),
            Confidence = 50,
            TtlSeconds = 60
        });

        _controller.Set(new ContextCacheRequest
        {
            Key = "stats-key-2",
            ElementName = "Menu",
            ElementType = "Menu",
            ApplicationName = "App",
            Context = "Menu",
            Tags = Array.Empty<string>(),
            Confidence = 60,
            TtlSeconds = 60
        });

        var result = _controller.Stats();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var stats = okResult.Value.Should().BeOfType<ContextCacheStats>().Subject;
        stats.TotalEntries.Should().Be(2);
    }

    [Fact]
    public void Get_AfterExpiry_ReturnsNotFound()
    {
        _controller.Set(new ContextCacheRequest
        {
            Key = "expiry-key",
            ElementName = "Button",
            ElementType = "Button",
            ApplicationName = "App",
            Context = "Context",
            Tags = Array.Empty<string>(),
            Confidence = 50,
            TtlSeconds = 1
        });

        Thread.Sleep(1100);

        var result = _controller.Get("expiry-key");
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }
}

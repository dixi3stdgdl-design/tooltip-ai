using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TooltipAI.Backend.Controllers;
using TooltipAI.Backend.Models;
using TooltipAI.Backend.Services;
using Xunit;

namespace TooltipAI.Tests.Backend;

public class PluginsControllerTests
{
    private readonly PluginsController _controller;

    public PluginsControllerTests()
    {
        var logger = Mock.Of<ILogger<PluginRegistryService>>();
        var service = new PluginRegistryService(logger);
        _controller = new PluginsController(service);
    }

    [Fact]
    public void GetAll_ReturnsOfficialPlugins()
    {
        var result = _controller.GetAll();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var plugins = okResult.Value.Should().BeAssignableTo<IReadOnlyList<PluginInfo>>().Subject;
        plugins.Should().NotBeEmpty();
        plugins.Should().Contain(p => p.Id == "tooltipai-context-basic");
    }

    [Fact]
    public void GetById_ExistingPlugin_ReturnsOk()
    {
        var result = _controller.GetById("tooltipai-context-basic");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var plugin = okResult.Value.Should().BeOfType<PluginInfo>().Subject;
        plugin.Name.Should().Be("Basic Context Pack");
    }

    [Fact]
    public void GetById_NonExistingPlugin_ReturnsNotFound()
    {
        var result = _controller.GetById("nonexistent");

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void Register_NewPlugin_ReturnsCreated()
    {
        var result = _controller.Register(new PluginRegisterRequest
        {
            Id = "test-register-new",
            Name = "Test Plugin",
            Description = "A test plugin",
            Version = "1.0.0",
            Author = "Test Author",
            DownloadUrl = "https://example.com/test.dll",
            Sha256Hash = "abc123def456",
            MinAppVersion = 100
        });

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public void Register_DuplicatePlugin_ReturnsConflict()
    {
        _controller.Register(new PluginRegisterRequest
        {
            Id = "duplicate-test-2",
            Name = "Plugin",
            Description = "Desc",
            Version = "1.0.0",
            Author = "Author",
            DownloadUrl = "https://example.com/dll",
            Sha256Hash = "hash",
            MinAppVersion = 100
        });

        var result = _controller.Register(new PluginRegisterRequest
        {
            Id = "duplicate-test-2",
            Name = "Plugin 2",
            Description = "Desc 2",
            Version = "2.0.0",
            Author = "Author 2",
            DownloadUrl = "https://example.com/dll2",
            Sha256Hash = "hash2",
            MinAppVersion = 100
        });

        result.Result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public void Stats_ReturnsCorrectCounts()
    {
        var result = _controller.Stats();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var stats = okResult.Value.Should().BeOfType<PluginRegistryStats>().Subject;
        stats.TotalPlugins.Should().BeGreaterThanOrEqualTo(2);
        stats.OfficialPlugins.Should().BeGreaterThanOrEqualTo(2);
    }
}

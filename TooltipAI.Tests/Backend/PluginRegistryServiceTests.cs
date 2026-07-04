using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TooltipAI.Backend.Models;
using TooltipAI.Backend.Services;
using Xunit;

namespace TooltipAI.Tests.Backend;

public class PluginRegistryServiceTests
{
    private readonly PluginRegistryService _service;

    public PluginRegistryServiceTests()
    {
        var logger = Mock.Of<ILogger<PluginRegistryService>>();
        _service = new PluginRegistryService(logger);
    }

    [Fact]
    public void GetAll_ContainsOfficialPlugins()
    {
        var plugins = _service.GetAll();

        plugins.Should().NotBeEmpty();
        plugins.Should().Contain(p => p.Id == "tooltipai-context-basic");
        plugins.Should().Contain(p => p.Id == "tooltipai-shortcuts");
    }

    [Fact]
    public void GetById_ExistingPlugin_ReturnsPlugin()
    {
        var plugin = _service.GetById("tooltipai-context-basic");

        plugin.Should().NotBeNull();
        plugin!.Name.Should().Be("Basic Context Pack");
        plugin.IsOfficial.Should().BeTrue();
    }

    [Fact]
    public void GetById_NonExistingPlugin_ReturnsNull()
    {
        var plugin = _service.GetById("nonexistent-plugin");

        plugin.Should().BeNull();
    }

    [Fact]
    public void Register_NewPlugin_Succeeds()
    {
        var result = _service.Register(new PluginRegisterRequest
        {
            Id = "community-plugin-1",
            Name = "Community Plugin",
            Description = "A community plugin",
            Version = "1.0.0",
            Author = "Community Dev",
            DownloadUrl = "https://example.com/plugin.dll",
            Sha256Hash = "abc123",
            MinAppVersion = 100
        });

        result.Should().BeTrue();

        var plugin = _service.GetById("community-plugin-1");
        plugin.Should().NotBeNull();
        plugin!.IsOfficial.Should().BeFalse();
        plugin.Author.Should().Be("Community Dev");
    }

    [Fact]
    public void Register_DuplicatePlugin_Fails()
    {
        _service.Register(new PluginRegisterRequest
        {
            Id = "duplicate-plugin",
            Name = "Plugin",
            Description = "Desc",
            Version = "1.0.0",
            Author = "Author",
            DownloadUrl = "https://example.com/dll",
            Sha256Hash = "hash",
            MinAppVersion = 100
        });

        var result = _service.Register(new PluginRegisterRequest
        {
            Id = "duplicate-plugin",
            Name = "Plugin 2",
            Description = "Desc 2",
            Version = "2.0.0",
            Author = "Author 2",
            DownloadUrl = "https://example.com/dll2",
            Sha256Hash = "hash2",
            MinAppVersion = 100
        });

        result.Should().BeFalse();
    }

    [Fact]
    public void GetStats_ReturnsCorrectCounts()
    {
        _service.Register(new PluginRegisterRequest
        {
            Id = "community-1",
            Name = "Community 1",
            Description = "Desc",
            Version = "1.0.0",
            Author = "Author",
            DownloadUrl = "https://example.com/dll",
            Sha256Hash = "hash",
            MinAppVersion = 100
        });

        var stats = _service.GetStats();

        stats.TotalPlugins.Should().BeGreaterThanOrEqualTo(2);
        stats.OfficialPlugins.Should().BeGreaterThanOrEqualTo(2);
        stats.CommunityPlugins.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void GetAll_OrderedByDownloads()
    {
        var plugins = _service.GetAll();

        for (int i = 1; i < plugins.Count; i++)
        {
            plugins[i - 1].Downloads.Should().BeGreaterThanOrEqualTo(plugins[i].Downloads);
        }
    }

    [Fact]
    public void Register_SetsPublishedAt()
    {
        var before = DateTime.UtcNow;

        _service.Register(new PluginRegisterRequest
        {
            Id = "timestamp-plugin",
            Name = "Timestamp Plugin",
            Description = "Desc",
            Version = "1.0.0",
            Author = "Author",
            DownloadUrl = "https://example.com/dll",
            Sha256Hash = "hash",
            MinAppVersion = 100
        });

        var after = DateTime.UtcNow;
        var plugin = _service.GetById("timestamp-plugin");

        plugin.Should().NotBeNull();
        plugin!.PublishedAt.Should().BeAfter(before);
        plugin.PublishedAt.Should().BeBefore(after);
    }
}

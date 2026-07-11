using FluentAssertions;
using TooltipAI.Core.Services;
using Xunit;

namespace TooltipAI.Tests.Core;

public class AppBlacklistServiceTests : IDisposable
{
    private readonly string _tempPath;
    private readonly AppBlacklistService _service;

    public AppBlacklistServiceTests()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), $"blacklist_test_{Guid.NewGuid()}.json");
        _service = new AppBlacklistService(_tempPath);
    }

    public void Dispose()
    {
        _service.Dispose();
        if (File.Exists(_tempPath))
            File.Delete(_tempPath);
    }

    [Fact]
    public void Blacklist_StartsEmpty()
    {
        _service.Blacklist.Should().BeEmpty();
    }

    [Fact]
    public void Blacklist_ReturnsDefensiveCopy()
    {
        _service.Add("app.exe");

        var snapshot = _service.Blacklist;
        snapshot.Add("mutation.exe");

        _service.Blacklist.Should().ContainSingle().Which.Should().Be("app.exe");
    }

    [Fact]
    public void Add_IsIdempotent()
    {
        _service.Add("dup.exe");
        _service.Add("dup.exe");

        _service.Blacklist.Should().ContainSingle();
    }

    [Fact]
    public void IsBlacklisted_MatchesCaseInsensitiveSubstring()
    {
        _service.Add("KeePass");

        _service.IsBlacklisted("C:\\Program Files\\keepass.exe").Should().BeTrue();
        _service.IsBlacklisted("chrome.exe").Should().BeFalse();
    }

    [Fact]
    public void Remove_RemovesEntry()
    {
        _service.Add("a.exe");
        _service.Add("b.exe");

        _service.Remove("a.exe");

        _service.Blacklist.Should().ContainSingle().Which.Should().Be("b.exe");
    }

    [Fact]
    public void SetBlacklist_ReplacesEntireList()
    {
        _service.Add("old.exe");

        _service.SetBlacklist(new List<string> { "x.exe", "y.exe" });

        _service.Blacklist.Should().BeEquivalentTo(new[] { "x.exe", "y.exe" });
    }

    [Fact]
    public void Clear_EmptiesList()
    {
        _service.Add("a.exe");
        _service.Add("b.exe");

        _service.Clear();

        _service.Blacklist.Should().BeEmpty();
    }

    [Fact]
    public void Add_RaisesBlacklistChangedEvent()
    {
        List<string>? received = null;
        _service.BlacklistChanged += list => received = list;

        _service.Add("watched.exe");

        received.Should().NotBeNull();
        received!.Should().Contain("watched.exe");
    }

    [Fact]
    public void Blacklist_PersistsAcrossInstances()
    {
        _service.Add("persist.exe");

        using var reloaded = new AppBlacklistService(_tempPath);

        reloaded.Blacklist.Should().Contain("persist.exe");
    }
}

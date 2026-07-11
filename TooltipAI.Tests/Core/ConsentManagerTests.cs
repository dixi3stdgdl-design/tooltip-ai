using FluentAssertions;
using TooltipAI.Core.Services;
using Xunit;

namespace TooltipAI.Tests.Core;

public class ConsentManagerTests : IDisposable
{
    private readonly string _tempPath;
    private readonly ConsentManager _manager;

    public ConsentManagerTests()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), $"consent_test_{Guid.NewGuid()}.json");
        _manager = new ConsentManager(_tempPath);
    }

    public void Dispose()
    {
        _manager.Dispose();
        if (File.Exists(_tempPath))
            File.Delete(_tempPath);
    }

    [Fact]
    public void Defaults_AreLocalOnlyWithEnrichmentAndTelemetryDisabled()
    {
        _manager.IsLocalOnlyMode.Should().BeTrue();
        _manager.IsAIEnrichmentEnabled.Should().BeFalse();
        _manager.IsTelemetryEnabled.Should().BeFalse();
    }

    [Fact]
    public void IsAIEnrichmentEnabled_RequiresLocalOnlyModeDisabled()
    {
        _manager.EnableAIEnrichment(true);

        // Local-only mode still on by default, so enrichment stays effectively off.
        _manager.IsAIEnrichmentEnabled.Should().BeFalse();

        _manager.SetLocalOnlyMode(false);

        _manager.IsAIEnrichmentEnabled.Should().BeTrue();
    }

    [Fact]
    public void IsTelemetryEnabled_RequiresLocalOnlyModeDisabled()
    {
        _manager.EnableTelemetry(true);
        _manager.SetLocalOnlyMode(false);

        _manager.IsTelemetryEnabled.Should().BeTrue();
    }

    [Fact]
    public void EnableAIEnrichment_RaisesConsentChangedEvent()
    {
        ConsentState? received = null;
        _manager.ConsentChanged += s => received = s;

        _manager.EnableAIEnrichment(true);

        received.Should().NotBeNull();
        received!.AIEnrichmentEnabled.Should().BeTrue();
    }

    [Fact]
    public void AddToBlacklist_IsIdempotent()
    {
        _manager.AddToBlacklist("secret.exe");
        _manager.AddToBlacklist("secret.exe");

        _manager.State.AppBlacklist.Should().ContainSingle().Which.Should().Be("secret.exe");
    }

    [Fact]
    public void IsAppBlacklisted_MatchesCaseInsensitiveSubstring()
    {
        _manager.AddToBlacklist("KeePass");

        _manager.IsAppBlacklisted("C:\\Apps\\keepass.exe").Should().BeTrue();
        _manager.IsAppBlacklisted("notepad.exe").Should().BeFalse();
    }

    [Fact]
    public void RemoveFromBlacklist_RemovesEntry()
    {
        _manager.AddToBlacklist("app1");
        _manager.AddToBlacklist("app2");

        _manager.RemoveFromBlacklist("app1");

        _manager.State.AppBlacklist.Should().ContainSingle().Which.Should().Be("app2");
    }

    [Fact]
    public void SetAppBlacklist_ReplacesEntireList()
    {
        _manager.AddToBlacklist("old");

        _manager.SetAppBlacklist(new List<string> { "new1", "new2" });

        _manager.State.AppBlacklist.Should().BeEquivalentTo(new[] { "new1", "new2" });
    }

    [Fact]
    public void ResetToDefaults_RestoresLocalOnlyDefaults()
    {
        _manager.SetLocalOnlyMode(false);
        _manager.EnableTelemetry(true);
        _manager.AddToBlacklist("app");

        _manager.ResetToDefaults();

        _manager.State.LocalOnlyMode.Should().BeTrue();
        _manager.State.TelemetryEnabled.Should().BeFalse();
        _manager.State.AppBlacklist.Should().BeEmpty();
    }

    [Fact]
    public void State_PersistsAcrossInstances()
    {
        _manager.SetLocalOnlyMode(false);
        _manager.EnableAIEnrichment(true);

        using var reloaded = new ConsentManager(_tempPath);

        reloaded.State.LocalOnlyMode.Should().BeFalse();
        reloaded.State.AIEnrichmentEnabled.Should().BeTrue();
    }
}

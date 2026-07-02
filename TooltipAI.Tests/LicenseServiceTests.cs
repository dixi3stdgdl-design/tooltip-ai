using TooltipAI.Core.Services;
using Xunit;

namespace TooltipAI.Tests;

public class LicenseServiceTests : IDisposable
{
    private readonly string _testDir;
    private readonly LicenseService _service;

    public LicenseServiceTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"tooltipai_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDir);
        _service = new LicenseService(Path.Combine(_testDir, "license.dat"));
    }

    public void Dispose()
    {
        _service.Dispose();
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, true);
    }

    [Fact]
    public void NewInstall_StartsTrial()
    {
        Assert.True(_service.IsTrialActive);
        Assert.False(_service.IsLicensed);
        Assert.False(_service.IsExpired);
    }

    [Fact]
    public void TrialRemainingDays_Returns14OnFirstRun()
    {
        var remaining = _service.GetRemainingTrialDays();
        Assert.Equal(14, remaining);
    }

    [Fact]
    public void CanUseService_DuringTrial()
    {
        Assert.True(_service.CanUseService());
    }

    [Fact]
    public void GenerateLicenseKey_ProducesValidKey()
    {
        var expiry = DateTime.UtcNow.AddDays(365);
        var key = LicenseService.GenerateLicenseKey("TEST-001", expiry);

        Assert.False(string.IsNullOrEmpty(key));
        Assert.True(key.Length > 10);
    }

    [Fact]
    public void ActivateLicense_WithValidKey_Succeeds()
    {
        var expiry = DateTime.UtcNow.AddDays(365);
        var key = LicenseService.GenerateLicenseKey("TEST-001", expiry);

        var result = _service.ActivateLicense(key);

        Assert.True(result);
        Assert.True(_service.IsLicensed);
        Assert.False(_service.IsTrialActive);
    }

    [Fact]
    public void ActivateLicense_WithInvalidKey_Fails()
    {
        var result = _service.ActivateLicense("invalid-key");

        Assert.False(result);
        Assert.False(_service.IsLicensed);
    }

    [Fact]
    public void ActivateLicense_WithEmptyKey_Fails()
    {
        Assert.False(_service.ActivateLicense(""));
        Assert.False(_service.ActivateLicense("   "));
    }

    [Fact]
    public void GetLicenseStatusMessage_DuringTrial_ContainsDaysRemaining()
    {
        var msg = _service.GetLicenseStatusMessage();
        Assert.Contains("Trial", msg);
        Assert.Contains("14", msg);
    }

    [Fact]
    public void GetLicenseStatusMessage_WhenLicensed_ReturnsLicensed()
    {
        var expiry = DateTime.UtcNow.AddDays(365);
        var key = LicenseService.GenerateLicenseKey("TEST-001", expiry);
        _service.ActivateLicense(key);

        var msg = _service.GetLicenseStatusMessage();
        Assert.Equal("Licensed", msg);
    }

    [Fact]
    public void LicenseStatusChanged_FiresOnActivation()
    {
        var fired = false;
        _service.LicenseStatusChanged += _ => fired = true;

        var expiry = DateTime.UtcNow.AddDays(365);
        var key = LicenseService.GenerateLicenseKey("TEST-001", expiry);
        _service.ActivateLicense(key);

        Assert.True(fired);
    }

    [Fact]
    public void GetTrialExpirationDate_Returns14DaysFromFirstRun()
    {
        var expiry = _service.GetTrialExpirationDate();
        Assert.NotNull(expiry);
        var daysUntil = (expiry.Value - DateTime.UtcNow).TotalDays;
        Assert.InRange(daysUntil, 13, 15);
    }
}

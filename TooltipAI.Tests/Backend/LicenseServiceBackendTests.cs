using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TooltipAI.Backend.Models;
using TooltipAI.Backend.Services;
using Xunit;

namespace TooltipAI.Tests.Backend;

public class LicenseServiceBackendTests
{
    private readonly LicenseService _service;
    private readonly string _testKey = "test-secret-key-for-unit-tests-2024";

    public LicenseServiceBackendTests()
    {
        var logger = Mock.Of<ILogger<LicenseService>>();
        _service = new LicenseService(_testKey, logger);
    }

    [Fact]
    public void GenerateLicenseKey_ReturnsValidBase64()
    {
        var key = _service.GenerateLicenseKey("LIC-001", "pro", DateTime.UtcNow.AddDays(30));

        key.Should().NotBeNullOrEmpty();
        var decoded = Convert.FromBase64String(key);
        decoded.Should().NotBeEmpty();
    }

    [Fact]
    public void GenerateLicenseKey_DecodesToCorrectParts()
    {
        var expiry = DateTime.UtcNow.AddDays(365);
        var key = _service.GenerateLicenseKey("LIC-001", "enterprise", expiry);

        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(key));
        var parts = decoded.Split('|');

        parts.Length.Should().Be(4);
        parts[0].Should().Be("LIC-001");
        parts[1].Should().Be("enterprise");
    }

    [Fact]
    public void Validate_WithValidKey_ReturnsValid()
    {
        var expiry = DateTime.UtcNow.AddDays(30);
        var key = _service.GenerateLicenseKey("LIC-002", "pro", expiry);

        var result = _service.Validate(new LicenseValidateRequest
        {
            LicenseKey = key,
            MachineId = "machine-001",
            AppVersion = "1.0.0"
        });

        result.Valid.Should().BeTrue();
        result.Tier.Should().Be("pro");
        result.ExpiresAt.Should().BeCloseTo(expiry, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Validate_WithExpiredKey_ReturnsInvalid()
    {
        var expiry = DateTime.UtcNow.AddDays(-5);
        var key = _service.GenerateLicenseKey("LIC-003", "free", expiry);

        var result = _service.Validate(new LicenseValidateRequest
        {
            LicenseKey = key,
            MachineId = "machine-001",
            AppVersion = "1.0.0"
        });

        result.Valid.Should().BeFalse();
        result.Error.Should().Contain("expired");
    }

    [Fact]
    public void Validate_WithEmptyKey_ReturnsInvalid()
    {
        var result = _service.Validate(new LicenseValidateRequest
        {
            LicenseKey = "",
            MachineId = "machine-001",
            AppVersion = "1.0.0"
        });

        result.Valid.Should().BeFalse();
        result.Error.Should().Contain("required");
    }

    [Fact]
    public void Validate_WithInvalidKey_ReturnsInvalid()
    {
        var result = _service.Validate(new LicenseValidateRequest
        {
            LicenseKey = "not-a-valid-key",
            MachineId = "machine-001",
            AppVersion = "1.0.0"
        });

        result.Valid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithTamperedKey_ReturnsInvalid()
    {
        var key = _service.GenerateLicenseKey("LIC-004", "pro", DateTime.UtcNow.AddDays(30));
        var bytes = Convert.FromBase64String(key);
        var decoded = Encoding.UTF8.GetString(bytes);
        var parts = decoded.Split('|');
        parts[0] = "LIC-TAMPERED";
        var tampered = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join('|', parts)));

        var result = _service.Validate(new LicenseValidateRequest
        {
            LicenseKey = tampered,
            MachineId = "machine-001",
            AppVersion = "1.0.0"
        });

        result.Valid.Should().BeFalse();
    }

    [Fact]
    public void Validate_DifferentKeys_DifferentLicenseIds()
    {
        var key1 = _service.GenerateLicenseKey("LIC-A", "free", DateTime.UtcNow.AddDays(30));
        var key2 = _service.GenerateLicenseKey("LIC-B", "pro", DateTime.UtcNow.AddDays(30));

        var result1 = _service.Validate(new LicenseValidateRequest
        {
            LicenseKey = key1,
            MachineId = "machine-001",
            AppVersion = "1.0.0"
        });

        var result2 = _service.Validate(new LicenseValidateRequest
        {
            LicenseKey = key2,
            MachineId = "machine-001",
            AppVersion = "1.0.0"
        });

        result1.Valid.Should().BeTrue();
        result2.Valid.Should().BeTrue();
        result1.Tier.Should().Be("free");
        result2.Tier.Should().Be("pro");
    }

    [Fact]
    public void Validate_ReturnsDaysRemaining()
    {
        var expiry = DateTime.UtcNow.AddDays(15);
        var key = _service.GenerateLicenseKey("LIC-005", "pro", expiry);

        var result = _service.Validate(new LicenseValidateRequest
        {
            LicenseKey = key,
            MachineId = "machine-001",
            AppVersion = "1.0.0"
        });

        result.DaysRemaining.Should().BeGreaterThanOrEqualTo(14);
        result.DaysRemaining.Should().BeLessThanOrEqualTo(16);
    }
}

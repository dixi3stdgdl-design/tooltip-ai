using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TooltipAI.Backend.Controllers;
using TooltipAI.Backend.Models;
using TooltipAI.Backend.Services;
using Xunit;

namespace TooltipAI.Tests.Backend;

public class LicenseControllerTests
{
    private readonly LicenseController _controller;
    private readonly LicenseService _service;

    public LicenseControllerTests()
    {
        var logger = Mock.Of<ILogger<LicenseService>>();
        _service = new LicenseService("test-key-for-controller-tests-2024", logger);
        _controller = new LicenseController(_service);
    }

    [Fact]
    public void Validate_ValidKey_ReturnsOk()
    {
        var expiry = DateTime.UtcNow.AddDays(30);
        var key = _service.GenerateLicenseKey("LIC-CTL-001", "pro", expiry);

        var result = _controller.Validate(new LicenseValidateRequest
        {
            LicenseKey = key,
            MachineId = "machine-001",
            AppVersion = "1.0.0"
        });

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<LicenseValidateResponse>().Subject;
        response.Valid.Should().BeTrue();
        response.Tier.Should().Be("pro");
    }

    [Fact]
    public void Validate_InvalidKey_ReturnsOkWithInvalid()
    {
        var result = _controller.Validate(new LicenseValidateRequest
        {
            LicenseKey = "invalid",
            MachineId = "machine-001",
            AppVersion = "1.0.0"
        });

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<LicenseValidateResponse>().Subject;
        response.Valid.Should().BeFalse();
    }

    [Fact]
    public void Generate_ValidRequest_ReturnsOk()
    {
        var result = _controller.Generate(new GenerateLicenseRequest
        {
            LicenseId = "LIC-GEN-001",
            Tier = "enterprise",
            ExpiryDate = DateTime.UtcNow.AddDays(365)
        });

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void Validate_ReturnsDaysRemaining()
    {
        var expiry = DateTime.UtcNow.AddDays(7);
        var key = _service.GenerateLicenseKey("LIC-DAYS", "free", expiry);

        var result = _controller.Validate(new LicenseValidateRequest
        {
            LicenseKey = key,
            MachineId = "machine-001",
            AppVersion = "1.0.0"
        });

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<LicenseValidateResponse>().Subject;
        response.DaysRemaining.Should().BeGreaterThanOrEqualTo(6);
        response.DaysRemaining.Should().BeLessThanOrEqualTo(8);
    }
}

using Microsoft.AspNetCore.Mvc;
using TooltipAI.Backend.Models;
using TooltipAI.Backend.Security;
using TooltipAI.Backend.Services;

namespace TooltipAI.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class LicenseController : ControllerBase
{
    private readonly LicenseService _licenseService;

    public LicenseController(LicenseService licenseService)
    {
        _licenseService = licenseService;
    }

    [HttpPost("validate")]
    public ActionResult<LicenseValidateResponse> Validate([FromBody] LicenseValidateRequest request)
    {
        var response = _licenseService.Validate(request);
        return Ok(response);
    }

    [HttpPost("generate")]
    [AdminApiKey]
    public ActionResult<object> Generate([FromBody] GenerateLicenseRequest request)
    {
        var key = _licenseService.GenerateLicenseKey(
            request.LicenseId,
            request.Tier,
            request.ExpiryDate);

        return Ok(new
        {
            licenseKey = key,
            licenseId = request.LicenseId,
            tier = request.Tier,
            expiresAt = request.ExpiryDate
        });
    }
}

public sealed class GenerateLicenseRequest
{
    public string LicenseId { get; init; } = string.Empty;
    public string Tier { get; init; } = "free";
    public DateTime ExpiryDate { get; init; } = DateTime.UtcNow.AddDays(14);
}

using Microsoft.AspNetCore.Mvc;
using TooltipAI.Backend.Models;
using TooltipAI.Backend.Services;

namespace TooltipAI.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LicenseController : ControllerBase
{
    private readonly LicenseService _licenseService;

    public LicenseController(LicenseService licenseService)
    {
        _licenseService = licenseService;
    }

    [HttpPost("validate")]
    public ActionResult<LicenseResponse> Validate([FromBody] LicenseRequest request)
    {
        var response = _licenseService.Validate(request);
        return Ok(response);
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "license", timestamp = DateTime.UtcNow });
    }
}

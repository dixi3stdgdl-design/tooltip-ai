using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TooltipAI.Backend.Models;
using TooltipAI.Backend.Services;

namespace TooltipAI.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    public ActionResult<AuthResponse> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new AuthResponse { Success = false, Error = "Invalid request" });

        var result = _authService.Register(request);

        if (!result.Success)
            return BadRequest(result);

        _logger.LogInformation("New user registered: {Email}", request.Email);
        return Ok(result);
    }

    [HttpPost("login")]
    public ActionResult<AuthResponse> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new AuthResponse { Success = false, Error = "Invalid request" });

        var result = _authService.Login(request);

        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }

    [HttpPost("refresh")]
    public ActionResult<AuthResponse> Refresh([FromBody] RefreshRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
            return BadRequest(new AuthResponse { Success = false, Error = "Refresh token required" });

        var result = _authService.RefreshToken(request.RefreshToken);

        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }

    [Authorize]
    [HttpGet("profile")]
    public ActionResult<UserProfile> GetProfile()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return Unauthorized();

        var profile = _authService.GetProfile(email);
        if (profile == null)
            return NotFound();

        return Ok(profile);
    }

    [Authorize]
    [HttpPost("change-password")]
    public ActionResult<AuthResponse> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return Unauthorized();

        // For now, just return success - implement actual password change logic
        return Ok(new AuthResponse { Success = true, Email = email });
    }

    [HttpGet("check")]
    public ActionResult<object> Check()
    {
        return Ok(new
        {
            authenticated = User.Identity?.IsAuthenticated ?? false,
            user = User.Identity?.Name
        });
    }
}

public class RefreshRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}

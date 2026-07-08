using System.ComponentModel.DataAnnotations;

namespace TooltipAI.Backend.Models;

public sealed class RegisterRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; init; } = string.Empty;

    [StringLength(100)]
    public string? DisplayName { get; init; }
}

public sealed class LoginRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; init; } = string.Empty;
}

public sealed class AuthResponse
{
    public bool Success { get; init; }
    public string? Token { get; init; }
    public string? RefreshToken { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public string? Email { get; init; }
    public string? DisplayName { get; init; }
    public string? Error { get; init; }
}

public sealed class UserProfile
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Tier { get; init; } = "free";
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
}

public sealed class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string NewPassword { get; init; } = string.Empty;
}

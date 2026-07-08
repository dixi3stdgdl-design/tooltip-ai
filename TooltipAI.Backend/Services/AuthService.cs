using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TooltipAI.Backend.Models;

namespace TooltipAI.Backend.Services;

public class AuthService
{
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;
    private readonly ConcurrentDictionary<string, UserRecord> _users = new();
    private readonly ConcurrentDictionary<string, RefreshTokenRecord> _refreshTokens = new();

    public AuthService(IConfiguration config, ILogger<AuthService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public AuthResponse Register(RegisterRequest request)
    {
        if (_users.ContainsKey(request.Email.ToLowerInvariant()))
        {
            return new AuthResponse { Success = false, Error = "Email already registered" };
        }

        var userId = Guid.NewGuid().ToString("N");
        var passwordHash = HashPassword(request.Password);

        var user = new UserRecord
        {
            Id = userId,
            Email = request.Email.ToLowerInvariant(),
            DisplayName = request.DisplayName ?? request.Email.Split('@')[0],
            PasswordHash = passwordHash,
            Tier = "free",
            CreatedAt = DateTime.UtcNow
        };

        _users[user.Email] = user;
        _logger.LogInformation("User registered: {Email}", user.Email);

        return GenerateTokens(user);
    }

    public AuthResponse Login(LoginRequest request)
    {
        if (!_users.TryGetValue(request.Email.ToLowerInvariant(), out var user))
        {
            return new AuthResponse { Success = false, Error = "Invalid email or password" };
        }

        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            return new AuthResponse { Success = false, Error = "Invalid email or password" };
        }

        user.LastLoginAt = DateTime.UtcNow;
        _logger.LogInformation("User logged in: {Email}", user.Email);

        return GenerateTokens(user);
    }

    public AuthResponse RefreshToken(string refreshToken)
    {
        if (!_refreshTokens.TryGetValue(refreshToken, out var record))
        {
            return new AuthResponse { Success = false, Error = "Invalid refresh token" };
        }

        if (record.ExpiresAt < DateTime.UtcNow)
        {
            _refreshTokens.TryRemove(refreshToken, out _);
            return new AuthResponse { Success = false, Error = "Refresh token expired" };
        }

        if (!_users.TryGetValue(record.Email, out var user))
        {
            return new AuthResponse { Success = false, Error = "User not found" };
        }

        _refreshTokens.TryRemove(refreshToken, out _);
        return GenerateTokens(user);
    }

    public UserProfile? GetProfile(string email)
    {
        if (!_users.TryGetValue(email.ToLowerInvariant(), out var user))
            return null;

        return new UserProfile
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            Tier = user.Tier,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtSecret()));
        var handler = new JwtSecurityTokenHandler();

        try
        {
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = "tooltip-ai-backend",
                ValidateAudience = true,
                ValidAudience = "tooltip-ai-client",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            }, out _);

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private AuthResponse GenerateTokens(UserRecord user)
    {
        var expiresAt = DateTime.UtcNow.AddHours(24);
        var token = GenerateJwtToken(user, expiresAt);
        var refreshToken = GenerateRefreshToken(user.Email);

        return new AuthResponse
        {
            Success = true,
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            Email = user.Email,
            DisplayName = user.DisplayName
        };
    }

    private string GenerateJwtToken(UserRecord user, DateTime expiresAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtSecret()));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("display_name", user.DisplayName),
            new Claim("tier", user.Tier),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: "tooltip-ai-backend",
            audience: "tooltip-ai-client",
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken(string email)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        _refreshTokens[token] = new RefreshTokenRecord
        {
            Email = email,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        return token;
    }

    private string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            100000,
            HashAlgorithmName.SHA256,
            32);

        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    private bool VerifyPassword(string password, string passwordHash)
    {
        var parts = passwordHash.Split('.');
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);
        var hash = Convert.FromBase64String(parts[1]);

        var testHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            100000,
            HashAlgorithmName.SHA256,
            32);

        return CryptographicOperations.FixedTimeEquals(hash, testHash);
    }

    private string GetJwtSecret()
    {
        return _config["Auth:JwtSecret"] ?? "tooltip-ai-jwt-secret-change-in-production-2026";
    }

    private sealed class UserRecord
    {
        public string Id { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public string PasswordHash { get; init; } = string.Empty;
        public string Tier { get; init; } = "free";
        public DateTime CreatedAt { get; init; }
        public DateTime? LastLoginAt { get; set; }
    }

    private sealed class RefreshTokenRecord
    {
        public string Email { get; init; } = string.Empty;
        public DateTime ExpiresAt { get; init; }
    }
}

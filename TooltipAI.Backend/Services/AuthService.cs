using System.Collections.Concurrent;
using System.Security.Cryptography;
using TooltipAI.Backend.Models;

namespace TooltipAI.Backend.Services;

public class AuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly ConcurrentDictionary<string, UserRecord> _users = new();
    private readonly ConcurrentDictionary<string, TokenRecord> _tokens = new();

    public AuthService(ILogger<AuthService> logger)
    {
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
        if (!_tokens.TryGetValue(refreshToken, out var record))
        {
            return new AuthResponse { Success = false, Error = "Invalid refresh token" };
        }

        if (record.ExpiresAt < DateTime.UtcNow)
        {
            _tokens.TryRemove(refreshToken, out _);
            return new AuthResponse { Success = false, Error = "Refresh token expired" };
        }

        if (!_users.TryGetValue(record.Email, out var user))
        {
            return new AuthResponse { Success = false, Error = "User not found" };
        }

        _tokens.TryRemove(refreshToken, out _);
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

    public string? ValidateTokenAndGetEmail(string token)
    {
        if (_tokens.TryGetValue(token, out var record) && record.ExpiresAt > DateTime.UtcNow)
        {
            return record.Email;
        }
        return null;
    }

    private AuthResponse GenerateTokens(UserRecord user)
    {
        var expiresAt = DateTime.UtcNow.AddHours(24);
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        _tokens[token] = new TokenRecord
        {
            Email = user.Email,
            ExpiresAt = expiresAt
        };

        _tokens[refreshToken] = new TokenRecord
        {
            Email = user.Email,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

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

    private sealed class TokenRecord
    {
        public string Email { get; init; } = string.Empty;
        public DateTime ExpiresAt { get; init; }
    }
}

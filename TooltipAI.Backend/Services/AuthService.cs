using System.Collections.Concurrent;
using System.Security.Cryptography;
using Microsoft.Azure.Cosmos;
using TooltipAI.Backend.Models;

namespace TooltipAI.Backend.Services;

public class AuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly Container? _usersContainer;
    private readonly ConcurrentDictionary<string, UserRecord> _inMemoryUsers = new();
    private readonly ConcurrentDictionary<string, TokenRecord> _tokens = new();
    private readonly bool _useCosmos;

    public AuthService(ILogger<AuthService> logger, CosmosClient? cosmosClient = null)
    {
        _logger = logger;
        if (cosmosClient != null)
        {
            try
            {
                _usersContainer = cosmosClient.GetDatabase("tooltipai").GetContainer("users");
                _useCosmos = true;
                _logger.LogInformation("Using Cosmos DB for auth storage");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to initialize Cosmos DB, using in-memory storage");
                _useCosmos = false;
            }
        }
        else
        {
            _logger.LogWarning("Cosmos DB not configured, using in-memory storage");
            _useCosmos = false;
        }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.ToLowerInvariant();

        if (_useCosmos && _usersContainer != null)
        {
            try
            {
                await _usersContainer.ReadItemAsync<UserDocument>(email, new PartitionKey(email));
                return new AuthResponse { Success = false, Error = "Email already registered" };
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // User doesn't exist, proceed
            }
        }
        else
        {
            if (_inMemoryUsers.ContainsKey(email))
                return new AuthResponse { Success = false, Error = "Email already registered" };
        }

        var passwordHash = HashPassword(request.Password);
        var user = new UserDocument
        {
            Id = email,
            Email = email,
            DisplayName = request.DisplayName ?? request.Email.Split('@')[0],
            PasswordHash = passwordHash,
            Tier = "free",
            CreatedAt = DateTime.UtcNow
        };

        if (_useCosmos && _usersContainer != null)
            await _usersContainer.CreateItemAsync(user, new PartitionKey(email));
        else
            _inMemoryUsers[email] = new UserRecord { Id = user.Id, Email = user.Email, DisplayName = user.DisplayName, PasswordHash = user.PasswordHash, Tier = user.Tier, CreatedAt = user.CreatedAt };

        _logger.LogInformation("User registered: {Email}", email);
        return GenerateTokens(email, user.DisplayName);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.ToLowerInvariant();
        string? storedHash = null;
        string displayName = email.Split('@')[0];

        if (_useCosmos && _usersContainer != null)
        {
            try
            {
                var response = await _usersContainer.ReadItemAsync<UserDocument>(email, new PartitionKey(email));
                var doc = response.Resource;
                storedHash = doc.PasswordHash;
                displayName = doc.DisplayName;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new AuthResponse { Success = false, Error = "Invalid email or password" };
            }
        }
        else
        {
            if (_inMemoryUsers.TryGetValue(email, out var u))
            {
                storedHash = u.PasswordHash;
                displayName = u.DisplayName;
            }
        }

        if (storedHash == null || !VerifyPassword(request.Password, storedHash))
            return new AuthResponse { Success = false, Error = "Invalid email or password" };

        _logger.LogInformation("User logged in: {Email}", email);
        return GenerateTokens(email, displayName);
    }

    public AuthResponse RefreshToken(string refreshToken)
    {
        if (!_tokens.TryGetValue(refreshToken, out var record))
            return new AuthResponse { Success = false, Error = "Invalid refresh token" };

        if (record.ExpiresAt < DateTime.UtcNow)
        {
            _tokens.TryRemove(refreshToken, out _);
            return new AuthResponse { Success = false, Error = "Refresh token expired" };
        }

        _tokens.TryRemove(refreshToken, out _);
        return GenerateTokens(record.Email, record.DisplayName);
    }

    public async Task<UserProfile?> GetProfileAsync(string email)
    {
        var e = email.ToLowerInvariant();

        if (_useCosmos && _usersContainer != null)
        {
            try
            {
                var response = await _usersContainer.ReadItemAsync<UserDocument>(e, new PartitionKey(e));
                var doc = response.Resource;
                return new UserProfile { Id = doc.Id, Email = doc.Email, DisplayName = doc.DisplayName, Tier = doc.Tier, CreatedAt = doc.CreatedAt, LastLoginAt = doc.LastLoginAt };
            }
            catch { return null; }
        }
        else
        {
            if (_inMemoryUsers.TryGetValue(e, out var u))
                return new UserProfile { Id = u.Id, Email = u.Email, DisplayName = u.DisplayName, Tier = u.Tier, CreatedAt = u.CreatedAt, LastLoginAt = u.LastLoginAt };
            return null;
        }
    }

    public string? ValidateTokenAndGetEmail(string token)
    {
        if (_tokens.TryGetValue(token, out var record) && record.ExpiresAt > DateTime.UtcNow)
            return record.Email;
        return null;
    }

    private AuthResponse GenerateTokens(string email, string displayName)
    {
        var expiresAt = DateTime.UtcNow.AddHours(24);
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        _tokens[token] = new TokenRecord { Email = email, DisplayName = displayName, ExpiresAt = expiresAt };
        _tokens[refreshToken] = new TokenRecord { Email = email, DisplayName = displayName, ExpiresAt = DateTime.UtcNow.AddDays(30) };

        return new AuthResponse { Success = true, Token = token, RefreshToken = refreshToken, ExpiresAt = expiresAt, Email = email, DisplayName = displayName };
    }

    private string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    private bool VerifyPassword(string password, string passwordHash)
    {
        var parts = passwordHash.Split('.');
        if (parts.Length != 2) return false;
        var salt = Convert.FromBase64String(parts[0]);
        var hash = Convert.FromBase64String(parts[1]);
        var testHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);
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
        public DateTime? LastLoginAt { get; init; }
    }

    private sealed class UserDocument
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Tier { get; set; } = "free";
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    private sealed class TokenRecord
    {
        public string Email { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public DateTime ExpiresAt { get; init; }
    }
}
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

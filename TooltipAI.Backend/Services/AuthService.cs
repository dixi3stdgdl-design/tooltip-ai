using System.Collections.Concurrent;
using System.Security.Cryptography;
using Microsoft.Azure.Cosmos;
using TooltipAI.Backend.Models;

namespace TooltipAI.Backend.Services;

public class AuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly Container? _usersContainer;
    private readonly ConcurrentDictionary<string, UserDocument> _inMemoryUsers = new();
    private readonly ConcurrentDictionary<string, TokenRecord> _tokens = new();
    private readonly bool _useCosmos;

    public AuthService(ILogger<AuthService> logger, CosmosClient? cosmosClient)
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
                _logger.LogWarning(ex, "Failed to initialize Cosmos DB container, using in-memory storage");
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
                // User doesn't exist, proceed with registration
            }
        }
        else
        {
            if (_inMemoryUsers.ContainsKey(email))
            {
                return new AuthResponse { Success = false, Error = "Email already registered" };
            }
        }

        var userId = Guid.NewGuid().ToString("N");
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
        {
            await _usersContainer.CreateItemAsync(user, new PartitionKey(email));
        }
        else
        {
            _inMemoryUsers[email] = user;
        }

        _logger.LogInformation("User registered: {Email}", user.Email);
        return GenerateTokens(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.ToLowerInvariant();

        UserDocument? user = null;

        if (_useCosmos && _usersContainer != null)
        {
            try
            {
                var response = await _usersContainer.ReadItemAsync<UserDocument>(email, new PartitionKey(email));
                user = response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new AuthResponse { Success = false, Error = "Invalid email or password" };
            }
        }
        else
        {
            _inMemoryUsers.TryGetValue(email, out user);
        }

        if (user == null)
        {
            return new AuthResponse { Success = false, Error = "Invalid email or password" };
        }

        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            return new AuthResponse { Success = false, Error = "Invalid email or password" };
        }

        user.LastLoginAt = DateTime.UtcNow;

        if (_useCosmos && _usersContainer != null)
        {
            await _usersContainer.UpsertItemAsync(user, new PartitionKey(email));
        }

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

        _tokens.TryRemove(refreshToken, out _);

        var email = record.Email;
        UserDocument? user = null;

        if (_useCosmos && _usersContainer != null)
        {
            try
            {
                var response = _usersContainer.ReadItemAsync<UserDocument>(email, new PartitionKey(email)).GetAwaiter().GetResult();
                user = response.Resource;
            }
            catch (CosmosException)
            {
                return new AuthResponse { Success = false, Error = "User not found" };
            }
        }
        else
        {
            _inMemoryUsers.TryGetValue(email, out user);
        }

        if (user == null)
        {
            return new AuthResponse { Success = false, Error = "User not found" };
        }

        return GenerateTokens(user);
    }

    public async Task<UserProfile?> GetProfileAsync(string email)
    {
        UserDocument? user = null;

        if (_useCosmos && _usersContainer != null)
        {
            try
            {
                var response = await _usersContainer.ReadItemAsync<UserDocument>(email.ToLowerInvariant(), new PartitionKey(email.ToLowerInvariant()));
                user = response.Resource;
            }
            catch (CosmosException)
            {
                return null;
            }
        }
        else
        {
            _inMemoryUsers.TryGetValue(email.ToLowerInvariant(), out user);
        }

        if (user == null) return null;

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

    private AuthResponse GenerateTokens(UserDocument user)
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
        public DateTime ExpiresAt { get; init; }
    }
}

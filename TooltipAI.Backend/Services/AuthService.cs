using System.Collections.Concurrent;
using System.Security.Cryptography;
using Microsoft.Azure.Cosmos;
using TooltipAI.Backend.Models;

namespace TooltipAI.Backend.Services;

public class AuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly Container _usersContainer;
    private readonly ConcurrentDictionary<string, TokenRecord> _tokens = new();

    public AuthService(ILogger<AuthService> logger, CosmosClient cosmosClient)
    {
        _logger = logger;
        _usersContainer = cosmosClient.GetDatabase("tooltipai").GetContainer("users");
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.ToLowerInvariant();

        try
        {
            await _usersContainer.ReadItemAsync<UserDocument>(email, new PartitionKey(email));
            return new AuthResponse { Success = false, Error = "Email already registered" };
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // User doesn't exist, proceed with registration
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

        await _usersContainer.CreateItemAsync(user, new PartitionKey(email));
        _logger.LogInformation("User registered: {Email}", user.Email);

        return GenerateTokens(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.ToLowerInvariant();

        UserDocument? user;
        try
        {
            var response = await _usersContainer.ReadItemAsync<UserDocument>(email, new PartitionKey(email));
            user = response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new AuthResponse { Success = false, Error = "Invalid email or password" };
        }

        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            return new AuthResponse { Success = false, Error = "Invalid email or password" };
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _usersContainer.UpsertItemAsync(user, new PartitionKey(email));
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

        // Fetch user from Cosmos DB
        var email = record.Email;
        try
        {
            var response = _usersContainer.ReadItemAsync<UserDocument>(email, new PartitionKey(email)).GetAwaiter().GetResult();
            return GenerateTokens(response.Resource);
        }
        catch (CosmosException)
        {
            return new AuthResponse { Success = false, Error = "User not found" };
        }
    }

    public async Task<UserProfile?> GetProfileAsync(string email)
    {
        try
        {
            var response = await _usersContainer.ReadItemAsync<UserDocument>(email.ToLowerInvariant(), new PartitionKey(email.ToLowerInvariant()));
            var user = response.Resource;

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
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
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

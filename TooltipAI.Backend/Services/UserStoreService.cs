using System.Collections.Concurrent;
using TooltipAI.Backend.Controllers;

namespace TooltipAI.Backend.Services;

public sealed class UserStoreService
{
    private readonly ILogger<UserStoreService> _logger;
    private readonly ConcurrentDictionary<string, UserInfo> _users = new();

    public UserStoreService(ILogger<UserStoreService> logger)
    {
        _logger = logger;
        SeedDemoUsers();
    }

    public UserInfo? GetUser(string userId)
    {
        _users.TryGetValue(userId, out var user);
        return user;
    }

    public IReadOnlyList<UserInfo> GetAllUsers(string tenantId)
    {
        return _users.Values
            .Where(u => u.TenantId == tenantId)
            .ToList()
            .AsReadOnly();
    }

    public bool UpdateUser(UserInfo user)
    {
        if (!_users.ContainsKey(user.UserId))
        {
            _logger.LogWarning("User not found: {UserId}", user.UserId);
            return false;
        }

        _users[user.UserId] = user;
        _logger.LogInformation("User updated: {UserId}", user.UserId);
        return true;
    }

    public int GetUserCount(string tenantId)
    {
        return _users.Values.Count(u => u.TenantId == tenantId);
    }

    public int GetTotalUserCount()
    {
        return _users.Count;
    }

    private void SeedDemoUsers()
    {
        var demoUsers = new[]
        {
            new UserInfo
            {
                UserId = "user-001",
                TenantId = "tenant-demo",
                Email = "alice@example.com",
                LicenseTier = "business",
                Role = "admin",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                LastActive = DateTime.UtcNow.AddMinutes(-5)
            },
            new UserInfo
            {
                UserId = "user-002",
                TenantId = "tenant-demo",
                Email = "bob@example.com",
                LicenseTier = "business",
                Role = "user",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                LastActive = DateTime.UtcNow.AddHours(-2)
            },
            new UserInfo
            {
                UserId = "user-003",
                TenantId = "tenant-demo",
                Email = "charlie@example.com",
                LicenseTier = "free",
                Role = "user",
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                LastActive = DateTime.UtcNow.AddDays(-1)
            }
        };

        foreach (var user in demoUsers)
        {
            _users.TryAdd(user.UserId, user);
        }
    }
}

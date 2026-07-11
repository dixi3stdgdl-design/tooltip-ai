using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using TooltipAI.Core.Common;

namespace TooltipAI.Core.Auth;

/// <summary>
/// Local user authentication and session management.
/// Supports email/password with local storage.
/// </summary>
public sealed class UserSession : IDisposable
{
    private readonly ILogger<UserSession> _logger;
    private readonly string _sessionPath;
    private UserSessionData? _currentSession;

    public bool IsLoggedIn => _currentSession != null && _currentSession.ExpiresAt > DateTime.UtcNow;
    public string? CurrentUserId => _currentSession?.UserId;
    public string? CurrentEmail => _currentSession?.Email;
    public string CurrentTier => _currentSession?.Tier ?? "free";

    public event Action<UserSessionData>? SessionChanged;

    public UserSession(ILogger<UserSession> logger, string? sessionPath = null)
    {
        _logger = logger;
        _sessionPath = sessionPath ?? AppDataPaths.Combine("session.json");
        
        _currentSession = LoadSession();
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        try
        {
            // In production, this would call the backend API
            // For now, simulate authentication
            await Task.Delay(100);

            var hashedPassword = HashPassword(password);
            
            // Create session
            _currentSession = new UserSessionData
            {
                UserId = Guid.NewGuid().ToString(),
                Email = email,
                Tier = "free",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };

            SaveSession(_currentSession);
            SessionChanged?.Invoke(_currentSession);

            _logger.LogInformation("User logged in: {Email}", email);

            return new AuthResult
            {
                Success = true,
                Session = _currentSession
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed");
            return new AuthResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<AuthResult> RegisterAsync(string email, string password, string name)
    {
        try
        {
            await Task.Delay(100);

            _currentSession = new UserSessionData
            {
                UserId = Guid.NewGuid().ToString(),
                Email = email,
                Name = name,
                Tier = "free",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            };

            SaveSession(_currentSession);
            SessionChanged?.Invoke(_currentSession);

            _logger.LogInformation("User registered: {Email}", email);

            return new AuthResult
            {
                Success = true,
                Session = _currentSession
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed");
            return new AuthResult { Success = false, Error = ex.Message };
        }
    }

    public void Logout()
    {
        _currentSession = null;
        if (File.Exists(_sessionPath))
            File.Delete(_sessionPath);
        
        _logger.LogInformation("User logged out");
    }

    public void UpgradeTier(string tier)
    {
        if (_currentSession != null)
        {
            _currentSession.Tier = tier;
            _currentSession.ExpiresAt = DateTime.UtcNow.AddMonths(1);
            SaveSession(_currentSession);
            SessionChanged?.Invoke(_currentSession);
        }
    }

    private UserSessionData? LoadSession()
        => JsonFile.Load<UserSessionData?>(_sessionPath, () => null, _logger, description: "session");

    private void SaveSession(UserSessionData session)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_sessionPath)!);
        JsonFile.Save(_sessionPath, session, _logger, description: "session");
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    public void Dispose()
    {
        // No unmanaged resources
    }
}

public sealed class UserSessionData
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Tier { get; set; } = "free";
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public sealed class AuthResult
{
    public bool Success { get; init; }
    public UserSessionData? Session { get; init; }
    public string? Error { get; init; }
}

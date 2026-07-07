using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TooltipAI.Backend.Models;

namespace TooltipAI.Backend.Services;

public sealed class LicenseService
{
    private readonly ILogger<LicenseService> _logger;
    private readonly string _hmacKey;
    private readonly Dictionary<string, LicenseInfo> _licenses = new();
    private readonly object _lock = new();

    public LicenseService(IConfiguration config, ILogger<LicenseService> logger)
    {
        _logger = logger;
        _hmacKey = config["License__HmacKey"]
            ?? Environment.GetEnvironmentVariable("License__HmacKey")
            ?? "dev-hmac-key-change-in-production";
    }

    public LicenseService(string hmacKey, ILogger<LicenseService> logger)
    {
        _logger = logger;
        _hmacKey = hmacKey;
    }

    public LicenseValidateResponse Validate(LicenseValidateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.LicenseKey))
        {
            return new LicenseValidateResponse
            {
                Valid = false,
                Error = "License key is required"
            };
        }

        try
        {
            var decoded = DecodeLicenseKey(request.LicenseKey);
            if (decoded == null)
            {
                return new LicenseValidateResponse
                {
                    Valid = false,
                    Error = "Invalid license key format"
                };
            }

            if (!VerifyHmac(decoded.LicenseId, decoded.ExpiryDate, decoded.Signature))
            {
                _logger.LogWarning("HMAC verification failed for license {LicenseId}", decoded.LicenseId);
                return new LicenseValidateResponse
                {
                    Valid = false,
                    Error = "Invalid license signature"
                };
            }

            if (decoded.ExpiryDate < DateTime.UtcNow)
            {
                return new LicenseValidateResponse
                {
                    Valid = false,
                    Tier = decoded.Tier,
                    ExpiresAt = decoded.ExpiryDate,
                    Error = "License expired",
                    DaysRemaining = 0
                };
            }

            var daysRemaining = (decoded.ExpiryDate - DateTime.UtcNow).Days;

            lock (_lock)
            {
                _licenses[decoded.LicenseId] = new LicenseInfo
                {
                    LicenseId = decoded.LicenseId,
                    LicenseKey = request.LicenseKey,
                    Tier = decoded.Tier,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = decoded.ExpiryDate,
                    MachineId = request.MachineId,
                    IsActive = true
                };
            }

            _logger.LogInformation("License validated: {LicenseId}, Tier: {Tier}, Expires: {Expires}",
                decoded.LicenseId, decoded.Tier, decoded.ExpiryDate);

            return new LicenseValidateResponse
            {
                Valid = true,
                Tier = decoded.Tier,
                ExpiresAt = decoded.ExpiryDate,
                DaysRemaining = daysRemaining
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating license");
            return new LicenseValidateResponse
            {
                Valid = false,
                Error = "License validation failed"
            };
        }
    }

    public string GenerateLicenseKey(string licenseId, string tier, DateTime expiryDate)
    {
        var payload = $"{licenseId}|{tier:O}|{expiryDate:O}";
        var signature = ComputeHmac(licenseId, expiryDate);
        var raw = $"{payload}|{signature}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
    }

    public async Task CreateLicenseAsync(LicenseInfo license)
    {
        lock (_lock)
        {
            _licenses[license.LicenseId] = license;
        }
        _logger.LogInformation("License created: {LicenseId}, Key: {Key}", license.LicenseId, license.LicenseKey);
        await Task.CompletedTask;
    }

    public async Task ActivateLicenseAsync(string licenseId)
    {
        lock (_lock)
        {
            if (_licenses.TryGetValue(licenseId, out var license))
            {
                _licenses[licenseId] = license with { IsActive = true };
            }
        }
        _logger.LogInformation("License activated: {LicenseId}", licenseId);
        await Task.CompletedTask;
    }

    public async Task DeactivateLicenseAsync(string licenseId)
    {
        lock (_lock)
        {
            if (_licenses.TryGetValue(licenseId, out var license))
            {
                _licenses[licenseId] = license with { IsActive = false };
            }
        }
        _logger.LogInformation("License deactivated: {LicenseId}", licenseId);
        await Task.CompletedTask;
    }

    public async Task ExtendLicenseAsync(string licenseKey, int days)
    {
        lock (_lock)
        {
            var license = _licenses.Values.FirstOrDefault(l => l.LicenseKey == licenseKey);
            if (license != null)
            {
                _licenses[license.LicenseId] = license with 
                { 
                    ExpiresAt = license.ExpiresAt.AddDays(days) 
                };
            }
        }
        _logger.LogInformation("License extended by {Days} days: {Key}", days, licenseKey);
        await Task.CompletedTask;
    }

    public async Task ExtendLicenseUntilAsync(string licenseId, DateTime until)
    {
        lock (_lock)
        {
            if (_licenses.TryGetValue(licenseId, out var license))
            {
                if (until > license.ExpiresAt)
                {
                    _licenses[licenseId] = license with { ExpiresAt = until };
                }
            }
        }
        _logger.LogInformation("License extended until {Until}: {LicenseId}", until, licenseId);
        await Task.CompletedTask;
    }

    public async Task RevokeLicenseAsync(string licenseKey)
    {
        lock (_lock)
        {
            var license = _licenses.Values.FirstOrDefault(l => l.LicenseKey == licenseKey);
            if (license != null)
            {
                _licenses.Remove(license.LicenseId);
            }
        }
        _logger.LogInformation("License revoked: {Key}", licenseKey);
        await Task.CompletedTask;
    }

    public async Task RevokeLicenseByOrderIdAsync(string orderId)
    {
        lock (_lock)
        {
            var license = _licenses.Values.FirstOrDefault(l => l.LicenseId == orderId);
            if (license != null)
            {
                _licenses.Remove(license.LicenseId);
                _logger.LogInformation("License revoked for order: {OrderId}", orderId);
            }
            else
            {
                _logger.LogWarning("No license found for order: {OrderId}", orderId);
            }
        }
        await Task.CompletedTask;
    }

    public async Task MarkLicensePastDueAsync(string licenseId)
    {
        lock (_lock)
        {
            if (_licenses.TryGetValue(licenseId, out var license))
            {
                // Mark as past due but keep active for grace period
                _licenses[licenseId] = license with { IsActive = true };
            }
        }
        _logger.LogInformation("License marked as past due: {LicenseId}", licenseId);
        await Task.CompletedTask;
    }

    public async Task<LicenseValidateResponse> ValidateLicenseAsync(string licenseKey)
    {
        var request = new LicenseValidateRequest
        {
            LicenseKey = licenseKey,
            MachineId = "web-validation"
        };
        return await Task.FromResult(Validate(request));
    }

    public LicenseInfo? GetCurrentLicense()
    {
        lock (_lock)
        {
            return _licenses.Values.FirstOrDefault(l => l.IsActive);
        }
    }

    public void SaveLicense(string licenseKey, string tier, DateTime expiresAt)
    {
        lock (_lock)
        {
            _licenses[$"local_{licenseKey}"] = new LicenseInfo
            {
                LicenseId = $"local_{licenseKey}",
                LicenseKey = licenseKey,
                Tier = tier,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                IsActive = true
            };
        }
    }

    public void ClearLicense()
    {
        lock (_lock)
        {
            var keysToRemove = _licenses.Where(l => l.Key.StartsWith("local_")).Select(l => l.Key).ToList();
            foreach (var key in keysToRemove)
            {
                _licenses.Remove(key);
            }
        }
    }

    private LicenseDecoded? DecodeLicenseKey(string licenseKey)
    {
        try
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(licenseKey));
            var parts = raw.Split('|');
            if (parts.Length != 4) return null;

            return new LicenseDecoded
            {
                LicenseId = parts[0],
                Tier = parts[1],
                ExpiryDate = DateTime.Parse(parts[2], null, System.Globalization.DateTimeStyles.RoundtripKind),
                Signature = parts[3]
            };
        }
        catch
        {
            return null;
        }
    }

    private string ComputeHmac(string licenseId, DateTime expiryDate)
    {
        var data = $"{licenseId}|{expiryDate:O}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_hmacKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }

    private bool VerifyHmac(string licenseId, DateTime expiryDate, string signature)
    {
        var expected = ComputeHmac(licenseId, expiryDate);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(signature));
    }

    private sealed class LicenseDecoded
    {
        public string LicenseId { get; init; } = string.Empty;
        public string Tier { get; init; } = string.Empty;
        public DateTime ExpiryDate { get; init; }
        public string Signature { get; init; } = string.Empty;
    }
}

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TooltipAI.Backend.Models;

namespace TooltipAI.Backend.Services;

public class LicenseService
{
    private readonly ILogger<LicenseService> _logger;
    private readonly string _hmacKey;
    private readonly Dictionary<string, LicenseInfo> _licenses = new();

    public LicenseService(IConfiguration config, ILogger<LicenseService> logger)
    {
        _logger = logger;
        _hmacKey = config["License:HmacKey"] ?? "TooltipAI-DefaultKey-Change-Me";
        SeedDemoLicenses();
    }

    private void SeedDemoLicenses()
    {
        _licenses["FREE-001"] = new LicenseInfo
        {
            LicenseId = "FREE-001",
            LicenseKey = "FREE-001",
            Plan = "free",
            CreatedAt = DateTime.UtcNow,
            ExpiryDate = DateTime.MaxValue,
            DailyRequestLimit = 10,
            IsActive = true
        };

        _licenses["PRO-001"] = new LicenseInfo
        {
            LicenseId = "PRO-001",
            LicenseKey = "PRO-001",
            Plan = "pro",
            CreatedAt = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddYears(1),
            DailyRequestLimit = -1,
            IsActive = true
        };
    }

    public LicenseResponse Validate(LicenseRequest request)
    {
        if (!_licenses.TryGetValue(request.LicenseKey, out var license))
        {
            return new LicenseResponse(
                Valid: false,
                LicenseId: null,
                ExpiryDate: null,
                Plan: null,
                DailyRequestsRemaining: 0,
                Message: "Invalid license key"
            );
        }

        if (!license.IsActive)
        {
            return new LicenseResponse(
                Valid: false,
                LicenseId: license.LicenseId,
                ExpiryDate: license.ExpiryDate,
                Plan: license.Plan,
                DailyRequestsRemaining: 0,
                Message: "License is deactivated"
            );
        }

        if (license.ExpiryDate < DateTime.UtcNow)
        {
            return new LicenseResponse(
                Valid: false,
                LicenseId: license.LicenseId,
                ExpiryDate: license.ExpiryDate,
                Plan: license.Plan,
                DailyRequestsRemaining: 0,
                Message: "License has expired"
            );
        }

        return new LicenseResponse(
            Valid: true,
            LicenseId: license.LicenseId,
            ExpiryDate: license.ExpiryDate,
            Plan: license.Plan,
            DailyRequestsRemaining: license.DailyRequestLimit,
            Message: "License valid"
        );
    }

    public string GenerateKey(string licenseId, string plan)
    {
        var payload = $"{licenseId}|{plan}|{DateTime.UtcNow:O}";
        var signature = ComputeHmac(payload);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{payload}|{signature}"));
    }

    private string ComputeHmac(string data)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_hmacKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }
}

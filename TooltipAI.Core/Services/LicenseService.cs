using System.Security.Cryptography;
using System.Text;

namespace TooltipAI.Core.Services;

public class LicenseService : IDisposable
{
    private const int TrialDays = 14;
    private const string RegistryKeyPath = @"SOFTWARE\TooltipAI";
    private const string FirstRunValue = "FirstRunDate";
    private const string LicenseKey = "LicenseKey";
    private static readonly string EncryptionKey =
        Environment.GetEnvironmentVariable("TOOLTIP_AI_ENCRYPTION_KEY")
        ?? "TooltipAI2026SecureKey!";

    private readonly string _settingsPath;
    private DateTime? _firstRunDate;
    private string? _licenseKey;

    public event Action<bool>? LicenseStatusChanged;

    public bool IsTrialActive => !IsLicensed && GetRemainingTrialDays() > 0;
    public bool IsLicensed => !string.IsNullOrEmpty(_licenseKey) && ValidateLicenseKey(_licenseKey!);
    public bool IsExpired => !IsLicensed && !IsTrialActive;

    public LicenseService(string? settingsPath = null)
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "TooltipAI");
        Directory.CreateDirectory(appFolder);

        _settingsPath = settingsPath ?? Path.Combine(appFolder, "license.dat");
        LoadLicense();
    }

    public int GetRemainingTrialDays()
    {
        if (_firstRunDate is null) return TrialDays;

        var elapsed = (DateTime.UtcNow - _firstRunDate.Value).TotalDays;
        var remaining = TrialDays - (int)elapsed;
        return Math.Max(0, remaining);
    }

    public DateTime? GetTrialExpirationDate()
    {
        return _firstRunDate?.AddDays(TrialDays);
    }

    public bool ActivateLicense(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;

        if (ValidateLicenseKey(key))
        {
            _licenseKey = key;
            SaveLicense();
            LicenseStatusChanged?.Invoke(true);
            return true;
        }

        return false;
    }

    public void StartTrial()
    {
        if (_firstRunDate is null)
        {
            _firstRunDate = DateTime.UtcNow;
            SaveLicense();
        }
    }

    public bool CanUseService()
    {
        return IsLicensed || IsTrialActive;
    }

    public string GetLicenseStatusMessage()
    {
        if (IsLicensed) return "Licensed";
        if (IsTrialActive) return $"Trial ({GetRemainingTrialDays()} days remaining)";
        return "Trial expired - Please purchase a license";
    }

    private bool ValidateLicenseKey(string key)
    {
        if (string.IsNullOrEmpty(key)) return false;

        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(key));
            var parts = decoded.Split('|');
            if (parts.Length != 3) return false;

            var licenseId = parts[0];
            var expiryStr = parts[1];
            var signature = parts[2];

            var expectedSignature = ComputeHmac($"{licenseId}|{expiryStr}", EncryptionKey);
            if (signature != expectedSignature) return false;

            if (DateTime.TryParse(expiryStr, out var expiry))
            {
                return expiry > DateTime.UtcNow;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public static string GenerateLicenseKey(string licenseId, DateTime expiry)
    {
        var data = $"{licenseId}|{expiry:O}";
        var signature = ComputeHmac(data, EncryptionKey);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{data}|{signature}"));
    }

    private static string ComputeHmac(string data, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }

    private void LoadLicense()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var lines = File.ReadAllLines(_settingsPath);
                foreach (var line in lines)
                {
                    var parts = line.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        switch (parts[0])
                        {
                            case FirstRunValue:
                                if (DateTime.TryParse(parts[1], out var firstRun))
                                    _firstRunDate = firstRun;
                                break;
                            case LicenseKey:
                                _licenseKey = parts[1];
                                break;
                        }
                    }
                }
            }

            if (_firstRunDate is null && !IsLicensed)
            {
                StartTrial();
            }
        }
        catch
        {
            _firstRunDate = DateTime.UtcNow;
            SaveLicense();
        }
    }

    private void SaveLicense()
    {
        try
        {
            var lines = new List<string>();
            if (_firstRunDate.HasValue)
                lines.Add($"{FirstRunValue}={_firstRunDate.Value:O}");
            if (!string.IsNullOrEmpty(_licenseKey))
                lines.Add($"{LicenseKey}={_licenseKey}");

            File.WriteAllLines(_settingsPath, lines);
        }
        catch
        {
            // Log but don't throw
        }
    }

    public void Dispose()
    {
        // No unmanaged resources
    }
}

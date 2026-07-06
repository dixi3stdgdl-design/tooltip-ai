using System.Windows;
using System.Windows.Media;
using TooltipAI.Core.Services;

namespace TooltipAI.UI.Views;

public partial class LicenseWindow : Window
{
    private readonly LicenseService _licenseService;
    private readonly UsageMeteringService _usageService;

    public LicenseWindow()
    {
        InitializeComponent();
        
        _licenseService = new LicenseService();
        _usageService = new UsageMeteringService();
        
        LoadLicenseStatus();
        LoadUsageStats();
    }

    private void LoadLicenseStatus()
    {
        var license = _licenseService.GetCurrentLicense();
        
        if (license != null && license.IsActive)
        {
            TxtStatus.Text = "Activated";
            TxtStatus.Foreground = Brushes.Green;
            TxtTier.Text = license.Tier;
            TxtExpiry.Text = license.ExpiresAt.ToString("yyyy-MM-dd");
            
            BtnActivate.IsEnabled = false;
            BtnDeactivate.IsEnabled = true;
            
            // Hide usage for paid tiers
            if (license.Tier != "free")
            {
                GrpUsage.Visibility = Visibility.Collapsed;
            }
        }
        else
        {
            TxtStatus.Text = "Not Activated";
            TxtStatus.Foreground = Brushes.Gray;
            TxtTier.Text = "Free";
            TxtExpiry.Text = "N/A";
            
            BtnActivate.IsEnabled = true;
            BtnDeactivate.IsEnabled = false;
        }
    }

    private void LoadUsageStats()
    {
        var stats = _usageService.GetStats();
        TxtUsage.Text = $"{stats.DailyCount}/{stats.DailyLimit} tooltips";
        TxtRemaining.Text = $"{stats.RemainingToday} tooltips";
        
        if (stats.RemainingToday == 0)
        {
            TxtRemaining.Foreground = Brushes.Red;
        }
        else if (stats.RemainingToday <= 3)
        {
            TxtRemaining.Foreground = Brushes.Orange;
        }
        else
        {
            TxtRemaining.Foreground = Brushes.Green;
        }
    }

    private async void BtnActivate_Click(object sender, RoutedEventArgs e)
    {
        var licenseKey = TxtLicenseKey.Text.Trim();
        
        if (string.IsNullOrEmpty(licenseKey))
        {
            ShowMessage("Please enter a license key.", false);
            return;
        }

        try
        {
            var result = await _licenseService.ValidateLicenseAsync(licenseKey);
            
            if (result.Valid)
            {
                _licenseService.SaveLicense(licenseKey, result.Tier ?? "pro", result.ExpiresAt ?? DateTime.UtcNow.AddMonths(1));
                ShowMessage("License activated successfully!", true);
                LoadLicenseStatus();
                LoadUsageStats();
            }
            else
            {
                ShowMessage($"Invalid license: {result.Error ?? "Unknown error"}", false);
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Activation failed: {ex.Message}", false);
        }
    }

    private void BtnDeactivate_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to deactivate your license?",
            "Confirm Deactivation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes)
        {
            _licenseService.ClearLicense();
            ShowMessage("License deactivated.", true);
            LoadLicenseStatus();
            LoadUsageStats();
        }
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ShowMessage(string message, bool isSuccess)
    {
        TxtMessage.Text = message;
        TxtMessage.Foreground = isSuccess ? Brushes.Green : Brushes.Red;
    }

    protected override void OnClosed(EventArgs e)
    {
        _usageService.Dispose();
        base.OnClosed(e);
    }
}

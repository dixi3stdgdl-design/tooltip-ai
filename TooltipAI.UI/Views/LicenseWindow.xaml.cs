using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using TooltipAI.Core.Services;

namespace TooltipAI.UI.Views;

public sealed partial class LicenseWindow : Window
{
    private readonly LicenseService _licenseService;
    private readonly UsageMeteringService _usageService;

    public LicenseWindow()
    {
        InitializeComponent();
        
        // Set window size (WinUI 3 doesn't support Width/Height in XAML)
        this.AppWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 450, Height = 350 });
        
        _licenseService = new LicenseService();
        _usageService = new UsageMeteringService();
        
        LoadLicenseStatus();
        LoadUsageStats();
    }

    private void LoadLicenseStatus()
    {
        if (_licenseService.IsLicensed)
        {
            TxtStatus.Text = "Activated";
            TxtStatus.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Green);
            TxtTier.Text = "Pro";
            TxtExpiry.Text = _licenseService.GetLicenseStatusMessage();
            
            BtnActivate.IsEnabled = false;
            BtnDeactivate.IsEnabled = true;
            GrpUsage.Visibility = Visibility.Collapsed;
        }
        else if (_licenseService.IsTrialActive)
        {
            TxtStatus.Text = "Trial Active";
            TxtStatus.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Orange);
            TxtTier.Text = "Free (Trial)";
            TxtExpiry.Text = $"{_licenseService.GetRemainingTrialDays()} days remaining";
            
            BtnActivate.IsEnabled = true;
            BtnDeactivate.IsEnabled = false;
        }
        else
        {
            TxtStatus.Text = "Not Activated";
            TxtStatus.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray);
            TxtTier.Text = "Free";
            TxtExpiry.Text = "Trial expired";
            
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
            TxtRemaining.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
        }
        else if (stats.RemainingToday <= 3)
        {
            TxtRemaining.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Orange);
        }
        else
        {
            TxtRemaining.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Green);
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
            var success = _licenseService.ActivateLicense(licenseKey);
            
            if (success)
            {
                ShowMessage("License activated successfully!", true);
                LoadLicenseStatus();
                LoadUsageStats();
            }
            else
            {
                ShowMessage("Invalid license key. Please check and try again.", false);
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Activation failed: {ex.Message}", false);
        }
    }

    private async void BtnDeactivate_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Confirm Deactivation",
            Content = "Are you sure you want to deactivate your license?",
            PrimaryButtonText = "Yes",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        
        if (result == ContentDialogResult.Primary)
        {
            _licenseService.StartTrial();
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
        TxtMessage.Foreground = new SolidColorBrush(
            isSuccess ? Microsoft.UI.Colors.Green : Microsoft.UI.Colors.Red);
    }

    protected override void OnClosed(WindowEventArgs e)
    {
        _usageService.Dispose();
        base.OnClosed(e);
    }
}

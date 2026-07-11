using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TooltipAI.Core.Services;

namespace TooltipAI.UI.Views;

public sealed partial class PrivacySettingsWindow : Window
{
    private readonly ConsentManager _consentManager;
    private readonly AppBlacklistService _blacklistService;

    public PrivacySettingsWindow()
    {
        InitializeComponent();
        
        // Set window size (WinUI 3 doesn't support Width/Height in XAML)
        this.AppWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 500, Height = 450 });
        
        _consentManager = new ConsentManager();
        _blacklistService = new AppBlacklistService();
        
        LoadSettings();
    }

    private void LoadSettings()
    {
        ChkEnableAgent.IsChecked = true;
        ChkAIEnrichment.IsChecked = _consentManager.State.AIEnrichmentEnabled;
        ChkTelemetry.IsChecked = _consentManager.State.TelemetryEnabled;
        ChkLocalOnly.IsChecked = _consentManager.State.LocalOnlyMode;
        
        LstBlacklist.ItemsSource = _blacklistService.Blacklist;
    }

    private async void BtnAddToBlacklist_Click(object sender, RoutedEventArgs e)
    {
        var appName = TxtBlacklistApp.Text.Trim();
        if (!string.IsNullOrEmpty(appName))
        {
            try
            {
                _blacklistService.Add(appName);
                TxtBlacklistApp.Text = string.Empty;
                LstBlacklist.ItemsSource = _blacklistService.Blacklist;
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Could not update the blacklist", ex);
            }
        }
    }

    private async void BtnRemoveFromBlacklist_Click(object sender, RoutedEventArgs e)
    {
        if (LstBlacklist.SelectedItem is string selectedApp)
        {
            try
            {
                _blacklistService.Remove(selectedApp);
                LstBlacklist.ItemsSource = _blacklistService.Blacklist;
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Could not update the blacklist", ex);
            }
        }
    }

    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _consentManager.EnableAIEnrichment(ChkAIEnrichment.IsChecked ?? false);
            _consentManager.EnableTelemetry(ChkTelemetry.IsChecked ?? false);
            _consentManager.SetLocalOnlyMode(ChkLocalOnly.IsChecked ?? true);

            var dialog = new ContentDialog
            {
                Title = "Success",
                Content = "Settings saved successfully.",
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();

            Close();
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Could not save privacy settings", ex);
        }
    }

    private async void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        var confirmDialog = new ContentDialog
        {
            Title = "Confirm Reset",
            Content = "Reset all privacy settings to defaults?",
            PrimaryButtonText = "Yes",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.Content.XamlRoot
        };

        var result = await confirmDialog.ShowAsync();
        
        if (result == ContentDialogResult.Primary)
        {
            try
            {
                _consentManager.ResetToDefaults();
                _blacklistService.Clear();
                LoadSettings();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Could not reset privacy settings", ex);
            }
        }
    }

    private async Task ShowErrorAsync(string title, Exception ex)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = ex.Message,
            CloseButtonText = "OK",
            XamlRoot = this.Content.XamlRoot
        };
        await dialog.ShowAsync();
    }

    protected override void OnClosed(WindowEventArgs e)
    {
        _consentManager.Dispose();
        _blacklistService.Dispose();
        base.OnClosed(e);
    }
}

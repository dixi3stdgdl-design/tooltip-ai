using System.Windows;
using TooltipAI.Core.Services;

namespace TooltipAI.UI.Views;

public partial class PrivacySettingsWindow : Window
{
    private readonly ConsentManager _consentManager;
    private readonly AppBlacklistService _blacklistService;

    public PrivacySettingsWindow()
    {
        InitializeComponent();
        
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

    private void BtnAddToBlacklist_Click(object sender, RoutedEventArgs e)
    {
        var appName = TxtBlacklistApp.Text.Trim();
        if (!string.IsNullOrEmpty(appName))
        {
            _blacklistService.Add(appName);
            TxtBlacklistApp.Text = string.Empty;
            LstBlacklist.ItemsSource = _blacklistService.Blacklist;
        }
    }

    private void BtnRemoveFromBlacklist_Click(object sender, RoutedEventArgs e)
    {
        if (LstBlacklist.SelectedItem is string selectedApp)
        {
            _blacklistService.Remove(selectedApp);
            LstBlacklist.ItemsSource = _blacklistService.Blacklist;
        }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        _consentManager.EnableAIEnrichment(ChkAIEnrichment.IsChecked ?? false);
        _consentManager.EnableTelemetry(ChkTelemetry.IsChecked ?? false);
        _consentManager.SetLocalOnlyMode(ChkLocalOnly.IsChecked ?? true);
        
        MessageBox.Show("Settings saved successfully.", "Success", 
            MessageBoxButton.OK, MessageBoxImage.Information);
        
        DialogResult = true;
        Close();
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Reset all privacy settings to defaults?", 
            "Confirm Reset", 
            MessageBoxButton.YesNo, 
            MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes)
        {
            _consentManager.ResetToDefaults();
            _blacklistService.Clear();
            LoadSettings();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _consentManager.Dispose();
        _blacklistService.Dispose();
        base.OnClosed(e);
    }
}

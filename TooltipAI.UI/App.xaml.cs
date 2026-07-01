using System.Diagnostics;
using Microsoft.UI.Xaml;

namespace TooltipAI.UI;

public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        EnsureServiceRunning();

        m_window = new MainWindow();
        m_window.Activate();
    }

    private static void EnsureServiceRunning()
    {
        var serviceProcess = Process.GetProcessesByName("TooltipAI.Service").FirstOrDefault();
        if (serviceProcess is null)
        {
            var servicePath = Path.Combine(AppContext.BaseDirectory, "TooltipAI.Service");
            if (File.Exists(servicePath + ".exe"))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = servicePath + ".exe",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                Thread.Sleep(2000);
            }
        }
    }

    private Window? m_window;
}

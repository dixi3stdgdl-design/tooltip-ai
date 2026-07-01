using System.Diagnostics;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace TooltipAI.UI;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        Console.WriteLine("[UI] Starting TooltipAI WinUI 3...");

        EnsureServiceRunning();

        Application.Start((p) =>
        {
            var context = new DispatcherQueueSynchronizationContext(
                DispatcherQueue.GetForCurrentThread());
            System.Threading.SynchronizationContext.SetSynchronizationContext(context);
            new App();
        });
    }

    static void EnsureServiceRunning()
    {
        var serviceProcess = Process.GetProcessesByName("TooltipAI.Service").FirstOrDefault();
        if (serviceProcess is null)
        {
            Console.WriteLine("[UI] Starting TooltipAI.Service...");
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
}

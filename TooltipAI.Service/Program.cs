using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TooltipAI.Core.Interfaces;
using TooltipAI.Core.Models;
using TooltipAI.Core.Plugins;
using TooltipAI.Core.Services;
using TooltipAI.Service.Services;
using TooltipAI.Service.Workers;

Console.WriteLine("[SERVICE] Starting...");

var builder = Host.CreateApplicationBuilder(args);

// Register services via DI
builder.Services.AddSingleton<SettingsService>();
builder.Services.AddSingleton<LoggingService>();
builder.Services.AddSingleton<PerformanceMonitor>();
builder.Services.AddSingleton<CrashRecoveryService>();
builder.Services.AddSingleton<KeyboardShortcutService>();
builder.Services.AddSingleton<MultiMonitorService>();
builder.Services.AddSingleton<NamedPipeService>();
builder.Services.AddSingleton<UpdateService>();

// Register platform-specific UIA service via reflection.
// On Windows, loads TooltipAI.Platform.Win.dll and resolves WindowsUIAutomationService.
// On other platforms, falls back to the built-in UIAutomationService (window-level only).
builder.Services.AddSingleton<IUIAutomationService>(sp =>
{
    if (OperatingSystem.IsWindows())
    {
        try
        {
            var platformAssembly = Assembly.Load("TooltipAI.Platform.Win");
            var serviceType = platformAssembly.GetType(
                "TooltipAI.Platform.Win.Services.WindowsUIAutomationService");
            if (serviceType != null)
            {
                Console.WriteLine("[SERVICE] Using WindowsUIAutomationService (real UIA)");
                return (IUIAutomationService)Activator.CreateInstance(serviceType)!;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SERVICE] Failed to load Platform.Win: {ex.Message}");
        }
    }

    Console.WriteLine("[SERVICE] Using fallback UIAutomationService (window-level only)");
    return new UIAutomationService();
});

builder.Services.AddSingleton<HybridAiService>();
builder.Services.AddSingleton<IAIService>(sp => sp.GetRequiredService<HybridAiService>());

// Register plugin loader
builder.Services.AddSingleton(sp =>
{
    var loader = new PluginLoader();
    loader.LoadPlugins();
    return loader;
});

// Register background workers
builder.Services.AddHostedService<MouseMonitorWorkerService>();
builder.Services.AddHostedService<UpdateCheckerService>();

var host = builder.Build();

Console.WriteLine("[SERVICE] DI container built. Starting services...");

await host.RunAsync();

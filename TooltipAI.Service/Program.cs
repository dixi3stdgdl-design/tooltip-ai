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
builder.Services.AddSingleton<IUIAutomationService, UIAutomationService>();
builder.Services.AddSingleton<NamedPipeService>();
builder.Services.AddSingleton<UpdateService>();

builder.Services.AddSingleton<IAIService>(sp =>
{
    var config = new AiComplexityConfig
    {
        DefaultLevel = AiComplexityLevel.Basic,
        EnableCloudApi = false,
        ApiTimeoutMs = 3000,
        CacheExpirationMinutes = 5,
        MaxCacheSize = 1000
    };
    return new HybridAiService(config);
});

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

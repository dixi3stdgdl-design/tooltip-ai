using System.Runtime.InteropServices;
using TooltipAI.Core.Interfaces;
using TooltipAI.Core.Models;
using TooltipAI.Core.Services;
using TooltipAI.Service.Services;
using TooltipAI.Service.Workers;

Console.WriteLine("[SERVICE] Starting...");

var aiConfig = new AiComplexityConfig
{
    DefaultLevel = AiComplexityLevel.Basic,
    EnableCloudApi = false,
    ApiTimeoutMs = 3000,
    CacheExpirationMinutes = 5,
    MaxCacheSize = 1000
};

var settings = new SettingsService();
var logger = new LoggingService();
var perfMonitor = new PerformanceMonitor();
var crashRecovery = new CrashRecoveryService();
var keyboard = new KeyboardShortcutService();
var multiMonitor = new MultiMonitorService();
var uiaService = new UIAutomationService();
var pipeService = new NamedPipeService();
var aiService = new HybridAiService(aiConfig);

Console.WriteLine("[SERVICE] Services created");

logger.LogInfo("TooltipAI Service starting", "Program");

keyboard.RegisterDefaultShortcuts();
keyboard.OnToggleStateChanged += enabled =>
{
    logger.LogInfo($"Tooltips {(enabled ? "enabled" : "disabled")} via keyboard shortcut", "KeyboardShortcut");
};

pipeService.Start();
Console.WriteLine("[SERVICE] Pipe server started. Waiting for connections...");

var mouseWorker = new MouseMonitorWorker(uiaService, pipeService, multiMonitor, logger, aiService);
mouseWorker.Start();
Console.WriteLine("[SERVICE] Mouse worker started");

Console.WriteLine("[SERVICE] Running. Press Ctrl+C to stop.");

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

try
{
    while (!cts.Token.IsCancellationRequested)
    {
        await Task.Delay(1000, cts.Token);
    }
}
catch (OperationCanceledException)
{
}

Console.WriteLine("[SERVICE] Stopping...");
mouseWorker.Stop();
logger.LogInfo("TooltipAI Service stopping", "Program");
settings.Dispose();
logger.Dispose();
Console.WriteLine("[SERVICE] Exiting.");

using Microsoft.Extensions.Hosting;
using TooltipAI.Core.Interfaces;
using TooltipAI.Core.Services;
using TooltipAI.Core.Plugins;
using TooltipAI.Service.Services;

namespace TooltipAI.Service.Workers;

/// <summary>
/// IHostedService wrapper for MouseMonitorWorker.
/// Bridges DI-resolved services to the existing worker.
/// </summary>
public class MouseMonitorWorkerService : IHostedService
{
    private readonly IUIAutomationService _uiaService;
    private readonly NamedPipeService _pipeService;
    private readonly MultiMonitorService _multiMonitor;
    private readonly LoggingService _logger;
    private readonly HybridAiService _aiService;
    private readonly PluginLoader _plugins;
    private MouseMonitorWorker? _worker;

    public MouseMonitorWorkerService(
        IUIAutomationService uiaService,
        NamedPipeService pipeService,
        MultiMonitorService multiMonitor,
        LoggingService logger,
        HybridAiService aiService,
        PluginLoader plugins)
    {
        _uiaService = uiaService;
        _pipeService = pipeService;
        _multiMonitor = multiMonitor;
        _logger = logger;
        _aiService = aiService;
        _plugins = plugins;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInfo("Starting mouse monitor with plugin support", "WorkerService");
        _pipeService.Start();

        _worker = new MouseMonitorWorker(
            _uiaService, _pipeService, _multiMonitor, _logger,
            _aiService);
        _worker.Start();

        Console.WriteLine("[SERVICE] Mouse worker started");
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInfo("Stopping mouse monitor", "WorkerService");
        _worker?.Stop();
        await _pipeService.StopAsync(cancellationToken);
    }
}

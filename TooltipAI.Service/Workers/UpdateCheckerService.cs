using Microsoft.Extensions.Hosting;
using TooltipAI.Core.Services;

namespace TooltipAI.Service.Workers;

/// <summary>
/// Background service that checks for updates every 6 hours.
/// </summary>
public class UpdateCheckerService : BackgroundService
{
    private readonly UpdateService _updateService;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6);

    public UpdateCheckerService(UpdateService updateService)
    {
        _updateService = updateService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var release = await _updateService.CheckForUpdateAsync(stoppingToken);
                if (release != null)
                {
                    Console.WriteLine($"[Update] New version available: v{release.Version}");
                    Console.WriteLine($"[Update] Current: v{_updateService.CurrentVersion}");
                    Console.WriteLine($"[Update] Release notes: {release.ReleaseNotes[..Math.Min(200, release.ReleaseNotes.Length)]}...");
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Update] Check failed: {ex.Message}");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
}

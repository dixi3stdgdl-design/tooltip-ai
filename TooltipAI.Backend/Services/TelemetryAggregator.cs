using System.Collections.Concurrent;
using TooltipAI.Backend.Controllers;

namespace TooltipAI.Backend.Services;

public class TelemetryAggregator
{
    private readonly ConcurrentDictionary<string, TelemetryEvent> _events = new();
    private readonly ConcurrentDictionary<string, HashSet<string>> _usersByTenant = new();
    private readonly ConcurrentDictionary<string, long> _eventCounts = new();
    private readonly ILogger<TelemetryAggregator> _logger;

    public TelemetryAggregator(ILogger<TelemetryAggregator> logger)
    {
        _logger = logger;
    }

    public void TrackEvent(TelemetryEvent telemetryEvent)
    {
        var key = $"{telemetryEvent.EventType}:{telemetryEvent.UserId}:{telemetryEvent.Timestamp:yyyyMMddHHmm}";
        _events[key] = telemetryEvent;

        _eventCounts.AddOrUpdate(telemetryEvent.EventType, 1, (k, v) => v + 1);

        if (!string.IsNullOrEmpty(telemetryEvent.TenantId) && !string.IsNullOrEmpty(telemetryEvent.UserId))
        {
            _usersByTenant.AddOrUpdate(
                telemetryEvent.TenantId,
                new HashSet<string> { telemetryEvent.UserId },
                (k, v) =>
                {
                    lock (v)
                    {
                        v.Add(telemetryEvent.UserId);
                    }
                    return v;
                });
        }

        CleanupOldEvents();
    }

    public TelemetryMetrics GetMetrics(string? tenantId, string? period)
    {
        var cutoff = GetCutoffDate(period);
        var recentEvents = _events.Values.Where(e => e.Timestamp >= cutoff).ToList();

        if (!string.IsNullOrEmpty(tenantId))
        {
            recentEvents = recentEvents.Where(e => e.TenantId == tenantId).ToList();
        }

        var uniqueUsers = recentEvents
            .Where(e => !string.IsNullOrEmpty(e.UserId))
            .Select(e => e.UserId)
            .Distinct()
            .Count();

        var totalTooltips = recentEvents.Count(e => e.EventType == "tooltip.shown");
        var enrichmentUsage = recentEvents.Count(e => e.EventType == "enrichment.used");
        var feedbackEvents = recentEvents.Where(e => e.EventType == "tooltip.feedback").ToList();
        var positiveFeedback = feedbackEvents.Count(e => 
            e.Properties?.ContainsKey("useful") == true && 
            e.Properties["useful"] == "true");

        var eventsByType = recentEvents
            .GroupBy(e => e.EventType)
            .ToDictionary(g => g.Key, g => (long)g.Count());

        return new TelemetryMetrics
        {
            TotalEvents = recentEvents.Count,
            UniqueUsers = uniqueUsers,
            AverageTooltipsPerUser = uniqueUsers > 0 ? (double)totalTooltips / uniqueUsers : 0,
            EnrichmentUsageRate = totalTooltips > 0 ? (double)enrichmentUsage / totalTooltips : 0,
            RelevanceRate = feedbackEvents.Count > 0 ? (double)positiveFeedback / feedbackEvents.Count : 0,
            EventsByType = eventsByType
        };
    }

    private DateTime GetCutoffDate(string? period)
    {
        return period?.ToLowerInvariant() switch
        {
            "1d" => DateTime.UtcNow.AddDays(-1),
            "7d" => DateTime.UtcNow.AddDays(-7),
            "30d" => DateTime.UtcNow.AddDays(-30),
            "90d" => DateTime.UtcNow.AddDays(-90),
            _ => DateTime.UtcNow.AddDays(-7)
        };
    }

    private void CleanupOldEvents()
    {
        if (_events.Count > 100000)
        {
            var cutoff = DateTime.UtcNow.AddDays(-30);
            var keysToRemove = _events
                .Where(e => e.Value.Timestamp < cutoff)
                .Select(e => e.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _events.TryRemove(key, out _);
            }

            _logger.LogDebug("Cleaned up {Count} old telemetry events", keysToRemove.Count);
        }
    }

    public TenantMetrics GetTenantMetrics(string tenantId, string period)
    {
        var cutoff = GetCutoffDate(period);
        var tenantEvents = _events.Values
            .Where(e => e.TenantId == tenantId && e.Timestamp >= cutoff)
            .ToList();

        var uniqueUsers = tenantEvents
            .Where(e => !string.IsNullOrEmpty(e.UserId))
            .Select(e => e.UserId)
            .Distinct()
            .Count();

        var totalTooltips = tenantEvents.Count(e => e.EventType == "tooltip.shown");
        var enrichmentUsage = tenantEvents.Count(e => e.EventType == "enrichment.used");
        var feedbackEvents = tenantEvents.Where(e => e.EventType == "tooltip.feedback").ToList();
        var positiveFeedback = feedbackEvents.Count(e =>
            e.Properties?.ContainsKey("useful") == true &&
            e.Properties["useful"] == "true");

        var sevenDayCutoff = DateTime.UtcNow.AddDays(-7);
        var thirtyDayCutoff = DateTime.UtcNow.AddDays(-30);
        var sevenDayUsers = tenantEvents
            .Where(e => e.Timestamp >= sevenDayCutoff && !string.IsNullOrEmpty(e.UserId))
            .Select(e => e.UserId)
            .Distinct()
            .Count();
        var thirtyDayUsers = tenantEvents
            .Where(e => e.Timestamp >= thirtyDayCutoff && !string.IsNullOrEmpty(e.UserId))
            .Select(e => e.UserId)
            .Distinct()
            .Count();

        return new TenantMetrics
        {
            TenantId = tenantId,
            Period = period,
            ActiveUsers = uniqueUsers,
            TotalTooltipsShown = totalTooltips,
            AverageTooltipsPerUser = uniqueUsers > 0 ? (double)totalTooltips / uniqueUsers : 0,
            EnrichmentUsageRate = totalTooltips > 0 ? (double)enrichmentUsage / totalTooltips : 0,
            Retention7Day = uniqueUsers > 0 ? (double)sevenDayUsers / uniqueUsers : 0,
            Retention30Day = uniqueUsers > 0 ? (double)thirtyDayUsers / uniqueUsers : 0
        };
    }

    public int GetActiveUserCount(int sinceMinutes)
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-sinceMinutes);
        return _events.Values
            .Where(e => e.Timestamp >= cutoff && !string.IsNullOrEmpty(e.UserId))
            .Select(e => e.UserId)
            .Distinct()
            .Count();
    }
}

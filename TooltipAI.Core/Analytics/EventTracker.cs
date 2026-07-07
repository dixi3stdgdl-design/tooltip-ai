using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace TooltipAI.Core.Analytics;

/// <summary>
/// Event tracking for Tooltip AI - opt-in only, privacy-first.
/// Uses PostHog self-hosted or cloud for analytics.
/// </summary>
public sealed class EventTracker : IDisposable
{
    private readonly ILogger<EventTracker> _logger;
    private readonly string _apiKey;
    private readonly string _endpoint;
    private readonly HttpClient _httpClient;
    private readonly bool _enabled;
    private readonly string _distinctId;

    public EventTracker(ILogger<EventTracker> logger, HttpClient httpClient, string? apiKey = null, string? endpoint = null)
    {
        _logger = logger;
        _httpClient = httpClient;
        _apiKey = apiKey ?? string.Empty;
        _endpoint = endpoint ?? "https://app.posthog.com";
        _enabled = !string.IsNullOrEmpty(_apiKey);
        _distinctId = GetOrCreateDistinctId();
    }

    public void Track(string eventName, Dictionary<string, object>? properties = null)
    {
        if (!_enabled) return;

        try
        {
            var payload = new
            {
                @event = eventName,
                properties = properties ?? new Dictionary<string, object>(),
                distinct_id = _distinctId,
                timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.PostAsync($"{_endpoint}/capture/", content);
            _logger.LogDebug("Tracked event: {Event}", eventName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to track event");
        }
    }

    public void Identify(string userId, Dictionary<string, object>? traits = null)
    {
        if (!_enabled) return;

        try
        {
            var payload = new
            {
                @event = "$identify",
                properties = traits ?? new Dictionary<string, object>(),
                distinct_id = _distinctId,
                timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.PostAsync($"{_endpoint}/capture/", content);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to identify user");
        }
    }

    public void Dispose() { }

    private string GetOrCreateDistinctId()
    {
        var idPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TooltipAI", "analytics_id.txt");

        if (File.Exists(idPath))
            return File.ReadAllText(idPath).Trim();

        var id = Guid.NewGuid().ToString();
        Directory.CreateDirectory(Path.GetDirectoryName(idPath)!);
        File.WriteAllText(idPath, id);
        return id;
    }
}

using System.Text;
using System.Text.Json;

namespace TooltipAI.Backend.Services;

public class LLMProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LLMProvider> _logger;
    private readonly string _apiKey;
    private readonly string _endpoint;

    public LLMProvider(HttpClient httpClient, IConfiguration configuration, ILogger<LLMProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["LLM:ApiKey"] ?? string.Empty;
        _endpoint = configuration["LLM:Endpoint"] ?? "https://api.openai.com/v1/chat/completions";
    }

    public async Task<EnrichmentResult> EnrichAsync(string controlType, string appName, Dictionary<string, string>? properties = null)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("LLM API key not configured, using fallback");
            return new EnrichmentResult
            {
                Summary = $"UI Element: {controlType} in {appName}",
                Source = "fallback"
            };
        }

        var prompt = BuildPrompt(controlType, appName, properties);

        try
        {
            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant that provides concise descriptions of UI elements. Return only the description, no preamble." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 150,
                temperature = 0.3
            };

            var json = JsonSerializer.Serialize(requestBody);

            using var request = new HttpRequestMessage(HttpMethod.Post, _endpoint);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            var summary = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "Unable to generate description";

            return new EnrichmentResult
            {
                Summary = summary.Trim(),
                Source = "llm"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LLM API call failed");
            throw;
        }
    }

    private string BuildPrompt(string controlType, string appName, Dictionary<string, string>? properties)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Describe this UI element concisely in 1-2 sentences:");
        sb.AppendLine($"- Application: {appName}");
        sb.AppendLine($"- Control Type: {controlType}");

        if (properties != null && properties.Count > 0)
        {
            foreach (var prop in properties)
            {
                sb.AppendLine($"- {prop.Key}: {prop.Value}");
            }
        }

        sb.AppendLine("\nProvide a helpful description of what this element does and how to use it.");

        return sb.ToString();
    }
}

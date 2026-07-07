using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace TooltipAI.Core.AI;

/// <summary>
/// Cloud LLM provider (Azure OpenAI / Anthropic).
/// Used for Pro/Business/Enterprise tiers.
/// </summary>
public sealed class CloudLLMProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CloudLLMProvider> _logger;
    private readonly string _apiKey;
    private readonly string _endpoint;
    private readonly string _model;

    public string ProviderName => "Cloud LLM";
    public bool IsAvailable => !string.IsNullOrEmpty(_apiKey);
    public bool IsLocal => false;

    public CloudLLMProvider(HttpClient httpClient, ILogger<CloudLLMProvider> logger, 
        string? apiKey = null, string? endpoint = null, string? model = null)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = apiKey ?? string.Empty;
        _endpoint = endpoint ?? "https://api.openai.com/v1/chat/completions";
        _model = model ?? "gpt-4o-mini";
    }

    public async Task<AIResponse> EnrichContextAsync(AIRequest request)
    {
        var sw = Stopwatch.StartNew();
        
        try
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return new AIResponse
                {
                    Summary = "Cloud LLM not configured",
                    Provider = ProviderName,
                    LatencyMs = sw.Elapsed.TotalMilliseconds,
                    ErrorMessage = "API key not configured",
                    Confidence = 0
                };
            }

            var prompt = BuildPrompt(request);
            var response = await CallLLMApi(prompt);
            
            sw.Stop();
            
            return new AIResponse
            {
                Summary = response,
                Shortcut = GetShortcutForElement(request),
                Tips = GetTipsForElement(request),
                Provider = ProviderName,
                LatencyMs = sw.Elapsed.TotalMilliseconds,
                Confidence = 95
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cloud LLM processing failed");
            sw.Stop();
            
            return new AIResponse
            {
                Summary = $"Error: {ex.Message}",
                Provider = ProviderName,
                LatencyMs = sw.Elapsed.TotalMilliseconds,
                ErrorMessage = ex.Message,
                Confidence = 0
            };
        }
    }

    public Task<bool> IsAvailableAsync()
    {
        return Task.FromResult(IsAvailable);
    }

    public Task<AIHealthStatus> GetHealthAsync()
    {
        return Task.FromResult(new AIHealthStatus
        {
            IsHealthy = IsAvailable,
            Status = IsAvailable ? "API configured" : "API key missing",
            LatencyMs = 0
        });
    }

    private string BuildPrompt(AIRequest request)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are a helpful UI assistant. Describe this UI element concisely and provide useful context.");
        sb.AppendLine();
        sb.AppendLine($"Application: {request.AppName}");
        sb.AppendLine($"Element: {request.ElementName}");
        sb.AppendLine($"Type: {request.ControlType}");
        sb.AppendLine($"State: {request.ElementState}");
        
        if (request.Properties.Any())
        {
            sb.AppendLine("Properties:");
            foreach (var prop in request.Properties)
            {
                sb.AppendLine($"  - {prop.Key}: {prop.Value}");
            }
        }

        if (request.AvailableActions.Any())
        {
            sb.AppendLine($"Available actions: {string.Join(", ", request.AvailableActions)}");
        }

        if (!string.IsNullOrEmpty(request.UserQuery))
        {
            sb.AppendLine($"User question: {request.UserQuery}");
        }

        sb.AppendLine();
        sb.AppendLine("Provide:");
        sb.AppendLine("1. A concise description (1-2 sentences)");
        sb.AppendLine("2. Keyboard shortcut if applicable");
        sb.AppendLine("3. 1-2 helpful tips");

        return sb.ToString();
    }

    private async Task<string> CallLLMApi(string prompt)
    {
        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = "You are a helpful UI assistant. Return concise, useful descriptions." },
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

        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "Unable to generate description";
    }

    private string? GetShortcutForElement(AIRequest request)
    {
        var elementName = request.ElementName.ToLowerInvariant();
        
        return elementName switch
        {
            "save" or "guardar" => "Ctrl+S",
            "copy" or "copiar" => "Ctrl+C",
            "paste" or "pegar" => "Ctrl+V",
            "undo" or "deshacer" => "Ctrl+Z",
            "redo" or "rehacer" => "Ctrl+Y",
            "find" or "buscar" => "Ctrl+F",
            "replace" or "reemplazar" => "Ctrl+H",
            "select all" or "seleccionar todo" => "Ctrl+A",
            _ => null
        };
    }

    private List<string> GetTipsForElement(AIRequest request)
    {
        var tips = new List<string>();
        var appName = request.AppName.ToLowerInvariant();

        if (appName.Contains("excel"))
        {
            tips.Add("Doble clic para editar celda directamente");
            tips.Add("F2 para editar celda seleccionada");
        }
        else if (appName.Contains("chrome"))
        {
            tips.Add("Ctrl+T para nueva pestana");
            tips.Add("Ctrl+Shift+T para reabrir pestana cerrada");
        }
        else if (appName.Contains("code"))
        {
            tips.Add("Ctrl+P para abrir archivo rapido");
            tips.Add("Ctrl+Shift+P para paleta de comandos");
        }

        return tips;
    }
}

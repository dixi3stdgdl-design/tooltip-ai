namespace TooltipAI.Core.AI;

/// <summary>
/// Interface for AI providers (local or cloud).
/// Abstraction layer for Gemini Nano, Azure OpenAI, Ollama, etc.
/// </summary>
public interface IAIProvider
{
    string ProviderName { get; }
    bool IsAvailable { get; }
    bool IsLocal { get; }
    
    Task<AIResponse> EnrichContextAsync(AIRequest request);
    Task<bool> IsAvailableAsync();
    Task<AIHealthStatus> GetHealthAsync();
}

public sealed class AIRequest
{
    public string ControlType { get; init; } = string.Empty;
    public string AppName { get; init; } = string.Empty;
    public string ElementName { get; init; } = string.Empty;
    public string ElementState { get; init; } = string.Empty;
    public Dictionary<string, string> Properties { get; init; } = new();
    public string[] AvailableActions { get; init; } = Array.Empty<string>();
    public string? UserQuery { get; init; }
}

public sealed class AIResponse
{
    public string Summary { get; init; } = string.Empty;
    public string? Shortcut { get; init; }
    public List<string> Tips { get; init; } = new();
    public string Provider { get; init; } = string.Empty;
    public double LatencyMs { get; init; }
    public bool IsFromCache { get; init; }
    public string? ErrorMessage { get; init; }
    public int Confidence { get; init; } = 100;
}

public sealed class AIHealthStatus
{
    public bool IsHealthy { get; init; }
    public string Status { get; init; } = string.Empty;
    public double LatencyMs { get; init; }
    public string? ErrorMessage { get; init; }
}

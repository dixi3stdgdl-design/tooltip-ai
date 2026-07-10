namespace TooltipAI.Core.AI;

/// <summary>
/// Interface for AI providers (local or cloud).
/// Abstraction layer for Gemini Nano, Azure OpenAI, Ollama, etc.
/// </summary>
public interface IAIProvider
{
    /// <summary>
    /// Gets the name of this AI provider (e.g., "GeminiNano", "CloudLLM").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets whether this provider is currently available for requests.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Gets whether this is a local (on-device) AI provider.
    /// </summary>
    bool IsLocal { get; }

    /// <summary>
    /// Enriches the provided context with AI-generated insights.
    /// </summary>
    /// <param name="request">The AI request containing element and context information.</param>
    /// <returns>An AI response with enriched context, tips, and shortcuts.</returns>
    Task<AIResponse> EnrichContextAsync(AIRequest request);

    /// <summary>
    /// Asynchronously checks if the provider is available and ready to handle requests.
    /// </summary>
    /// <returns>True if the provider is ready, false otherwise.</returns>
    Task<bool> IsAvailableAsync();

    /// <summary>
    /// Gets the current health status of this AI provider.
    /// </summary>
    /// <returns>Health status including latency and error information.</returns>
    Task<AIHealthStatus> GetHealthAsync();
}

/// <summary>
/// Represents a request to an AI provider for context enrichment.
/// </summary>
public sealed class AIRequest
{
    /// <summary>The UI control type (e.g., "Button", "TextBox").</summary>
    public string ControlType { get; init; } = string.Empty;

    /// <summary>The application name (e.g., "excel", "chrome").</summary>
    public string AppName { get; init; } = string.Empty;

    /// <summary>The name of the UI element.</summary>
    public string ElementName { get; init; } = string.Empty;

    /// <summary>The current state of the element (e.g., "enabled", "disabled").</summary>
    public string ElementState { get; init; } = string.Empty;

    /// <summary>Additional properties of the element.</summary>
    public Dictionary<string, string> Properties { get; init; } = new();

    /// <summary>List of actions available on this element.</summary>
    public string[] AvailableActions { get; init; } = Array.Empty<string>();

    /// <summary>Optional user query or voice command for gaze interaction.</summary>
    public string? UserQuery { get; init; }
}

/// <summary>
/// Represents the AI provider's response with enriched context.
/// </summary>
public sealed class AIResponse
{
    /// <summary>The enriched summary or description.</summary>
    public string Summary { get; init; } = string.Empty;

    /// <summary>Keyboard shortcut for the element, if available.</summary>
    public string? Shortcut { get; init; }

    /// <summary>List of usage tips for the element.</summary>
    public List<string> Tips { get; init; } = new();

    /// <summary>The provider that generated this response.</summary>
    public string Provider { get; init; } = string.Empty;

    /// <summary>Response latency in milliseconds.</summary>
    public double LatencyMs { get; init; }

    /// <summary>Whether this response came from cache.</summary>
    public bool IsFromCache { get; init; }

    /// <summary>Error message if the request failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Confidence score (0-100) for the response accuracy.</summary>
    public int Confidence { get; init; } = 100;
}

/// <summary>
/// Health status information for an AI provider.
/// </summary>
public sealed class AIHealthStatus
{
    /// <summary>Whether the provider is healthy and operational.</summary>
    public bool IsHealthy { get; init; }

    /// <summary>Human-readable status message.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Latency of the health check in milliseconds.</summary>
    public double LatencyMs { get; init; }

    /// <summary>Error message if the health check failed.</summary>
    public string? ErrorMessage { get; init; }
}

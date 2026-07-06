using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace TooltipAI.Core.AI;

/// <summary>
/// AI Router that selects between local (Gemini Nano) and cloud providers
/// based on user tier and availability.
/// </summary>
public sealed class AIRouter
{
    private readonly GeminiNanoProvider _localProvider;
    private readonly CloudLLMProvider _cloudProvider;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AIRouter> _logger;
    private readonly TimeSpan _cacheTtl = TimeSpan.FromMinutes(5);

    public AIRouter(
        GeminiNanoProvider localProvider,
        CloudLLMProvider cloudProvider,
        IMemoryCache cache,
        ILogger<AIRouter> logger)
    {
        _localProvider = localProvider;
        _cloudProvider = cloudProvider;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Enrich context using the appropriate AI provider based on tier.
    /// </summary>
    public async Task<AIResponse> EnrichContextAsync(AIRequest request, string tier = "free")
    {
        var cacheKey = $"ai:{tier}:{request.AppName}:{request.ControlType}:{request.ElementName}";
        
        // Check cache first
        if (_cache.TryGetValue<AIResponse>(cacheKey, out var cached) && cached != null)
        {
            _logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return cached;
        }

        AIResponse response;

        switch (tier.ToLowerInvariant())
        {
            case "pro":
            case "business":
            case "enterprise":
                // Pro+: Try cloud first, fallback to local
                response = await EnrichWithCloudFirst(request);
                break;
            
            case "free":
            default:
                // Free: Use local only
                response = await EnrichWithLocalOnly(request);
                break;
        }

        // Cache the response
        if (response.Confidence > 0 && string.IsNullOrEmpty(response.ErrorMessage))
        {
            _cache.Set(cacheKey, response, _cacheTtl);
        }

        return response;
    }

    private async Task<AIResponse> EnrichWithLocalOnly(AIRequest request)
    {
        if (_localProvider.IsAvailable)
        {
            return await _localProvider.EnrichContextAsync(request);
        }

        // Fallback to rule-based
        return new AIResponse
        {
            Summary = $"{request.ControlType}: {request.ElementName}",
            Provider = "rules",
            Confidence = 50
        };
    }

    private async Task<AIResponse> EnrichWithCloudFirst(AIRequest request)
    {
        // Try cloud if available
        if (_cloudProvider.IsAvailable)
        {
            try
            {
                var cloudResponse = await _cloudProvider.EnrichContextAsync(request);
                if (cloudResponse.Confidence > 0)
                {
                    return cloudResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cloud LLM failed, falling back to local");
            }
        }

        // Fallback to local
        if (_localProvider.IsAvailable)
        {
            return await _localProvider.EnrichContextAsync(request);
        }

        // Final fallback
        return new AIResponse
        {
            Summary = $"{request.ControlType}: {request.ElementName}",
            Provider = "fallback",
            Confidence = 50
        };
    }

    public async Task<Dictionary<string, AIHealthStatus>> GetHealthAsync()
    {
        var localHealth = await _localProvider.GetHealthAsync();
        var cloudHealth = await _cloudProvider.GetHealthAsync();

        return new Dictionary<string, AIHealthStatus>
        {
            ["local"] = localHealth,
            ["cloud"] = cloudHealth
        };
    }
}

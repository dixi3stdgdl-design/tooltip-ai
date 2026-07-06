using Microsoft.AspNetCore.Mvc;
using TooltipAI.Core.AI;
using TooltipAI.Core.Translate;

namespace TooltipAI.Backend.Controllers;

[ApiController]
[Route("api/health")]
public class HealthCheckController : ControllerBase
{
    private readonly GeminiNanoProvider _geminiProvider;
    private readonly AIRouter _aiRouter;
    private readonly Translator _translator;
    private readonly LanguageDetector _langDetector;
    private readonly ILogger<HealthCheckController> _logger;

    public HealthCheckController(
        GeminiNanoProvider geminiProvider,
        AIRouter aiRouter,
        Translator translator,
        LanguageDetector langDetector,
        ILogger<HealthCheckController> logger)
    {
        _geminiProvider = geminiProvider;
        _aiRouter = aiRouter;
        _translator = translator;
        _langDetector = langDetector;
        _logger = logger;
    }

    /// <summary>
    /// Basic health check
    /// </summary>
    [HttpGet]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            service = "tooltipai-backend",
            version = "1.0.0",
            timestamp = DateTime.UtcNow,
            uptime = GetUptime()
        });
    }

    /// <summary>
    /// Detailed Gemini Nano health check
    /// </summary>
    [HttpGet("gemini")]
    public async Task<IActionResult> GeminiHealth()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var health = await _geminiProvider.GetHealthAsync();
        var isAvailable = await _geminiProvider.IsAvailableAsync();

        // Test inference
        AIResponse? testResult = null;
        string? testError = null;
        double inferenceLatencyMs = 0;

        try
        {
            var testRequest = new AIRequest
            {
                ControlType = "Button",
                AppName = "Test",
                ElementName = "Save",
                ElementState = "Enabled"
            };

            var inferenceSw = System.Diagnostics.Stopwatch.StartNew();
            testResult = await _geminiProvider.EnrichContextAsync(testRequest);
            inferenceSw.Stop();
            inferenceLatencyMs = inferenceSw.ElapsedMilliseconds;
        }
        catch (Exception ex)
        {
            testError = ex.Message;
        }

        sw.Stop();

        return Ok(new
        {
            status = isAvailable ? "healthy" : "degraded",
            provider = "Gemini Nano",
            model = new
            {
                available = isAvailable,
                path = GetModelPath(),
                status = health.Status,
                version = "v2"
            },
            inference = new
            {
                testPassed = testResult != null && string.IsNullOrEmpty(testError),
                latencyMs = inferenceLatencyMs,
                sampleResponse = testResult?.Summary,
                error = testError
            },
            capabilities = new
            {
                contextEnrichment = true,
                translation = true,
                conversation = true,
                offline = true,
                costPerQuery = "$0.00"
            },
            timestamp = DateTime.UtcNow,
            checkDurationMs = sw.ElapsedMilliseconds
        });
    }

    /// <summary>
    /// Translation health check
    /// </summary>
    [HttpGet("translate")]
    public async Task<IActionResult> TranslateHealth()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var supportedLanguages = _langDetector.GetSupportedLanguages();

        // Test translation
        TranslationResult? testResult = null;
        string? testError = null;

        try
        {
            testResult = await _translator.TranslateAsync("Hello world", "en", "es");
        }
        catch (Exception ex)
        {
            testError = ex.Message;
        }

        sw.Stop();

        return Ok(new
        {
            status = testResult != null && string.IsNullOrEmpty(testError) ? "healthy" : "degraded",
            provider = "Gemini Nano (local)",
            languages = new
            {
                supported = supportedLanguages.Count,
                list = supportedLanguages.Select(l => new { code = l.Code, name = l.Name })
            },
            translation = new
            {
                testPassed = testResult != null && string.IsNullOrEmpty(testError),
                sampleInput = "Hello world",
                sampleOutput = testResult?.TranslatedText,
                provider = testResult?.Provider.ToString(),
                latencyMs = testResult?.LatencyMs ?? 0,
                error = testError
            },
            cost = "$0.00 (all local)",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// AI Router health check (shows all providers)
    /// </summary>
    [HttpGet("ai")]
    public async Task<IActionResult> AIHealth()
    {
        var providerHealth = await _aiRouter.GetHealthAsync();

        return Ok(new
        {
            status = providerHealth.All(h => h.Value.IsHealthy) ? "healthy" : "degraded",
            providers = providerHealth.Select(p => new
            {
                name = p.Key,
                healthy = p.Value.IsHealthy,
                status = p.Value.Status,
                latencyMs = p.Value.LatencyMs,
                error = p.Value.ErrorMessage
            }),
            routing = new
            {
                free = "Gemini Nano (local)",
                pro = "Cloud LLM",
                business = "Cloud LLM Dedicated",
                enterprise = "Custom"
            },
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Full system health check
    /// </summary>
    [HttpGet("full")]
    public async Task<IActionResult> FullHealth()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var geminiHealth = await _geminiProvider.GetHealthAsync();
        var aiHealth = await _aiRouter.GetHealthAsync();
        var translateHealth = await _translator.TranslateAsync("test", "en", "es");

        sw.Stop();

        return Ok(new
        {
            status = "healthy",
            components = new
            {
                backend = new { status = "healthy", version = "1.0.0" },
                geminiNano = new { status = geminiHealth.IsHealthy ? "healthy" : "degraded", available = _geminiProvider.IsAvailable },
                aiRouter = new { status = aiHealth.Values.All(h => h.IsHealthy) ? "healthy" : "degraded" },
                translate = new { status = translateHealth != null ? "healthy" : "degraded" }
            },
            metrics = new
            {
                uptime = GetUptime(),
                memoryUsedMb = GC.GetTotalMemory(false) / 1024 / 1024,
                gcCollections = GC.CollectionCount(0) + GC.CollectionCount(1) + GC.CollectionCount(2)
            },
            timestamp = DateTime.UtcNow,
            checkDurationMs = sw.ElapsedMilliseconds
        });
    }

    /// <summary>
    /// Readiness probe for container orchestration
    /// </summary>
    [HttpGet("ready")]
    public IActionResult Ready()
    {
        return Ok(new { status = "ready" });
    }

    /// <summary>
    /// Liveness probe for container orchestration
    /// </summary>
    [HttpGet("live")]
    public IActionResult Live()
    {
        return Ok(new { status = "alive" });
    }

    private string GetModelPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TooltipAI", "models", "gemini-nano-1b");
    }

    private string GetUptime()
    {
        var uptime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime;
        return $"{(int)uptime.TotalHours}h {uptime.Minutes}m";
    }
}

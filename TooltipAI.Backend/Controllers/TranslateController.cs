using Microsoft.AspNetCore.Mvc;
using TooltipAI.Core.Translate;

namespace TooltipAI.Backend.Controllers;

[ApiController]
[Route("api/translate")]
public class TranslateController : ControllerBase
{
    private readonly Translator _translator;
    private readonly LanguageDetector _langDetector;
    private readonly ConversationMode _conversationMode;
    private readonly ILogger<TranslateController> _logger;

    public TranslateController(
        Translator translator,
        LanguageDetector langDetector,
        ConversationMode conversationMode,
        ILogger<TranslateController> logger)
    {
        _translator = translator;
        _langDetector = langDetector;
        _conversationMode = conversationMode;
        _logger = logger;
    }

    /// <summary>
    /// Translate text
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Translate([FromBody] TranslateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var startTime = DateTime.UtcNow;

        var result = await _translator.TranslateAsync(
            request.Text,
            request.SourceLanguage ?? "auto",
            request.TargetLanguage,
            request.Context != null ? new TranslationContext
            {
                Domain = request.Context.Domain,
                Formality = request.Context.Formality,
                Audience = request.Context.Audience
            } : null);

        var latencyMs = (DateTime.UtcNow - startTime).TotalMilliseconds;

        _logger.LogInformation("Translation: {Source} -> {Target} via {Provider} ({Latency}ms)",
            request.SourceLanguage, request.TargetLanguage, result.Provider, latencyMs);

        return Ok(new TranslateResponse
        {
            TranslatedText = result.TranslatedText,
            SourceLanguage = result.SourceLanguage,
            TargetLanguage = result.TargetLanguage,
            Alternatives = result.Alternatives,
            CulturalNote = result.CulturalNote,
            Provider = result.Provider.ToString(),
            LatencyMs = result.LatencyMs
        });
    }

    /// <summary>
    /// Detect language of text
    /// </summary>
    [HttpPost("detect")]
    public IActionResult DetectLanguage([FromBody] DetectRequest request)
    {
        if (string.IsNullOrEmpty(request.Text))
            return BadRequest("Text is required");

        var result = _langDetector.DetectLanguage(request.Text);

        return Ok(new DetectResponse
        {
            LanguageCode = result.Code,
            LanguageName = result.Name,
            Confidence = result.Confidence
        });
    }

    /// <summary>
    /// Get supported languages
    /// </summary>
    [HttpGet("languages")]
    public IActionResult GetLanguages()
    {
        var languages = _langDetector.GetSupportedLanguages();
        return Ok(languages);
    }

    /// <summary>
    /// Ask question about text (conversation mode)
    /// </summary>
    [HttpPost("ask")]
    public async Task<IActionResult> AskQuestion([FromBody] AskRequest request)
    {
        if (string.IsNullOrEmpty(request.Question))
            return BadRequest("Question is required");

        if (string.IsNullOrEmpty(request.Context))
            return BadRequest("Context is required");

        var result = await _conversationMode.AskAsync(request.Question, request.Context);

        return Ok(new AskResponse
        {
            Answer = result.Answer,
            Language = result.Language,
            Provider = result.Provider,
            LatencyMs = result.LatencyMs
        });
    }

    /// <summary>
    /// Get conversation history
    /// </summary>
    [HttpGet("history")]
    public IActionResult GetHistory()
    {
        var history = _conversationMode.GetHistory();
        return Ok(history);
    }

    /// <summary>
    /// Clear conversation history
    /// </summary>
    [HttpDelete("history")]
    public IActionResult ClearHistory()
    {
        _conversationMode.ClearHistory();
        return Ok(new { message = "History cleared" });
    }

    /// <summary>
    /// Health check
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            provider = "Gemini Nano (local)",
            cost = "$0",
            languages = 10,
            timestamp = DateTime.UtcNow
        });
    }
}

// Request/Response models

public sealed class TranslateRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    public string Text { get; init; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    public string TargetLanguage { get; init; } = string.Empty;

    public string? SourceLanguage { get; init; }
    public TranslationContextRequest? Context { get; init; }
}

public sealed class TranslationContextRequest
{
    public string? Domain { get; init; }
    public string? Formality { get; init; }
    public string? Audience { get; init; }
}

public sealed class TranslateResponse
{
    public string TranslatedText { get; init; } = string.Empty;
    public string SourceLanguage { get; init; } = string.Empty;
    public string TargetLanguage { get; init; } = string.Empty;
    public List<string> Alternatives { get; init; } = new();
    public string? CulturalNote { get; init; }
    public string Provider { get; init; } = string.Empty;
    public double LatencyMs { get; init; }
}

public sealed class DetectRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    public string Text { get; init; } = string.Empty;
}

public sealed class DetectResponse
{
    public string LanguageCode { get; init; } = string.Empty;
    public string LanguageName { get; init; } = string.Empty;
    public float Confidence { get; init; }
}

public sealed class AskRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    public string Question { get; init; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    public string Context { get; init; } = string.Empty;
}

public sealed class AskResponse
{
    public string Answer { get; init; } = string.Empty;
    public string Language { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public double LatencyMs { get; init; }
}

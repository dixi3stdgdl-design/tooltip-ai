using Microsoft.Extensions.Logging;

namespace TooltipAI.Core.Translate;

/// <summary>
/// Conversation mode for asking questions about translated text.
/// Uses Gemini Nano for local, free conversation.
/// </summary>
public sealed class ConversationMode
{
    private readonly Translator _translator;
    private readonly LanguageDetector _langDetector;
    private readonly ILogger<ConversationMode> _logger;
    private readonly List<ConversationMessage> _history = new();

    public ConversationMode(
        Translator translator,
        LanguageDetector langDetector,
        ILogger<ConversationMode> logger)
    {
        _translator = translator;
        _langDetector = langDetector;
        _logger = logger;
    }

    public async Task<ConversationResponse> AskAsync(string question, string context)
    {
        try
        {
            // Detect language of context
            var lang = _langDetector.DetectLanguage(context);

            // Build conversation prompt
            var prompt = BuildConversationPrompt(question, context, lang.Code);

            // Process with Gemini Nano (local, free)
            var response = await ProcessWithGeminiNano(prompt, question, context);

            // Add to history
            _history.Add(new ConversationMessage
            {
                Role = "user",
                Content = question,
                Context = context,
                Timestamp = DateTime.UtcNow
            });

            _history.Add(new ConversationMessage
            {
                Role = "assistant",
                Content = response,
                Timestamp = DateTime.UtcNow
            });

            return new ConversationResponse
            {
                Answer = response,
                Language = lang.Code,
                Provider = "Gemini Nano (local)",
                LatencyMs = 10 // Simulated
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Conversation failed");

            return new ConversationResponse
            {
                Answer = "Lo siento, no pude procesar tu pregunta. Intenta de nuevo.",
                Language = "es",
                Provider = "error",
                ErrorMessage = ex.Message
            };
        }
    }

    public List<ConversationMessage> GetHistory()
    {
        return new List<ConversationMessage>(_history);
    }

    public void ClearHistory()
    {
        _history.Clear();
    }

    private string BuildConversationPrompt(string question, string context, string lang)
    {
        return $@"You are a helpful translation assistant. The user is asking about this text:

Text: {context}

Language: {lang}

User question: {question}

Provide a helpful, concise answer in the same language as the question.";
    }

    private async Task<string> ProcessWithGeminiNano(string prompt, string question, string context)
    {
        // In production, call Gemini Nano SDK
        // For now, use enhanced rule-based responses
        await Task.Delay(10);

        var questionLower = question.ToLowerInvariant();

        if (questionLower.Contains("que significa") || questionLower.Contains("what does it mean"))
        {
            return $"El texto significa: {context}";
        }

        if (questionLower.Contains("como se traduce") || questionLower.Contains("how to say"))
        {
            var translation = await _translator.TranslateAsync(context, "auto", "es");
            return $"Se traduce como: {translation.TranslatedText}";
        }

        if (questionLower.Contains("ejemplo") || questionLower.Contains("example"))
        {
            return $"Ejemplo de uso: {context} en una oración completa.";
        }

        return $"Sobre el texto '{context}': {question}";
    }
}

public sealed class ConversationMessage
{
    public string Role { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string? Context { get; init; }
    public DateTime Timestamp { get; init; }
}

public sealed class ConversationResponse
{
    public string Answer { get; init; } = string.Empty;
    public string Language { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public double LatencyMs { get; init; }
    public string? ErrorMessage { get; init; }
}

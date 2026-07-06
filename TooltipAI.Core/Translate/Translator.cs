using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace TooltipAI.Core.Translate;

/// <summary>
/// Translates text using Gemini Nano (local) or Cloud LLM.
/// Gemini Nano is primary - zero cost for all translations.
/// </summary>
public sealed class Translator
{
    private readonly ILogger<Translator> _logger;
    private readonly Dictionary<string, string> _localDictionary;
    private bool _geminiNanoAvailable;

    public Translator(ILogger<Translator> logger)
    {
        _logger = logger;
        _localDictionary = InitializeDictionary();
        _geminiNanoAvailable = CheckGeminiNanoAvailability();
    }

    public async Task<TranslationResult> TranslateAsync(
        string text,
        string sourceLang,
        string targetLang,
        TranslationContext? context = null)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            // Priority 1: Gemini Nano (local, free)
            if (_geminiNanoAvailable)
            {
                var (translated, alternatives, cultural) = await TranslateWithGeminiNano(text, sourceLang, targetLang, context);
                sw.Stop();
                return CreateResult(translated, sourceLang, targetLang, alternatives, cultural, TranslationProvider.GeminiNano, sw.ElapsedMilliseconds);
            }

            // Priority 2: Local dictionary (free)
            var dictResult = TranslateWithDictionary(text, sourceLang, targetLang);
            if (dictResult != null)
            {
                sw.Stop();
                return CreateResult(dictResult.Value.translated, sourceLang, targetLang, dictResult.Value.alternatives, null, TranslationProvider.LocalDictionary, sw.ElapsedMilliseconds);
            }

            // Priority 3: Cloud LLM (fallback, paid)
            var (cloudTranslated, cloudAlternatives, cloudCultural) = await TranslateWithCloudLLM(text, sourceLang, targetLang, context);
            sw.Stop();
            return CreateResult(cloudTranslated, sourceLang, targetLang, cloudAlternatives, cloudCultural, TranslationProvider.CloudLLM, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Translation failed");
            sw.Stop();

            return new TranslationResult
            {
                TranslatedText = text,
                SourceLanguage = sourceLang,
                TargetLanguage = targetLang,
                Provider = TranslationProvider.Error,
                LatencyMs = sw.ElapsedMilliseconds,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task<(string translated, List<string> alternatives, string? cultural)> TranslateWithGeminiNano(
        string text, string sourceLang, string targetLang, TranslationContext? context)
    {
        // Build prompt for Gemini Nano
        var prompt = BuildTranslationPrompt(text, sourceLang, targetLang, context);

        // Simulate Gemini Nano processing
        // In production, call actual Gemini Nano SDK
        await Task.Delay(10); // Simulate minimal latency

        // Enhanced local translation
        var translatedText = await ProcessWithGeminiNano(prompt, text, sourceLang, targetLang);
        var alternatives = await GetAlternatives(text, sourceLang, targetLang);
        var cultural = await GetCulturalNote(text, sourceLang, targetLang);

        return (translatedText, alternatives, cultural);
    }

    private (string translated, List<string> alternatives)? TranslateWithDictionary(string text, string sourceLang, string targetLang)
    {
        var key = $"{sourceLang}:{targetLang}:{text.ToLowerInvariant()}";

        if (_localDictionary.TryGetValue(key, out var translated))
        {
            return (translated, new List<string>());
        }

        return null;
    }

    private async Task<(string translated, List<string> alternatives, string? cultural)> TranslateWithCloudLLM(
        string text, string sourceLang, string targetLang, TranslationContext? context)
    {
        // Cloud LLM fallback
        await Task.Delay(50);

        return ($"[Cloud] {text}", new List<string>(), null);
    }

    private TranslationResult CreateResult(
        string translated, string sourceLang, string targetLang,
        List<string> alternatives, string? cultural,
        TranslationProvider provider, double latencyMs)
    {
        return new TranslationResult
        {
            TranslatedText = translated,
            SourceLanguage = sourceLang,
            TargetLanguage = targetLang,
            Alternatives = alternatives ?? new List<string>(),
            CulturalNote = cultural,
            Provider = provider,
            LatencyMs = latencyMs
        };
    }

    private string BuildTranslationPrompt(string text, string sourceLang, string targetLang, TranslationContext? context)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Translate the following text from {GetLanguageName(sourceLang)} to {GetLanguageName(targetLang)}:");
        sb.AppendLine($"Text: {text}");

        if (context != null)
        {
            if (!string.IsNullOrEmpty(context.Domain))
                sb.AppendLine($"Domain: {context.Domain}");
            if (!string.IsNullOrEmpty(context.Formality))
                sb.AppendLine($"Formality: {context.Formality}");
        }

        sb.AppendLine("Provide:");
        sb.AppendLine("1. The translation");
        sb.AppendLine("2. Alternative translations (2-3 options)");
        sb.AppendLine("3. Cultural notes if applicable");

        return sb.ToString();
    }

    private async Task<string> ProcessWithGeminiNano(string prompt, string text, string sourceLang, string targetLang)
    {
        // In production, call Gemini Nano SDK
        // For now, use enhanced local translation
        await Task.Delay(5);

        // Try dictionary first
        var dictResult = TranslateWithDictionary(text, sourceLang, targetLang);
        if (dictResult != null)
            return dictResult.Value.translated;

        // Fallback to simple word-by-word (for demo)
        return $"[{GetLanguageName(targetLang)}] {text}";
    }

    private async Task<List<string>> GetAlternatives(string text, string sourceLang, string targetLang)
    {
        await Task.Delay(5);

        return new List<string>
        {
            $"Alternative 1: {text}",
            $"Alternative 2: {text}"
        };
    }

    private async Task<string?> GetCulturalNote(string text, string sourceLang, string targetLang)
    {
        await Task.Delay(5);

        if (sourceLang == "en" && targetLang == "es")
        {
            return "En contextos formales en español, se prefiere 'usted' sobre 'tú'.";
        }

        return null;
    }

    private string GetLanguageName(string code)
    {
        return code switch
        {
            "en" => "English",
            "es" => "Spanish",
            "fr" => "French",
            "de" => "German",
            "pt" => "Portuguese",
            "it" => "Italian",
            "ja" => "Japanese",
            "zh" => "Chinese",
            "ko" => "Korean",
            "ar" => "Arabic",
            _ => code
        };
    }

    private bool CheckGeminiNanoAvailability()
    {
        try
        {
            var modelPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "TooltipAI", "models", "gemini-nano");
            return Directory.Exists(modelPath) || File.Exists(Path.Combine(modelPath, "model.bin"));
        }
        catch
        {
            return false;
        }
    }

    private Dictionary<string, string> InitializeDictionary()
    {
        // Common translations dictionary
        return new Dictionary<string, string>
        {
            // English -> Spanish
            ["en:es:hello"] = "hola",
            ["en:es:goodbye"] = "adiós",
            ["en:es:thank you"] = "gracias",
            ["en:es:please"] = "por favor",
            ["en:es:yes"] = "sí",
            ["en:es:no"] = "no",
            ["en:es:good morning"] = "buenos días",
            ["en:es:good night"] = "buenas noches",
            ["en:es:how are you"] = "cómo estás",
            ["en:es:i love you"] = "te quiero",

            // Spanish -> English
            ["es:en:hola"] = "hello",
            ["es:en:adiós"] = "goodbye",
            ["es:en:gracias"] = "thank you",
            ["es:en:por favor"] = "please",
            ["es:en:sí"] = "yes",
            ["es:en:no"] = "no",
            ["es:en:buenos días"] = "good morning",
            ["es:en:buenas noches"] = "good night",
            ["es:en:cómo estás"] = "how are you",
            ["es:en:te quiero"] = "i love you",

            // English -> French
            ["en:fr:hello"] = "bonjour",
            ["en:fr:goodbye"] = "au revoir",
            ["en:fr:thank you"] = "merci",
            ["en:fr:please"] = "s'il vous plaît",
            ["en:fr:yes"] = "oui",
            ["en:fr:no"] = "non",

            // English -> German
            ["en:de:hello"] = "hallo",
            ["en:de:goodbye"] = "auf wiedersehen",
            ["en:de:thank you"] = "danke",
            ["en:de:please"] = "bitte",
            ["en:de:yes"] = "ja",
            ["en:de:no"] = "nein",
        };
    }
}

public sealed class TranslationResult
{
    public string TranslatedText { get; init; } = string.Empty;
    public string SourceLanguage { get; init; } = string.Empty;
    public string TargetLanguage { get; init; } = string.Empty;
    public List<string> Alternatives { get; init; } = new();
    public string? CulturalNote { get; init; }
    public TranslationProvider Provider { get; init; }
    public double LatencyMs { get; init; }
    public string? ErrorMessage { get; init; }
}

public sealed class TranslationContext
{
    public string? Domain { get; init; }
    public string? Formality { get; init; }
    public string? Audience { get; init; }
}

public enum TranslationProvider
{
    LocalDictionary,
    GeminiNano,
    CloudLLM,
    Error
}

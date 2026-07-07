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
        return new Dictionary<string, string>
        {
            // ===== EN <-> ES (30 entries) =====
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
            ["en:es:welcome"] = "bienvenido",
            ["en:es:help"] = "ayuda",
            ["en:es:save"] = "guardar",
            ["en:es:cancel"] = "cancelar",
            ["en:es:delete"] = "eliminar",
            ["en:es:copy"] = "copiar",
            ["en:es:paste"] = "pegar",
            ["en:es:undo"] = "deshacer",
            ["en:es:redo"] = "rehacer",
            ["en:es:search"] = "buscar",
            ["en:es:settings"] = "configuración",
            ["en:es:file"] = "archivo",
            ["en:es:edit"] = "editar",
            ["en:es:view"] = "ver",
            ["en:es:close"] = "cerrar",
            ["en:es:open"] = "abrir",
            ["en:es:new"] = "nuevo",
            ["en:es:print"] = "imprimir",
            ["en:es:export"] = "exportar",
            ["en:es:import"] = "importar",

            // ES -> EN
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
            ["es:en:bienvenido"] = "welcome",
            ["es:en:ayuda"] = "help",
            ["es:en:guardar"] = "save",
            ["es:en:cancelar"] = "cancel",
            ["es:en:eliminar"] = "delete",
            ["es:en:copiar"] = "copy",
            ["es:en:pegar"] = "paste",
            ["es:en:deshacer"] = "undo",
            ["es:en:rehacer"] = "redo",
            ["es:en:buscar"] = "search",
            ["es:en:configuración"] = "settings",
            ["es:en:archivo"] = "file",
            ["es:en:editar"] = "edit",
            ["es:en:ver"] = "view",
            ["es:en:cerrar"] = "close",
            ["es:en:abrir"] = "open",
            ["es:en:nuevo"] = "new",
            ["es:en:imprimir"] = "print",
            ["es:en:exportar"] = "export",
            ["es:en:importar"] = "import",

            // EN <-> FR
            ["en:fr:hello"] = "bonjour",
            ["en:fr:goodbye"] = "au revoir",
            ["en:fr:thank you"] = "merci",
            ["en:fr:please"] = "s'il vous plaît",
            ["en:fr:yes"] = "oui",
            ["en:fr:no"] = "non",
            ["en:fr:welcome"] = "bienvenue",
            ["en:fr:help"] = "aide",
            ["en:fr:save"] = "enregistrer",
            ["en:fr:cancel"] = "annuler",
            ["en:fr:delete"] = "supprimer",
            ["en:fr:copy"] = "copier",
            ["en:fr:paste"] = "coller",
            ["en:fr:search"] = "rechercher",
            ["en:fr:settings"] = "paramètres",
            ["en:fr:close"] = "fermer",
            ["en:fr:open"] = "ouvrir",

            // EN <-> DE
            ["en:de:hello"] = "hallo",
            ["en:de:goodbye"] = "auf wiedersehen",
            ["en:de:thank you"] = "danke",
            ["en:de:please"] = "bitte",
            ["en:de:yes"] = "ja",
            ["en:de:no"] = "nein",
            ["en:de:welcome"] = "willkommen",
            ["en:de:help"] = "hilfe",
            ["en:de:save"] = "speichern",
            ["en:de:cancel"] = "abbrechen",
            ["en:de:delete"] = "löschen",
            ["en:de:copy"] = "kopieren",
            ["en:de:paste"] = "einfügen",
            ["en:de:search"] = "suchen",
            ["en:de:settings"] = "einstellungen",
            ["en:de:close"] = "schließen",
            ["en:de:open"] = "öffnen",

            // EN <-> PT
            ["en:pt:hello"] = "olá",
            ["en:pt:goodbye"] = "adeus",
            ["en:pt:thank you"] = "obrigado",
            ["en:pt:please"] = "por favor",
            ["en:pt:yes"] = "sim",
            ["en:pt:no"] = "não",
            ["en:pt:welcome"] = "bem-vindo",
            ["en:pt:help"] = "ajuda",
            ["en:pt:save"] = "salvar",
            ["en:pt:cancel"] = "cancelar",
            ["en:pt:delete"] = "excluir",
            ["en:pt:copy"] = "copiar",
            ["en:pt:paste"] = "colar",
            ["en:pt:search"] = "pesquisar",
            ["en:pt:settings"] = "configurações",
            ["en:pt:close"] = "fechar",
            ["en:pt:open"] = "abrir",

            // EN <-> IT
            ["en:it:hello"] = "ciao",
            ["en:it:goodbye"] = "arrivederci",
            ["en:it:thank you"] = "grazie",
            ["en:it:please"] = "per favore",
            ["en:it:yes"] = "sì",
            ["en:it:no"] = "no",
            ["en:it:welcome"] = "benvenuto",
            ["en:it:help"] = "aiuto",
            ["en:it:save"] = "salva",
            ["en:it:cancel"] = "annulla",
            ["en:it:delete"] = "elimina",
            ["en:it:copy"] = "copia",
            ["en:it:paste"] = "incolla",
            ["en:it:search"] = "cerca",
            ["en:it:settings"] = "impostazioni",
            ["en:it:close"] = "chiudi",
            ["en:it:open"] = "apri",

            // EN <-> JA
            ["en:ja:hello"] = "こんにちは",
            ["en:ja:goodbye"] = "さようなら",
            ["en:ja:thank you"] = "ありがとう",
            ["en:ja:please"] = "お願いします",
            ["en:ja:yes"] = "はい",
            ["en:ja:no"] = "いいえ",
            ["en:ja:welcome"] = "ようこそ",
            ["en:ja:help"] = "ヘルプ",
            ["en:ja:save"] = "保存",
            ["en:ja:cancel"] = "キャンセル",
            ["en:ja:delete"] = "削除",
            ["en:ja:copy"] = "コピー",
            ["en:ja:paste"] = "貼り付け",
            ["en:ja:search"] = "検索",
            ["en:ja:settings"] = "設定",
            ["en:ja:close"] = "閉じる",
            ["en:ja:open"] = "開く",

            // EN <-> ZH
            ["en:zh:hello"] = "你好",
            ["en:zh:goodbye"] = "再见",
            ["en:zh:thank you"] = "谢谢",
            ["en:zh:please"] = "请",
            ["en:zh:yes"] = "是",
            ["en:zh:no"] = "不",
            ["en:zh:welcome"] = "欢迎",
            ["en:zh:help"] = "帮助",
            ["en:zh:save"] = "保存",
            ["en:zh:cancel"] = "取消",
            ["en:zh:delete"] = "删除",
            ["en:zh:copy"] = "复制",
            ["en:zh:paste"] = "粘贴",
            ["en:zh:search"] = "搜索",
            ["en:zh:settings"] = "设置",
            ["en:zh:close"] = "关闭",
            ["en:zh:open"] = "打开",

            // EN <-> KO
            ["en:ko:hello"] = "안녕하세요",
            ["en:ko:goodbye"] = "안녕히 가세요",
            ["en:ko:thank you"] = "감사합니다",
            ["en:ko:please"] = "부탁합니다",
            ["en:ko:yes"] = "네",
            ["en:ko:no"] = "아니요",
            ["en:ko:welcome"] = "환영합니다",
            ["en:ko:help"] = "도움말",
            ["en:ko:save"] = "저장",
            ["en:ko:cancel"] = "취소",
            ["en:ko:delete"] = "삭제",
            ["en:ko:copy"] = "복사",
            ["en:ko:paste"] = "붙여넣기",
            ["en:ko:search"] = "검색",
            ["en:ko:settings"] = "설정",
            ["en:ko:close"] = "닫기",
            ["en:ko:open"] = "열기",

            // EN <-> AR
            ["en:ar:hello"] = "مرحبا",
            ["en:ar:goodbye"] = "وداعا",
            ["en:ar:thank you"] = "شكرا",
            ["en:ar:please"] = "من فضلك",
            ["en:ar:yes"] = "نعم",
            ["en:ar:no"] = "لا",
            ["en:ar:welcome"] = "أهلا وسهلا",
            ["en:ar:help"] = "مساعدة",
            ["en:ar:save"] = "حفظ",
            ["en:ar:cancel"] = "إلغاء",
            ["en:ar:delete"] = "حذف",
            ["en:ar:copy"] = "نسخ",
            ["en:ar:paste"] = "لصق",
            ["en:ar:search"] = "بحث",
            ["en:ar:settings"] = "إعدادات",
            ["en:ar:close"] = "إغلاق",
            ["en:ar:open"] = "فتح",
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

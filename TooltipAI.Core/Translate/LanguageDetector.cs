using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace TooltipAI.Core.Translate;

/// <summary>
/// Detects language of text using character frequency analysis and pattern matching.
/// </summary>
public sealed class LanguageDetector
{
    private readonly Dictionary<string, LanguageProfile> _profiles;
    private readonly ILogger<LanguageDetector> _logger;

    public LanguageDetector(ILogger<LanguageDetector> logger)
    {
        _logger = logger;
        _profiles = InitializeProfiles();
    }

    public LanguageInfo DetectLanguage(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new LanguageInfo { Code = "unknown", Name = "Unknown", Confidence = 0 };

        var scores = new Dictionary<string, float>();

        foreach (var profile in _profiles)
        {
            scores[profile.Key] = CalculateScore(text, profile.Value);
        }

        var bestMatch = scores.OrderByDescending(x => x.Value).First();
        var confidence = Math.Min(bestMatch.Value / 100f, 1.0f);

        return new LanguageInfo
        {
            Code = bestMatch.Key,
            Name = _profiles[bestMatch.Key].Name,
            Confidence = confidence
        };
    }

    public List<LanguageInfo> GetSupportedLanguages()
    {
        return _profiles.Select(p => new LanguageInfo
        {
            Code = p.Key,
            Name = p.Value.Name,
            Confidence = 1.0f
        }).ToList();
    }

    private float CalculateScore(string text, LanguageProfile profile)
    {
        float score = 0;
        var lowerText = text.ToLowerInvariant();

        // Check for unique characters
        foreach (var ch in profile.UniqueChars)
        {
            if (lowerText.Contains(ch))
                score += 20;
        }

        // Check for common words
        foreach (var word in profile.CommonWords)
        {
            if (lowerText.Contains(word))
                score += 10;
        }

        // Check character frequency
        var charFreq = CalculateCharFrequency(lowerText);
        foreach (var freq in charFreq)
        {
            if (profile.CharFrequency.TryGetValue(freq.Key, out var expected))
            {
                var diff = Math.Abs(freq.Value - expected);
                score += Math.Max(0, 10 - diff * 100);
            }
        }

        return score;
    }

    private Dictionary<char, float> CalculateCharFrequency(string text)
    {
        var freq = new Dictionary<char, float>();
        var total = text.Length;

        foreach (var ch in text)
        {
            if (char.IsLetter(ch))
            {
                var lower = char.ToLower(ch);
                freq[lower] = freq.GetValueOrDefault(lower, 0) + 1f / total;
            }
        }

        return freq;
    }

    private Dictionary<string, LanguageProfile> InitializeProfiles()
    {
        return new Dictionary<string, LanguageProfile>
        {
            ["en"] = new LanguageProfile
            {
                Name = "English",
                UniqueChars = new[] { "th", "wh", "sh", "ck" },
                CommonWords = new[] { "the", "and", "is", "in", "to", "of", "a", "that", "it", "for" },
                CharFrequency = new Dictionary<char, float>
                {
                    ['e'] = 0.127f, ['t'] = 0.091f, ['a'] = 0.082f, ['o'] = 0.075f, ['i'] = 0.070f
                }
            },
            ["es"] = new LanguageProfile
            {
                Name = "Spanish",
                UniqueChars = new[] { "ñ", "¿", "¡", "á", "é", "í", "ó", "ú" },
                CommonWords = new[] { "el", "la", "de", "en", "que", "los", "las", "del", "por", "con" },
                CharFrequency = new Dictionary<char, float>
                {
                    ['e'] = 0.137f, ['a'] = 0.125f, ['o'] = 0.087f, ['s'] = 0.079f, ['r'] = 0.068f
                }
            },
            ["fr"] = new LanguageProfile
            {
                Name = "French",
                UniqueChars = new[] { "è", "ê", "ë", "à", "â", "ô", "û", "ù", "î", "ï" },
                CommonWords = new[] { "le", "la", "de", "et", "les", "des", "un", "une", "est", "en" },
                CharFrequency = new Dictionary<char, float>
                {
                    ['e'] = 0.147f, ['a'] = 0.081f, ['s'] = 0.079f, ['i'] = 0.073f, ['t'] = 0.071f
                }
            },
            ["de"] = new LanguageProfile
            {
                Name = "German",
                UniqueChars = new[] { "ä", "ö", "ü", "ß" },
                CommonWords = new[] { "der", "die", "und", "den", "von", "ist", "das", "des", "ein", "eine" },
                CharFrequency = new Dictionary<char, float>
                {
                    ['e'] = 0.170f, ['n'] = 0.098f, ['i'] = 0.076f, ['s'] = 0.072f, ['r'] = 0.070f
                }
            },
            ["pt"] = new LanguageProfile
            {
                Name = "Portuguese",
                UniqueChars = new[] { "ã", "õ", "á", "é", "í", "ó", "ú", "ê", "ô" },
                CommonWords = new[] { "de", "que", "do", "da", "em", "um", "para", "com", "uma", "os" },
                CharFrequency = new Dictionary<char, float>
                {
                    ['a'] = 0.146f, ['e'] = 0.126f, ['o'] = 0.104f, ['s'] = 0.078f, ['r'] = 0.065f
                }
            },
            ["it"] = new LanguageProfile
            {
                Name = "Italian",
                UniqueChars = new[] { "à", "è", "é", "ì", "ò", "ù" },
                CommonWords = new[] { "di", "che", "il", "la", "del", "per", "un", "una", "con", "sono" },
                CharFrequency = new Dictionary<char, float>
                {
                    ['e'] = 0.118f, ['a'] = 0.102f, ['o'] = 0.098f, ['i'] = 0.094f, ['t'] = 0.069f
                }
            },
            ["ja"] = new LanguageProfile
            {
                Name = "Japanese",
                UniqueChars = new[] { "は", "の", "に", "を", "た", "が", "で", "て", "と", "し" },
                CommonWords = new[] { "です", "ます", "した", "して", "ない", "この", "それ", "から", "まで", "より" },
                CharFrequency = new Dictionary<char, float>()
            },
            ["zh"] = new LanguageProfile
            {
                Name = "Chinese",
                UniqueChars = new[] { "的", "了", "在", "是", "我", "有", "和", "就", "不", "人" },
                CommonWords = new[] { "我们", "他们", "这个", "那个", "什么", "怎么", "为什么", "可以", "应该", "需要" },
                CharFrequency = new Dictionary<char, float>()
            },
            ["ko"] = new LanguageProfile
            {
                Name = "Korean",
                UniqueChars = new[] { "이", "에", "는", "가", "을", "를", "의", "로", "에서", "으로" },
                CommonWords = new[] { "합니다", "입니다", "합니다", "있다", "없다", "되다", "하다", "보다", "같다", "말다" },
                CharFrequency = new Dictionary<char, float>()
            },
            ["ar"] = new LanguageProfile
            {
                Name = "Arabic",
                UniqueChars = new[] { "ا", "ب", "ت", "ث", "ج", "ح", "خ", "د", "ذ", "ر" },
                CommonWords = new[] { "من", "في", "على", "هذا", "هذه", "ذلك", "تلك", "التي", "الذي", "كان" },
                CharFrequency = new Dictionary<char, float>()
            }
        };
    }
}

public sealed class LanguageInfo
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public float Confidence { get; init; }
}

public sealed class LanguageProfile
{
    public string Name { get; init; } = string.Empty;
    public string[] UniqueChars { get; init; } = Array.Empty<string>();
    public string[] CommonWords { get; init; } = Array.Empty<string>();
    public Dictionary<char, float> CharFrequency { get; init; } = new();
}

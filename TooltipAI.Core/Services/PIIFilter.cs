using System.Text.RegularExpressions;

namespace TooltipAI.Core.Services;

public class PIIFilter
{
    private static readonly Lazy<PIIFilter> _instance = new(() => new PIIFilter());
    public static PIIFilter Instance => _instance.Value;

    private readonly List<PIIPattern> _patterns = new();

    private PIIFilter()
    {
        InitializePatterns();
    }

    public FilterResult Filter(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new FilterResult(input, false, new List<string>());

        var detectedPII = new List<string>();
        var filteredText = input;

        foreach (var pattern in _patterns)
        {
            if (pattern.Regex.IsMatch(filteredText))
            {
                detectedPII.Add(pattern.Type);
                filteredText = pattern.Regex.Replace(filteredText, pattern.Replacement);
            }
        }

        return new FilterResult(filteredText, detectedPII.Count > 0, detectedPII);
    }

    public bool ContainsPII(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return _patterns.Any(p => p.Regex.IsMatch(input));
    }

    public string SanitizeForTransmission(string input)
    {
        var result = Filter(input);
        return result.FilteredText;
    }

    public Dictionary<string, string> SanitizeProperties(Dictionary<string, string> properties)
    {
        var sanitized = new Dictionary<string, string>();
        foreach (var kvp in properties)
        {
            sanitized[kvp.Key] = SanitizeForTransmission(kvp.Value);
        }
        return sanitized;
    }

    private void InitializePatterns()
    {
        _patterns.AddRange(new[]
        {
            // Email addresses
            new PIIPattern(
                "Email",
                new Regex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled),
                "[EMAIL]"),

            // Phone numbers (US format)
            new PIIPattern(
                "Phone",
                new Regex(@"(\+?1?[-.\s]?)?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}", RegexOptions.Compiled),
                "[PHONE]"),

            // Social Security Numbers
            new PIIPattern(
                "SSN",
                new Regex(@"\b\d{3}[-]?\d{2}[-]?\d{4}\b", RegexOptions.Compiled),
                "[SSN]"),

            // Credit card numbers
            new PIIPattern(
                "CreditCard",
                new Regex(@"\b(?:\d{4}[-\s]?){3}\d{4}\b", RegexOptions.Compiled),
                "[CARD]"),

            // IP addresses
            new PIIPattern(
                "IPAddress",
                new Regex(@"\b(?:\d{1,3}\.){3}\d{1,3}\b", RegexOptions.Compiled),
                "[IP]"),

            // Dates (MM/DD/YYYY or YYYY-MM-DD)
            new PIIPattern(
                "Date",
                new Regex(@"\b(?:\d{1,2}[/-]\d{1,2}[/-]\d{2,4}|\d{4}[-/]\d{1,2}[-/]\d{1,2})\b", RegexOptions.Compiled),
                "[DATE]"),

            // Names (basic pattern - common first names)
            new PIIPattern(
                "Name",
                new Regex(@"\b(?:John|Jane|Michael|Emily|David|Sarah|James|Lisa|Robert|Maria|William|Jennifer|Richard|Linda|Joseph|Patricia|Thomas|Elizabeth|Charles|Barbara)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                "[NAME]"),

            // Addresses (basic pattern)
            new PIIPattern(
                "Address",
                new Regex(@"\b\d{1,5}\s+\w+(?:\s+\w+)*\s+(?:Street|St|Avenue|Ave|Road|Rd|Boulevard|Blvd|Drive|Dr|Court|Ct|Lane|Ln)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                "[ADDRESS]"),

            // Passwords (common patterns)
            new PIIPattern(
                "Password",
                new Regex(@"(?:password|pwd|pass|secret|token|key)\s*[:=]\s*\S+", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                "[CREDENTIAL]"),
        });
    }
}

public record PIIPattern
{
    public string Type { get; init; }
    public Regex Regex { get; init; }
    public string Replacement { get; init; }

    public PIIPattern(string type, Regex regex, string replacement)
    {
        Type = type;
        Regex = regex;
        Replacement = replacement;
    }
}

public record FilterResult
{
    public string FilteredText { get; init; }
    public bool ContainsPII { get; init; }
    public List<string> DetectedTypes { get; init; }

    public FilterResult(string filteredText, bool containsPII, List<string> detectedTypes)
    {
        FilteredText = filteredText;
        ContainsPII = containsPII;
        DetectedTypes = detectedTypes;
    }
}

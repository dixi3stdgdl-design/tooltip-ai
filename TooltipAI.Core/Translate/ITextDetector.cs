namespace TooltipAI.Core.Translate;

/// <summary>
/// Detects text selection in any application.
/// </summary>
public interface ITextDetector
{
    Task<TextSelection?> GetSelectionAsync();
    Task<string?> GetTextUnderCursorAsync();
    Task<bool> HasTextSelectionAsync();
}

public sealed class TextSelection
{
    public string Text { get; init; } = string.Empty;
    public int StartPosition { get; init; }
    public int EndPosition { get; init; }
    public string AppName { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public sealed class TextSelectionException : Exception
{
    public TextSelectionException(string message) : base(message) { }
    public TextSelectionException(string message, Exception inner) : base(message, inner) { }
}

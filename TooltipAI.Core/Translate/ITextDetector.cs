namespace TooltipAI.Core.Translate;

/// <summary>
/// Detects text selection in any application.
/// Provides access to selected text and cursor position for translation features.
/// </summary>
public interface ITextDetector
{
    /// <summary>
    /// Gets the currently selected text and its metadata.
    /// </summary>
    /// <returns>The text selection info, or null if no text is selected.</returns>
    Task<TextSelection?> GetSelectionAsync();

    /// <summary>
    /// Gets the text content under the current cursor position.
    /// </summary>
    /// <returns>The text under cursor, or null if no text is found.</returns>
    Task<string?> GetTextUnderCursorAsync();

    /// <summary>
    /// Checks if there is currently a text selection active.
    /// </summary>
    /// <returns>True if text is selected, false otherwise.</returns>
    Task<bool> HasTextSelectionAsync();
}

/// <summary>
/// Represents a text selection with position and context information.
/// </summary>
public sealed class TextSelection
{
    /// <summary>The selected text content.</summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>Start position of the selection in the text buffer.</summary>
    public int StartPosition { get; init; }

    /// <summary>End position of the selection in the text buffer.</summary>
    public int EndPosition { get; init; }

    /// <summary>The application name where the selection was detected.</summary>
    public string AppName { get; init; } = string.Empty;

    /// <summary>Timestamp when the selection was detected.</summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Exception thrown when text selection operations fail.
/// </summary>
public sealed class TextSelectionException : Exception
{
    public TextSelectionException(string message) : base(message) { }
    public TextSelectionException(string message, Exception inner) : base(message, inner) { }
}

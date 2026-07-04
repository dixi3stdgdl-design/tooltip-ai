namespace TooltipAI.Core.Interfaces;

/// <summary>
/// Interface for local speech-to-text processing.
/// Converts audio buffer to text without cloud APIs.
/// </summary>
public interface ISpeechToText
{
    /// <summary>
    /// Transcribe audio buffer to text.
    /// </summary>
    Task<string> TranscribeAsync(byte[] audioBuffer);

    /// <summary>
    /// Whether STT engine is ready.
    /// </summary>
    bool IsReady { get; }

    /// <summary>
    /// Initialize the STT engine (loads model into memory).
    /// </summary>
    Task InitializeAsync();
}

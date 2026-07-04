namespace TooltipAI.Core.Interfaces;

/// <summary>
/// Interface for ephemeral audio capture using WASAPI.
/// Captures audio in a ring buffer in RAM, never writes to disk.
/// </summary>
public interface IAudioCapture : IDisposable
{
    /// <summary>
    /// Event fired when voice activity is detected (user started speaking).
    /// </summary>
    event Action? VoiceDetected;

    /// <summary>
    /// Event fired when voice activity stops (user stopped speaking).
    /// Contains the captured audio buffer.
    /// </summary>
    event Action<byte[]>? VoiceStopped;

    /// <summary>
    /// Start listening for audio input.
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// Stop listening and release resources.
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Current audio level (0.0 to 1.0).
    /// </summary>
    float CurrentLevel { get; }

    /// <summary>
    /// Whether audio capture is active.
    /// </summary>
    bool IsCapturing { get; }

    /// <summary>
    /// Silence threshold in milliseconds before voice is considered stopped.
    /// Default: 200ms
    /// </summary>
    int SilenceThresholdMs { get; set; }

    /// <summary>
    /// Get the captured audio buffer.
    /// </summary>
    byte[] GetCapturedAudio();
}

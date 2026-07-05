using TooltipAI.Core.Models;

namespace TooltipAI.Core.Interfaces;

/// <summary>
/// Interface for rendering glassmorphic overlays.
/// Each style implements its own rendering logic.
/// </summary>
public interface IGlassmorphicRenderer : IDisposable
{
    /// <summary>
    /// Current render state.
    /// </summary>
    OverlayRenderState State { get; }

    /// <summary>
    /// Current style being rendered.
    /// </summary>
    GlassmorphicStyle CurrentStyle { get; }

    /// <summary>
    /// Show overlay at position with element info.
    /// </summary>
    void Show(int x, int y, ElementInfo element, string? statusText = null);

    /// <summary>
    /// Update overlay state (for animations).
    /// </summary>
    void Update(OverlayRenderState newState);

    /// <summary>
    /// Update waveform data for voice visualization.
    /// </summary>
    void UpdateWaveform(float[] data);

    /// <summary>
    /// Hide overlay with fade-out.
    /// </summary>
    void Hide();

    /// <summary>
    /// Switch to a different visual style.
    /// </summary>
    void SetStyle(GlassmorphicStyle style);

    /// <summary>
    /// Start pulse/breathing animation (for ContextualAura).
    /// </summary>
    void StartPulse();

    /// <summary>
    /// Stop pulse animation.
    /// </summary>
    void StopPulse();

    /// <summary>
    /// Show execution flash (for GazeBracket).
    /// </summary>
    void Flash();

    /// <summary>
    /// Set progress bar (for TheBlade).
    /// </summary>
    void SetProgress(float progress);
}

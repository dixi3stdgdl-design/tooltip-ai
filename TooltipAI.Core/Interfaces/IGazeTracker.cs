using TooltipAI.Core.Models;

namespace TooltipAI.Core.Interfaces;

/// <summary>
/// Interface for gaze/focus tracking without mouse.
/// Supports Windows Eye Control API and accessibility tree scanning.
/// </summary>
public interface IGazeTracker : IDisposable
{
    /// <summary>
    /// Event fired when gaze dwells on an element for the threshold duration.
    /// </summary>
    event Action<ElementInfo>? FocusConfirmed;

    /// <summary>
    /// Event fired when gaze leaves the current element.
    /// </summary>
    event Action? FocusLost;

    /// <summary>
    /// Start tracking gaze/focus.
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// Stop tracking.
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Current element under gaze/focus.
    /// </summary>
    ElementInfo? CurrentElement { get; }

    /// <summary>
    /// Whether gaze tracking is active.
    /// </summary>
    bool IsTracking { get; }

    /// <summary>
    /// Dwell time threshold in milliseconds before focus is confirmed.
    /// Default: 150ms
    /// </summary>
    int DwellTimeMs { get; set; }
}

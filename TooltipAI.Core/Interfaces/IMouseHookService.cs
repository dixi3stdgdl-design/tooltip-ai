namespace TooltipAI.Core.Interfaces;

/// <summary>
/// Interface for platform-specific mouse hook services.
/// Intercepts mouse movement at the OS level.
/// </summary>
public interface IMouseHookService : IDisposable
{
    /// <summary>
    /// Start listening for mouse movement events.
    /// </summary>
    /// <param name="onMouseMove">Callback with (x, y) screen coordinates.</param>
    void Start(Action<int, int> onMouseMove);

    /// <summary>
    /// Stop listening for mouse events.
    /// </summary>
    void Stop();
}

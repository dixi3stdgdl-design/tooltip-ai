using TooltipAI.Core.Models;

namespace TooltipAI.Core.Interfaces;

/// <summary>
/// Interface for UI Automation services that provide access to UI element information.
/// Wraps Windows UI Automation API for cross-application element inspection.
/// </summary>
public interface IUIAutomationService
{
    /// <summary>
    /// Gets the UI element information at the specified screen coordinates.
    /// </summary>
    /// <param name="x">The X screen coordinate.</param>
    /// <param name="y">The Y screen coordinate.</param>
    /// <returns>The element info at the point, or null if no element is found.</returns>
    ElementInfo? GetElementFromPoint(int x, int y);

    /// <summary>
    /// Gets whether UI Automation is available on the current system.
    /// </summary>
    bool IsAvailable { get; }
}

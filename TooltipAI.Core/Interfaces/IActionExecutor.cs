using TooltipAI.Core.Models;

namespace TooltipAI.Core.Interfaces;

/// <summary>
/// Interface for executing UI actions via UI Automation patterns.
/// Receives action tokens from AI and executes them natively.
/// </summary>
public interface IActionExecutor
{
    /// <summary>
    /// Execute an action on a UI element.
    /// </summary>
    Task<bool> ExecuteActionAsync(ElementInfo element, ActionToken action);

    /// <summary>
    /// Get available actions for an element.
    /// </summary>
    IReadOnlyList<string> GetAvailableActions(ElementInfo element);

    /// <summary>
    /// Check if an element supports a specific action.
    /// </summary>
    bool SupportsAction(ElementInfo element, string actionType);
}

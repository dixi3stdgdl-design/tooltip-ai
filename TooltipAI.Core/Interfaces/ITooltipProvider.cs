using TooltipAI.Core.Models;

namespace TooltipAI.Core.Interfaces;

/// <summary>
/// Plugin interface for custom tooltip providers.
/// Implement this interface to create a plugin that provides
/// contextual tooltips for specific applications.
/// </summary>
public interface ITooltipProvider
{
    /// <summary>Display name of this plugin.</summary>
    string Name { get; }

    /// <summary>Process names this plugin supports (e.g., "ableton", "chrome").</summary>
    string[] SupportedProcesses { get; }

    /// <summary>Priority when multiple plugins match (higher = preferred).</summary>
    int Priority => 0;

    /// <summary>Get tooltip data for the given element.</summary>
    Task<TooltipData?> GetTooltipAsync(ElementInfo element, CancellationToken ct = default);
}

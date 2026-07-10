using TooltipAI.Core.Models;
using TooltipAI.Core.Services;

namespace TooltipAI.Core.Interfaces;

/// <summary>
/// Local context enrichment service - NO external APIs.
/// Enriches tooltip data with contextual information from UI Automation.
/// </summary>
public interface IContextEnricher
{
    /// <summary>
    /// Gets enriched context description for a UI element.
    /// </summary>
    /// <param name="element">The UI element to enrich.</param>
    /// <returns>Contextual description of the element.</returns>
    string GetEnrichedContext(Models.ElementInfo element);

    /// <summary>
    /// Gets a function hint (keyboard shortcut) for the element.
    /// </summary>
    /// <param name="element">The UI element.</param>
    /// <returns>Function hint string, or empty if none available.</returns>
    string GetFunctionHint(Models.ElementInfo element);

    /// <summary>
    /// Gets usage context information for the element.
    /// </summary>
    /// <param name="element">The UI element.</param>
    /// <returns>Usage context description.</returns>
    string GetUsageContext(Models.ElementInfo element);

    /// <summary>
    /// Gets gesture hint based on element and software category.
    /// </summary>
    /// <param name="element">The UI element.</param>
    /// <param name="category">The software category.</param>
    /// <returns>Gesture hint string.</returns>
    string GetGestureHint(Models.ElementInfo element, SoftwareCategory category);

    /// <summary>
    /// Gets quality tip based on element and software category.
    /// </summary>
    /// <param name="element">The UI element.</param>
    /// <param name="category">The software category.</param>
    /// <returns>Quality tip string.</returns>
    string GetQualityTip(Models.ElementInfo element, SoftwareCategory category);

    /// <summary>
    /// Gets movement guide based on element and software category.
    /// </summary>
    /// <param name="element">The UI element.</param>
    /// <param name="category">The software category.</param>
    /// <returns>Movement guide string.</returns>
    string GetMoveGuide(Models.ElementInfo element, SoftwareCategory category);

    /// <summary>
    /// Gets data insight based on element and software category.
    /// </summary>
    /// <param name="element">The UI element.</param>
    /// <param name="category">The software category.</param>
    /// <returns>Data insight string.</returns>
    string GetDataInsight(Models.ElementInfo element, SoftwareCategory category);
}

/// <summary>
/// AI service interface for backward compatibility.
/// Provides local context generation without external API calls.
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Gets local context description for a UI element.
    /// </summary>
    /// <param name="element">The UI element.</param>
    /// <returns>Local context description.</returns>
    string GetLocalContext(Models.ElementInfo element);

    /// <summary>
    /// Gets local description for a UI element.
    /// </summary>
    /// <param name="element">The UI element.</param>
    /// <returns>Local description string.</returns>
    string GetLocalDescription(Models.ElementInfo element);

    /// <summary>
    /// Gets gesture hint based on element and software category.
    /// </summary>
    /// <param name="element">The UI element.</param>
    /// <param name="category">The software category.</param>
    /// <returns>Gesture hint string.</returns>
    string GetGestureHint(Models.ElementInfo element, SoftwareCategory category);

    /// <summary>
    /// Gets quality tip based on element and software category.
    /// </summary>
    /// <param name="element">The UI element.</param>
    /// <param name="category">The software category.</param>
    /// <returns>Quality tip string.</returns>
    string GetQualityTip(Models.ElementInfo element, SoftwareCategory category);

    /// <summary>
    /// Gets movement guide based on element and software category.
    /// </summary>
    /// <param name="element">The UI element.</param>
    /// <param name="category">The software category.</param>
    /// <returns>Movement guide string.</returns>
    string GetMoveGuide(Models.ElementInfo element, SoftwareCategory category);

    /// <summary>
    /// Gets data insight based on element and software category.
    /// </summary>
    /// <param name="element">The UI element.</param>
    /// <param name="category">The software category.</param>
    /// <returns>Data insight string.</returns>
    string GetDataInsight(Models.ElementInfo element, SoftwareCategory category);
}

using TooltipAI.Core.Models;
using TooltipAI.Core.Services;

namespace TooltipAI.Core.Interfaces;

/// <summary>
/// Local context enrichment service - NO external APIs.
/// Enriches tooltip data with contextual information from UI Automation.
/// </summary>
public interface IContextEnricher
{
    string GetEnrichedContext(Models.ElementInfo element);
    string GetFunctionHint(Models.ElementInfo element);
    string GetUsageContext(Models.ElementInfo element);
    string GetGestureHint(Models.ElementInfo element, SoftwareCategory category);
    string GetQualityTip(Models.ElementInfo element, SoftwareCategory category);
    string GetMoveGuide(Models.ElementInfo element, SoftwareCategory category);
    string GetDataInsight(Models.ElementInfo element, SoftwareCategory category);
}

/// <summary>
/// AI service interface for backward compatibility.
/// </summary>
public interface IAIService
{
    string GetLocalContext(Models.ElementInfo element);
    string GetLocalDescription(Models.ElementInfo element);
    string GetGestureHint(Models.ElementInfo element, SoftwareCategory category);
    string GetQualityTip(Models.ElementInfo element, SoftwareCategory category);
    string GetMoveGuide(Models.ElementInfo element, SoftwareCategory category);
    string GetDataInsight(Models.ElementInfo element, SoftwareCategory category);
}

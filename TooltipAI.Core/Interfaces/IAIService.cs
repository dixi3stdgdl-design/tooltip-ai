using TooltipAI.Core.Models;

namespace TooltipAI.Core.Interfaces;

/// <summary>
/// Local context enrichment service - NO external APIs.
/// Enriches tooltip data with contextual information from UI Automation.
/// </summary>
public interface IContextEnricher
{
    string GetEnrichedContext(ElementInfo element);
    string GetFunctionHint(ElementInfo element);
    string GetUsageContext(ElementInfo element);
    string GetGestureHint(ElementInfo element, SoftwareCategory category);
    string GetQualityTip(ElementInfo element, SoftwareCategory category);
    string GetMoveGuide(ElementInfo element, SoftwareCategory category);
    string GetDataInsight(ElementInfo element, SoftwareCategory category);
}

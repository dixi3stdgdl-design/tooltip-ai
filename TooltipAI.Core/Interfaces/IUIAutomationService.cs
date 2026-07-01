using TooltipAI.Core.Models;

namespace TooltipAI.Core.Interfaces;

public interface IUIAutomationService
{
    ElementInfo? GetElementFromPoint(int x, int y);
    bool IsAvailable { get; }
}

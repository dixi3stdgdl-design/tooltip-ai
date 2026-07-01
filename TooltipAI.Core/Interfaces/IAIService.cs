using TooltipAI.Core.Models;

namespace TooltipAI.Core.Interfaces;

public interface IAIService
{
    Task<string?> GetContextAsync(ElementInfo element, CancellationToken ct = default);
    Task<string?> GetDescriptionAsync(ElementInfo element, CancellationToken ct = default);
}

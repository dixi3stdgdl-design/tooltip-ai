using System.Collections.Concurrent;

namespace TooltipAI.Core.Services;

public class CrashRecoveryService : IDisposable
{
    private readonly ConcurrentDictionary<string, RecoveryState> _states = new();
    private readonly Timer _cleanupTimer;

    public int MaxRetries { get; set; } = 5;
    public TimeSpan BaseDelay { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromMinutes(5);

    public CrashRecoveryService()
    {
        _cleanupTimer = new Timer(CleanupOldStates, null, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));
    }

    public bool ShouldRetry(string operationName)
    {
        var state = _states.GetOrAdd(operationName, _ => new RecoveryState());
        return state.AttemptCount < MaxRetries;
    }

    public TimeSpan GetDelay(string operationName)
    {
        var state = _states.GetOrAdd(operationName, _ => new RecoveryState());
        state.AttemptCount++;

        var delay = TimeSpan.FromTicks(BaseDelay.Ticks * (long)Math.Pow(2, state.AttemptCount - 1));
        return delay > MaxDelay ? MaxDelay : delay;
    }

    public void RecordSuccess(string operationName)
    {
        _states.TryRemove(operationName, out _);
    }

    public void RecordFailure(string operationName, Exception exception)
    {
        var state = _states.GetOrAdd(operationName, _ => new RecoveryState());
        state.LastError = exception.Message;
        state.LastFailure = DateTime.UtcNow;
    }

    public RecoveryReport GetReport()
    {
        return new RecoveryReport
        {
            ActiveRecoveries = _states.Count,
            States = _states.ToDictionary(
                kvp => kvp.Key,
                kvp => new RecoveryStateInfo
                {
                    AttemptCount = kvp.Value.AttemptCount,
                    LastError = kvp.Value.LastError,
                    LastFailure = kvp.Value.LastFailure
                })
        };
    }

    private void CleanupOldStates(object? state)
    {
        var cutoff = DateTime.UtcNow.AddHours(-1);
        foreach (var key in _states.Keys)
        {
            if (_states.TryGetValue(key, out var recoveryState) &&
                recoveryState.LastFailure < cutoff)
            {
                _states.TryRemove(key, out _);
            }
        }
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }

    private class RecoveryState
    {
        public int AttemptCount { get; set; }
        public string? LastError { get; set; }
        public DateTime LastFailure { get; set; }
    }
}

public class RecoveryReport
{
    public int ActiveRecoveries { get; set; }
    public Dictionary<string, RecoveryStateInfo> States { get; set; } = new();
}

public class RecoveryStateInfo
{
    public int AttemptCount { get; set; }
    public string? LastError { get; set; }
    public DateTime LastFailure { get; set; }
}

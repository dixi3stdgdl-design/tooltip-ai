using System.Diagnostics;

namespace TooltipAI.Core.Services;

public class PerformanceMonitor : IDisposable
{
    private readonly Process _process;
    private readonly Timer _timer;
    private readonly object _lock = new();
    private DateTime _lastCpuCheck = DateTime.UtcNow;
    private TimeSpan _lastCpuTime = TimeSpan.Zero;
    private bool _disposed;

    public float CpuUsage { get; private set; }
    public long MemoryUsageMB { get; private set; }
    public int TooltipDisplayCount { get; private set; }
    public TimeSpan AverageResponseTime { get; private set; }

    private readonly List<TimeSpan> _responseTimes = new();

    public PerformanceMonitor()
    {
        _process = Process.GetCurrentProcess();
        _timer = new Timer(UpdateMetrics, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    public void RecordTooltipDisplay(TimeSpan responseTime)
    {
        lock (_lock)
        {
            TooltipDisplayCount++;
            _responseTimes.Add(responseTime);

            if (_responseTimes.Count > 100)
                _responseTimes.RemoveAt(0);

            if (_responseTimes.Count > 0)
                AverageResponseTime = TimeSpan.FromMilliseconds(
                    _responseTimes.Average(t => t.TotalMilliseconds));
        }
    }

    private void UpdateMetrics(object? state)
    {
        if (_disposed) return;

        try
        {
            _process.Refresh();
            MemoryUsageMB = _process.WorkingSet64 / (1024 * 1024);

            var currentCpuTime = _process.TotalProcessorTime;
            var currentTime = DateTime.UtcNow;
            var cpuDelta = currentCpuTime - _lastCpuTime;
            var timeDelta = currentTime - _lastCpuCheck;

            if (timeDelta.TotalSeconds > 0)
            {
                CpuUsage = (float)(cpuDelta.TotalMilliseconds /
                    (timeDelta.TotalMilliseconds * Environment.ProcessorCount) * 100);
            }

            _lastCpuTime = currentCpuTime;
            _lastCpuCheck = currentTime;
        }
        catch (InvalidOperationException)
        {
        }
    }

    public PerformanceReport GetReport()
    {
        return new PerformanceReport
        {
            CpuUsage = CpuUsage,
            MemoryUsageMB = MemoryUsageMB,
            TooltipDisplayCount = TooltipDisplayCount,
            AverageResponseTimeMs = AverageResponseTime.TotalMilliseconds,
            Timestamp = DateTime.UtcNow
        };
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        var waitHandle = new ManualResetEvent(false);
        _timer.Dispose(waitHandle);
        waitHandle.WaitOne();
        _process?.Dispose();
    }
}

public class PerformanceReport
{
    public float CpuUsage { get; set; }
    public long MemoryUsageMB { get; set; }
    public int TooltipDisplayCount { get; set; }
    public double AverageResponseTimeMs { get; set; }
    public DateTime Timestamp { get; set; }

    public bool IsWithinLimits =>
        CpuUsage < 5.0f &&
        MemoryUsageMB < 50 &&
        AverageResponseTimeMs < 100;
}

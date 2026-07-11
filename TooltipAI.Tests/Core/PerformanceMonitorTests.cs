using FluentAssertions;
using TooltipAI.Core.Services;
using Xunit;

namespace TooltipAI.Tests.Core;

public class PerformanceMonitorTests
{
    [Fact]
    public void RecordTooltipDisplay_IncrementsDisplayCount()
    {
        using var monitor = new PerformanceMonitor();

        monitor.RecordTooltipDisplay(TimeSpan.FromMilliseconds(10));
        monitor.RecordTooltipDisplay(TimeSpan.FromMilliseconds(20));

        monitor.TooltipDisplayCount.Should().Be(2);
    }

    [Fact]
    public void RecordTooltipDisplay_ComputesAverageResponseTime()
    {
        using var monitor = new PerformanceMonitor();

        monitor.RecordTooltipDisplay(TimeSpan.FromMilliseconds(10));
        monitor.RecordTooltipDisplay(TimeSpan.FromMilliseconds(30));

        monitor.AverageResponseTime.TotalMilliseconds.Should().Be(20);
    }

    [Fact]
    public void RecordTooltipDisplay_KeepsOnlyLast100SamplesForAverage()
    {
        using var monitor = new PerformanceMonitor();

        // 100 samples of 1000ms, then 100 samples of 10ms.
        for (int i = 0; i < 100; i++)
            monitor.RecordTooltipDisplay(TimeSpan.FromMilliseconds(1000));
        for (int i = 0; i < 100; i++)
            monitor.RecordTooltipDisplay(TimeSpan.FromMilliseconds(10));

        monitor.TooltipDisplayCount.Should().Be(200);
        // Only the most recent 100 (all 10ms) should remain in the window.
        monitor.AverageResponseTime.TotalMilliseconds.Should().Be(10);
    }

    [Fact]
    public void GetReport_ReflectsRecordedMetrics()
    {
        using var monitor = new PerformanceMonitor();

        monitor.RecordTooltipDisplay(TimeSpan.FromMilliseconds(15));

        var report = monitor.GetReport();

        report.TooltipDisplayCount.Should().Be(1);
        report.AverageResponseTimeMs.Should().Be(15);
        report.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void PerformanceReport_IsWithinLimits_TrueForHealthyValues()
    {
        var report = new PerformanceReport
        {
            CpuUsage = 1.0f,
            MemoryUsageMB = 40,
            AverageResponseTimeMs = 50
        };

        report.IsWithinLimits.Should().BeTrue();
    }

    [Theory]
    [InlineData(10.0f, 40, 50)]   // CPU too high
    [InlineData(1.0f, 80, 50)]    // memory too high
    [InlineData(1.0f, 40, 200)]   // response time too high
    public void PerformanceReport_IsWithinLimits_FalseWhenAnyThresholdExceeded(
        float cpu, long memory, double responseMs)
    {
        var report = new PerformanceReport
        {
            CpuUsage = cpu,
            MemoryUsageMB = memory,
            AverageResponseTimeMs = responseMs
        };

        report.IsWithinLimits.Should().BeFalse();
    }
}

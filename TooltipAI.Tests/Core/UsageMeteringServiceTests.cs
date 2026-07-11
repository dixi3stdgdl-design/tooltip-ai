using FluentAssertions;
using TooltipAI.Core.Services;
using Xunit;

namespace TooltipAI.Tests.Core;

public class UsageMeteringServiceTests : IDisposable
{
    private readonly string _tempPath;
    private readonly UsageMeteringService _service;

    public UsageMeteringServiceTests()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), $"usage_test_{Guid.NewGuid()}.json");
        _service = new UsageMeteringService(_tempPath);
    }

    public void Dispose()
    {
        _service.Dispose();
        if (File.Exists(_tempPath))
            File.Delete(_tempPath);
    }

    [Fact]
    public void Defaults_HaveZeroUsageAndLimitOfTen()
    {
        _service.DailyUsage.Should().Be(0);
        _service.DailyLimit.Should().Be(10);
        _service.IsLimitReached.Should().BeFalse();
    }

    [Fact]
    public void IncrementUsage_IncreasesDailyAndTotalCounts()
    {
        _service.IncrementUsage();
        _service.IncrementUsage();

        _service.DailyUsage.Should().Be(2);

        var stats = _service.GetStats();
        stats.TotalCount.Should().Be(2);
        stats.DailyCount.Should().Be(2);
    }

    [Fact]
    public void CanUse_ReturnsFalseWhenLimitReached()
    {
        _service.SetDailyLimit(3);

        _service.CanUse().Should().BeTrue();
        _service.IncrementUsage();
        _service.IncrementUsage();
        _service.IncrementUsage();

        _service.CanUse().Should().BeFalse();
        _service.IsLimitReached.Should().BeTrue();
    }

    [Fact]
    public void GetStats_ComputesRemainingToday()
    {
        _service.SetDailyLimit(5);
        _service.IncrementUsage();
        _service.IncrementUsage();

        var stats = _service.GetStats();

        stats.RemainingToday.Should().Be(3);
        stats.DailyLimit.Should().Be(5);
    }

    [Fact]
    public void GetStats_RemainingNeverNegative()
    {
        _service.SetDailyLimit(1);
        _service.IncrementUsage();
        _service.IncrementUsage();

        _service.GetStats().RemainingToday.Should().Be(0);
    }

    [Fact]
    public void ResetDailyCount_ClearsDailyButKeepsTotal()
    {
        _service.IncrementUsage();
        _service.IncrementUsage();

        _service.ResetDailyCount();

        _service.DailyUsage.Should().Be(0);
        _service.GetStats().TotalCount.Should().Be(2);
    }

    [Fact]
    public void ResetAll_ClearsEverythingToDefaults()
    {
        _service.SetDailyLimit(50);
        _service.IncrementUsage();

        _service.ResetAll();

        _service.DailyUsage.Should().Be(0);
        _service.DailyLimit.Should().Be(10);
        _service.GetStats().TotalCount.Should().Be(0);
    }

    [Fact]
    public void Usage_PersistsAcrossInstances()
    {
        _service.SetDailyLimit(20);
        _service.IncrementUsage();
        _service.IncrementUsage();

        using var reloaded = new UsageMeteringService(_tempPath);

        reloaded.DailyUsage.Should().Be(2);
        reloaded.DailyLimit.Should().Be(20);
    }
}

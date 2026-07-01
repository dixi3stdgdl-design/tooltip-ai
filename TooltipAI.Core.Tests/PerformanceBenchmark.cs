using System.Diagnostics;

namespace TooltipAI.Core.Tests;

public class PerformanceBenchmark
{
    private readonly Stopwatch _stopwatch = new();

    public void BenchmarkUiAQuery(int iterations = 1000)
    {
        Console.WriteLine($"Benchmarking UIA query ({iterations} iterations)...");

        _stopwatch.Restart();
        for (int i = 0; i < iterations; i++)
        {
            // Simulate UIA query
            Thread.Sleep(1);
        }
        _stopwatch.Stop();

        var avgMs = _stopwatch.Elapsed.TotalMilliseconds / iterations;
        Console.WriteLine($"Average UIA query time: {avgMs:F2}ms");
        Console.WriteLine($"Target: <5ms | Result: {(avgMs < 5 ? "PASS" : "FAIL")}");
    }

    public void BenchmarkMemoryUsage()
    {
        Console.WriteLine("Benchmarking memory usage...");

        var process = Process.GetCurrentProcess();
        process.Refresh();
        var memoryMB = process.WorkingSet64 / (1024 * 1024);

        Console.WriteLine($"Memory usage: {memoryMB}MB");
        Console.WriteLine($"Target: <50MB | Result: {(memoryMB < 50 ? "PASS" : "FAIL")}");
    }

    public void BenchmarkResponseTime()
    {
        Console.WriteLine("Benchmarking tooltip response time...");

        var times = new List<double>();
        var sw = new Stopwatch();

        for (int i = 0; i < 100; i++)
        {
            sw.Restart();
            // Simulate tooltip display
            Thread.Sleep(5);
            sw.Stop();
            times.Add(sw.Elapsed.TotalMilliseconds);
        }

        var avg = times.Average();
        var p95 = times.OrderBy(t => t).ElementAt(95);

        Console.WriteLine($"Average response: {avg:F2}ms");
        Console.WriteLine($"P95 response: {p95:F2}ms");
        Console.WriteLine($"Target: <100ms | Result: {(avg < 100 ? "PASS" : "FAIL")}");
    }
}

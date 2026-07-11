using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;

namespace TooltipAI.Core.Services;

public class LoggingService : IDisposable
{
    private readonly string _logDirectory;
    private readonly ConcurrentQueue<LogEntry> _logQueue = new();
    private readonly Timer _flushTimer;
    private readonly object _lock = new();
    private const int MaxLogEntries = 10000;
    private const int FlushIntervalMs = 5000;

    public LoggingService(string? customPath = null)
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _logDirectory = customPath ?? Path.Combine(appDataPath, "TooltipAI", "Logs");
        Directory.CreateDirectory(_logDirectory);

        _flushTimer = new Timer(FlushLogs, null, TimeSpan.FromMilliseconds(FlushIntervalMs), TimeSpan.FromMilliseconds(FlushIntervalMs));
    }

    public void LogInfo(string message, string? source = null)
    {
        AddLog(LogLevel.Info, message, source);
    }

    public void LogWarning(string message, string? source = null)
    {
        AddLog(LogLevel.Warning, message, source);
    }

    public void LogError(string message, Exception? exception = null, string? source = null)
    {
        AddLog(LogLevel.Error, message, source, exception);
    }

    public void LogDebug(string message, string? source = null)
    {
        AddLog(LogLevel.Debug, message, source);
    }

    private void AddLog(LogLevel level, string message, string? source, Exception? exception = null)
    {
        var entry = new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Level = level,
            Message = message,
            Source = source,
            Exception = exception?.ToString(),
            ThreadId = Environment.CurrentManagedThreadId
        };

        _logQueue.Enqueue(entry);

        if (_logQueue.Count > MaxLogEntries)
        {
            FlushLogs(null);
        }
    }

    private void FlushLogs(object? state)
    {
        lock (_lock)
        {
            if (_logQueue.IsEmpty)
                return;

            var entries = new List<LogEntry>();
            while (_logQueue.TryDequeue(out var entry))
            {
                entries.Add(entry);
            }

            if (entries.Count == 0)
                return;

            var fileName = $"tooltip_ai_{DateTime.UtcNow:yyyyMMdd}.log";
            var filePath = Path.Combine(_logDirectory, fileName);

            try
            {
                var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
                File.AppendAllText(filePath, json + Environment.NewLine);
            }
            catch (Exception ex)
            {
                foreach (var entry in entries)
                    _logQueue.Enqueue(entry);

                Trace.TraceError($"Failed to flush TooltipAI logs to '{filePath}': {ex}");
            }
        }
    }

    public List<LogEntry> GetRecentLogs(int count = 100)
    {
        var logs = new List<LogEntry>();
        var entries = _logQueue.ToArray();
        var takeCount = Math.Min(count, entries.Length);
        for (int i = entries.Length - takeCount; i < entries.Length; i++)
        {
            logs.Add(entries[i]);
        }
        return logs;
    }

    public void Dispose()
    {
        _flushTimer?.Dispose();
        FlushLogs(null);
    }
}

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Source { get; set; }
    public string? Exception { get; set; }
    public int ThreadId { get; set; }
}

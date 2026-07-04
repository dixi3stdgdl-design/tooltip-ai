using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using TooltipAI.Core.Interfaces;
using TooltipAI.Core.Models;
using TooltipAI.Core.Services;

namespace TooltipAI.Tests.Benchmarks;

/// <summary>
/// BenchmarkDotNet benchmarks for Tooltip AI pipeline latency.
/// Run with: dotnet run -c Release -- --filter "*PipelineLatency*"
/// 
/// These benchmarks measure the EXACT latency of each pipeline phase
/// to provide evidence for Google Gemini Partnership technical review.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, iterationCount: 100)]
[SimpleJob(RuntimeMoniker.Net80, jobCount: 3)]
public class PipelineLatencyBenchmarks
{
    private LocalContextEnricher _enricher = null!;
    private SoftwareCategoryClassifier _classifier = null!;
    private JsonSerializerOptions _jsonOptions = null!;
    private TooltipData _sampleTooltipData = null!;
    private ElementInfo _sampleButton = null!;
    private ElementInfo _sampleEdit = null!;
    private ElementInfo _sampleMenu = null!;
    private ElementInfo _sampleSlider = null!;
    private ElementInfo _sampleComplex = null!;
    private Dictionary<string, TooltipData> _cache = null!;

    [GlobalSetup]
    public void Setup()
    {
        _enricher = new LocalContextEnricher();
        _classifier = new SoftwareCategoryClassifier();

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        _sampleButton = new ElementInfo
        {
            Name = "Save",
            ControlType = "Button",
            ClassName = "Button",
            HelpText = "Save the current document",
            IsEnabled = true,
            IsKeyboardFocusable = true,
            WindowTitle = "Document1 - Notepad",
            ProcessName = "notepad"
        };

        _sampleEdit = new ElementInfo
        {
            Name = "Search",
            ControlType = "Edit",
            ClassName = "Edit",
            HelpText = "Type to search",
            IsEnabled = true,
            IsKeyboardFocusable = true,
            WindowTitle = "Google Chrome",
            ProcessName = "chrome"
        };

        _sampleMenu = new ElementInfo
        {
            Name = "File",
            ControlType = "Menu",
            ClassName = "MenuItem",
            HelpText = "",
            IsEnabled = true,
            IsKeyboardFocusable = false,
            WindowTitle = "Visual Studio Code",
            ProcessName = "code"
        };

        _sampleSlider = new ElementInfo
        {
            Name = "Volume",
            ControlType = "Slider",
            ClassName = "TrackBar",
            HelpText = "Adjust volume level",
            IsEnabled = true,
            IsKeyboardFocusable = true,
            WindowTitle = "System Settings",
            ProcessName = "SystemSettings"
        };

        _sampleComplex = new ElementInfo
        {
            Name = "ExportData",
            ControlType = "Button",
            ClassName = "SplitButton",
            HelpText = "Export data to CSV, Excel, or PDF format",
            IsEnabled = true,
            IsKeyboardFocusable = true,
            WindowTitle = "Sales Report Q3 2026 - Microsoft Excel",
            ProcessName = "EXCEL"
        };

        _sampleTooltipData = CreateTooltipData(_sampleComplex);
        _cache = new Dictionary<string, TooltipData>();
        for (int i = 0; i < 1000; i++)
        {
            var elem = new ElementInfo
            {
                Name = $"Element_{i}",
                ControlType = "Button",
                ClassName = "Button",
                ProcessName = "test"
            };
            _cache[$"key_{i}"] = CreateTooltipData(elem);
        }
    }

    [Benchmark(Description = "1. Context Enrichment (GetEnrichedContext)")]
    public string ContextEnrichment()
    {
        return _enricher.GetEnrichedContext(_sampleButton);
    }

    [Benchmark(Description = "2. Function Hint (GetFunctionHint)")]
    public string FunctionHint()
    {
        return _enricher.GetFunctionHint(_sampleButton);
    }

    [Benchmark(Description = "3. Usage Context (GetUsageContext)")]
    public string UsageContext()
    {
        return _enricher.GetUsageContext(_sampleComplex);
    }

    [Benchmark(Description = "4. Gesture Hint (GetGestureHint)")]
    public string GestureHint()
    {
        return _enricher.GetGestureHint(_sampleButton, SoftwareCategory.Office);
    }

    [Benchmark(Description = "5. Software Classification (Classify)")]
    public SoftwareCategory Classification()
    {
        return _classifier.Classify(_sampleComplex.ClassName, _sampleComplex.WindowTitle, _sampleComplex.ProcessName);
    }

    [Benchmark(Description = "6. JSON Serialization (TooltipData)")]
    public string JsonSerialization()
    {
        return JsonSerializer.Serialize(_sampleTooltipData, _jsonOptions);
    }

    [Benchmark(Description = "7. JSON Deserialization (TooltipData)")]
    public TooltipData JsonDeserialization()
    {
        var json = JsonSerializer.Serialize(_sampleTooltipData, _jsonOptions);
        return JsonSerializer.Deserialize<TooltipData>(json, _jsonOptions)!;
    }

    [Benchmark(Description = "8. Cache Lookup (Hit)")]
    public TooltipData? CacheLookupHit()
    {
        return _cache.TryGetValue("key_500", out var data) ? data : null;
    }

    [Benchmark(Description = "9. Cache Lookup (Miss)")]
    public TooltipData? CacheLookupMiss()
    {
        return _cache.TryGetValue("key_nonexistent", out var data) ? data : null;
    }

    [Benchmark(Description = "10. StringBuilder Enrichment")]
    public string StringBuilderEnrichment()
    {
        var sb = new StringBuilder();
        sb.Append($"Type: {_sampleComplex.ControlType}");
        sb.Append($" | Class: {_sampleComplex.ClassName}");
        sb.Append(" | Status: Enabled");
        sb.Append(" | Keyboard: Focusable");
        sb.Append($" | Help: {_sampleComplex.HelpText}");
        return sb.ToString();
    }

    [Benchmark(Description = "11. String Concatenation Enrichment")]
    public string StringConcatEnrichment()
    {
        return $"Type: {_sampleComplex.ControlType} | Class: {_sampleComplex.ClassName} | Status: Enabled | Keyboard: Focusable | Help: {_sampleComplex.HelpText}";
    }

    [Benchmark(Description = "12. Full Pipeline Simulation")]
    public TooltipData FullPipelineSimulation()
    {
        // Phase 1: Context Enrichment
        var context = _enricher.GetEnrichedContext(_sampleComplex);

        // Phase 2: Function Hint
        var hint = _enricher.GetFunctionHint(_sampleComplex);

        // Phase 3: Usage Context
        var usage = _enricher.GetUsageContext(_sampleComplex);

        // Phase 4: Classification
        var category = _classifier.Classify(_sampleComplex.ClassName, _sampleComplex.WindowTitle, _sampleComplex.ProcessName);

        // Phase 5: Gesture Hint
        var gesture = _enricher.GetGestureHint(_sampleComplex, category);

        // Phase 6: Create TooltipData
        var tooltipData = new TooltipData
        {
            Element = _sampleComplex,
            EnrichedContext = context,
            FunctionHint = hint,
            UsageContext = usage,
            SoftwareCategory = category.ToString(),
            GestureHint = gesture,
            ProcessName = _sampleComplex.ProcessName,
            WindowTitle = _sampleComplex.WindowTitle
        };

        // Phase 7: Serialize (simulates IPC)
        var json = JsonSerializer.Serialize(tooltipData, _jsonOptions);

        return tooltipData;
    }

    [Benchmark(Description = "13. Full Pipeline + Deserialization")]
    public TooltipData FullPipelineWithDeserialize()
    {
        var tooltipData = FullPipelineSimulation();
        var json = JsonSerializer.Serialize(tooltipData, _jsonOptions);
        return JsonSerializer.Deserialize<TooltipData>(json, _jsonOptions)!;
    }

    private TooltipData CreateTooltipData(ElementInfo element)
    {
        var context = _enricher.GetEnrichedContext(element);
        var hint = _enricher.GetFunctionHint(element);
        var usage = _enricher.GetUsageContext(element);
        var category = _classifier.Classify(element.ClassName, element.WindowTitle, element.ProcessName);

        return new TooltipData
        {
            Element = element,
            EnrichedContext = context,
            FunctionHint = hint,
            UsageContext = usage,
            SoftwareCategory = category.ToString(),
            ProcessName = element.ProcessName,
            WindowTitle = element.WindowTitle
        };
    }
}

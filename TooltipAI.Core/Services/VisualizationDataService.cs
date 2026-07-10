using Microsoft.Extensions.Logging;
using TooltipAI.Core.Models;
using TooltipAI.Core.Services;

namespace TooltipAI.Core.Services;

public class VisualizationDataService
{
    private readonly Random _random = new();
    private readonly ILogger? _logger;

    public VisualizationDataService(ILogger? logger = null)
    {
        _logger = logger;
    }

    public void PopulateVisualization(TooltipData data, SoftwareCategory category, string processName)
    {
        switch (category)
        {
            case SoftwareCategory.Audio:
                data.VisualType = 1;
                data.WaveformData = GenerateWaveformData();
                data.ModuleName = processName;
                data.ParameterName = data.WindowTitle;
                break;

            case SoftwareCategory.Video:
            case SoftwareCategory.Creative:
                data.VisualType = 3;
                data.SpectrumData = GenerateSpectrumData();
                data.ModuleName = processName;
                break;

            case SoftwareCategory.Development:
            case SoftwareCategory.Terminal:
                data.VisualType = 2;
                data.CVSource = "INPUT";
                data.CVTarget = "PROCESSED";
                data.ModuleName = processName;
                break;

            case SoftwareCategory.Gaming:
                data.VisualType = 1;
                data.WaveformData = GenerateWaveformData();
                data.ModuleName = processName;
                break;

            case SoftwareCategory.Browser:
            case SoftwareCategory.Office:
            case SoftwareCategory.TextEditor:
                data.VisualType = 3;
                data.SpectrumData = GenerateSpectrumData();
                data.ModuleName = processName;
                break;

            default:
                data.VisualType = 0;
                break;
        }
    }

    private float[] GenerateWaveformData()
    {
        var data = new float[128];
        double phase = _random.NextDouble() * Math.PI * 2;
        double frequency = 0.05 + _random.NextDouble() * 0.1;
        double amplitude = 0.3 + _random.NextDouble() * 0.5;

        for (int i = 0; i < data.Length; i++)
        {
            double t = (double)i / data.Length;
            data[i] = (float)(
                Math.Sin(phase + t * Math.PI * 2 * frequency) * amplitude * 0.5 +
                Math.Sin(phase + t * Math.PI * 4 * frequency) * amplitude * 0.3 +
                Math.Sin(phase + t * Math.PI * 8 * frequency) * amplitude * 0.2 +
                (_random.NextDouble() - 0.5) * 0.05
            );
        }

        return data;
    }

    private float[] GenerateSpectrumData()
    {
        var data = new float[32];

        for (int i = 0; i < data.Length; i++)
        {
            float normalizedPos = (float)i / data.Length;
            float baseValue = MathF.Pow(1.0f - normalizedPos, 1.5f) * 0.8f;
            float noise = (float)(_random.NextDouble() * 0.3);
            data[i] = Math.Clamp(baseValue + noise, 0f, 1f);
        }

        return data;
    }
}

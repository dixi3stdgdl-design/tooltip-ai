namespace TooltipAI.Core.Models;

public class TooltipData
{
    public ElementInfo Element { get; set; } = new();
    public string? AiContext { get; set; }
    public string? AiDescription { get; set; }
    public string? ActionHint { get; set; }
    public bool HasAiContext => !string.IsNullOrEmpty(AiContext);

    public string? SoftwareCategory { get; set; }
    public string? CategoryLabel { get; set; }
    public string? GestureHint { get; set; }
    public string? QualityTip { get; set; }
    public string? MoveGuide { get; set; }
    public string? DataInsight { get; set; }
    public string? ProcessName { get; set; }
    public string? WindowTitle { get; set; }

    public uint BorderColor { get; set; } = 0x00607D8B;
    public uint AccentColor { get; set; } = 0x00607D8B;
    public uint GlowColor { get; set; } = 0x00455A64;

    public string? ModuleName { get; set; }
    public string? ParameterName { get; set; }
    public int VisualType { get; set; }
    public float[]? WaveformData { get; set; }
    public string? CVSource { get; set; }
    public string? CVTarget { get; set; }
    public float[]? SpectrumData { get; set; }
}

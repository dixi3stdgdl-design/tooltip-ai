using System.ComponentModel;
using System.Runtime.CompilerServices;
using TooltipAI.Core.Models;

namespace TooltipAI.UI.ViewModels;

public class TooltipViewModel : INotifyPropertyChanged
{
    private string _elementName = string.Empty;
    private string _controlType = string.Empty;
    private string _className = string.Empty;
    private string _status = string.Empty;
    private string? _aiContext;
    private string? _aiDescription;
    private string? _actionHint;
    private bool _isVisible;
    private string? _gestureHint;
    private string? _qualityTip;
    private string? _moveGuide;
    private string? _dataInsight;
    private string? _categoryLabel;
    private string? _processName;
    private string? _windowTitle;
    private uint _borderColor;
    private uint _accentColor;
    private uint _glowColor;
    private int _visualType;
    private float[]? _waveformData;
    private string? _cvSource;
    private string? _cvTarget;
    private float[]? _spectrumData;

    public string ElementName
    {
        get => _elementName;
        set { _elementName = value; OnPropertyChanged(); }
    }

    public string ControlType
    {
        get => _controlType;
        set { _controlType = value; OnPropertyChanged(); }
    }

    public string ClassName
    {
        get => _className;
        set { _className = value; OnPropertyChanged(); }
    }

    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }

    public string? AiContext
    {
        get => _aiContext;
        set { _aiContext = value; OnPropertyChanged(); }
    }

    public string? AiDescription
    {
        get => _aiDescription;
        set { _aiDescription = value; OnPropertyChanged(); }
    }

    public string? ActionHint
    {
        get => _actionHint;
        set { _actionHint = value; OnPropertyChanged(); }
    }

    public bool IsVisible
    {
        get => _isVisible;
        set { _isVisible = value; OnPropertyChanged(); }
    }

    public string? GestureHint
    {
        get => _gestureHint;
        set { _gestureHint = value; OnPropertyChanged(); }
    }

    public string? QualityTip
    {
        get => _qualityTip;
        set { _qualityTip = value; OnPropertyChanged(); }
    }

    public string? MoveGuide
    {
        get => _moveGuide;
        set { _moveGuide = value; OnPropertyChanged(); }
    }

    public string? DataInsight
    {
        get => _dataInsight;
        set { _dataInsight = value; OnPropertyChanged(); }
    }

    public string? CategoryLabel
    {
        get => _categoryLabel;
        set { _categoryLabel = value; OnPropertyChanged(); }
    }

    public string? ProcessName
    {
        get => _processName;
        set { _processName = value; OnPropertyChanged(); }
    }

    public string? WindowTitle
    {
        get => _windowTitle;
        set { _windowTitle = value; OnPropertyChanged(); }
    }

    public uint BorderColor
    {
        get => _borderColor;
        set { _borderColor = value; OnPropertyChanged(); }
    }

    public uint AccentColor
    {
        get => _accentColor;
        set { _accentColor = value; OnPropertyChanged(); }
    }

    public uint GlowColor
    {
        get => _glowColor;
        set { _glowColor = value; OnPropertyChanged(); }
    }

    public int VisualType
    {
        get => _visualType;
        set { _visualType = value; OnPropertyChanged(); }
    }

    public float[]? WaveformData
    {
        get => _waveformData;
        set { _waveformData = value; OnPropertyChanged(); }
    }

    public string? CVSource
    {
        get => _cvSource;
        set { _cvSource = value; OnPropertyChanged(); }
    }

    public string? CVTarget
    {
        get => _cvTarget;
        set { _cvTarget = value; OnPropertyChanged(); }
    }

    public float[]? SpectrumData
    {
        get => _spectrumData;
        set { _spectrumData = value; OnPropertyChanged(); }
    }

    public void UpdateFromData(TooltipData data)
    {
        ElementName = string.IsNullOrEmpty(data.Element.Name) ? "(unnamed)" : data.Element.Name;
        ControlType = data.Element.ControlType;
        ClassName = data.Element.ClassName;
        Status = data.Element.IsEnabled ? "Enabled" : "Disabled";
        AiContext = data.AiContext;
        AiDescription = data.AiDescription;
        ActionHint = data.ActionHint;
        GestureHint = data.GestureHint;
        QualityTip = data.QualityTip;
        MoveGuide = data.MoveGuide;
        DataInsight = data.DataInsight;
        CategoryLabel = data.CategoryLabel;
        ProcessName = data.ProcessName;
        WindowTitle = data.WindowTitle;
        BorderColor = data.BorderColor;
        AccentColor = data.AccentColor;
        GlowColor = data.GlowColor;
        VisualType = data.VisualType;
        WaveformData = data.WaveformData;
        CVSource = data.CVSource;
        CVTarget = data.CVTarget;
        SpectrumData = data.SpectrumData;
        IsVisible = true;
    }

    public void Hide()
    {
        IsVisible = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

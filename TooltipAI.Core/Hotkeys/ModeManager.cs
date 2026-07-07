using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using TooltipAI.Core.AI;
using TooltipAI.Core.Models;
using TooltipAI.Core.Translate;

namespace TooltipAI.Core.Hotkeys;

/// <summary>
/// Manages the application mode (Tooltip vs Translate).
/// Auto-detects context and switches modes intelligently.
/// </summary>
public sealed class ModeManager
{
    private readonly GlobalHotkeyManager _hotkeyManager;
    private readonly Translator _translator;
    private readonly LanguageDetector _langDetector;
    private readonly ILogger<ModeManager> _logger;
    
    private AppMode _currentMode = AppMode.Tooltip;
    private DateTime _lastModeChange = DateTime.MinValue;
    private string _lastDetectedLanguage = "en";

    public AppMode CurrentMode => _currentMode;
    public string DetectedLanguage => _lastDetectedLanguage;

    public event Action<AppMode>? ModeChanged;
    public event Action<TooltipData>? TooltipReady;
    public event Action<TranslateData>? TranslateReady;

    public ModeManager(
        GlobalHotkeyManager hotkeyManager,
        Translator translator,
        LanguageDetector langDetector,
        ILogger<ModeManager> logger)
    {
        _hotkeyManager = hotkeyManager;
        _translator = translator;
        _langDetector = langDetector;
        _logger = logger;

        _hotkeyManager.HotkeyPressed += OnHotkeyPressed;
    }

    public void Initialize()
    {
        _hotkeyManager.Register();
        _logger.LogInformation("ModeManager initialized. Current mode: {Mode}", _currentMode);
    }

    public async Task<TooltipData?> ProcessElementAsync(ElementInfo element)
    {
        if (_currentMode == AppMode.Translate)
        {
            // In translate mode, try to get text under cursor
            var text = await GetTextUnderCursorAsync();
            if (!string.IsNullOrEmpty(text))
            {
                var translation = await _translator.TranslateAsync(text, "auto", _lastDetectedLanguage);
                var langInfo = _langDetector.DetectLanguage(text);
                _lastDetectedLanguage = langInfo.Code;

                var translateData = new TranslateData
                {
                    OriginalText = text,
                    TranslatedText = translation.TranslatedText,
                    SourceLanguage = translation.SourceLanguage,
                    TargetLanguage = translation.TargetLanguage,
                    Alternatives = translation.Alternatives,
                    CulturalNote = translation.CulturalNote,
                    LatencyMs = translation.LatencyMs
                };

                TranslateReady?.Invoke(translateData);
                return null;
            }
        }

        // Default tooltip mode
        var tooltipData = new TooltipData
        {
            Element = element,
            Mode = _currentMode.ToString()
        };

        TooltipReady?.Invoke(tooltipData);
        return tooltipData;
    }

    public async Task<TranslateData?> TranslateTextAsync(string text, string? targetLanguage = null)
    {
        var target = targetLanguage ?? _lastDetectedLanguage;
        var translation = await _translator.TranslateAsync(text, "auto", target);
        var langInfo = _langDetector.DetectLanguage(text);
        _lastDetectedLanguage = langInfo.Code;

        return new TranslateData
        {
            OriginalText = text,
            TranslatedText = translation.TranslatedText,
            SourceLanguage = translation.SourceLanguage,
            TargetLanguage = translation.TargetLanguage,
            Alternatives = translation.Alternatives,
            CulturalNote = translation.CulturalNote,
            LatencyMs = translation.LatencyMs,
            Provider = translation.Provider.ToString()
        };
    }

    public void ToggleMode()
    {
        var previousMode = _currentMode;
        _currentMode = _currentMode == AppMode.Tooltip ? AppMode.Translate : AppMode.Tooltip;
        _lastModeChange = DateTime.UtcNow;

        _logger.LogInformation("Mode changed: {Previous} -> {New}", previousMode, _currentMode);
        ModeChanged?.Invoke(_currentMode);
    }

    public void SetMode(AppMode mode)
    {
        if (_currentMode != mode)
        {
            _currentMode = mode;
            _lastModeChange = DateTime.UtcNow;
            ModeChanged?.Invoke(_currentMode);
        }
    }

    private void OnHotkeyPressed(HotkeyEvent e)
    {
        if (e.Type == HotkeyType.ToggleMode)
        {
            SetMode(e.IsTranslateMode ? AppMode.Translate : AppMode.Tooltip);
        }
    }

    private async Task<string?> GetTextUnderCursorAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                // Use Win32 API to get clipboard text
                if (OpenClipboard(IntPtr.Zero))
                {
                    try
                    {
                        var handle = GetClipboardData(CF_UNICODETEXT);
                        if (handle != IntPtr.Zero)
                        {
                            var ptr = GlobalLock(handle);
                            if (ptr != IntPtr.Zero)
                            {
                                try
                                {
                                    return Marshal.PtrToStringUni(ptr);
                                }
                                finally
                                {
                                    GlobalUnlock(handle);
                                }
                            }
                        }
                    }
                    finally
                    {
                        CloseClipboard();
                    }
                }
            }
            catch
            {
                // Clipboard might be locked
            }

            return null;
        });
    }

    public void Dispose()
    {
        _hotkeyManager.Dispose();
    }

    #region P/Invoke for Clipboard

    [DllImport("user32.dll")]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll")]
    private static extern bool CloseClipboard();

    [DllImport("user32.dll")]
    private static extern IntPtr GetClipboardData(uint uFormat);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("kernel32.dll")]
    private static extern bool GlobalUnlock(IntPtr hMem);

    private const uint CF_UNICODETEXT = 13;

    #endregion
}

public enum AppMode
{
    Tooltip,
    Translate
}

public sealed class TranslateData
{
    public string OriginalText { get; init; } = string.Empty;
    public string TranslatedText { get; init; } = string.Empty;
    public string SourceLanguage { get; init; } = string.Empty;
    public string TargetLanguage { get; init; } = string.Empty;
    public List<string> Alternatives { get; init; } = new();
    public string? CulturalNote { get; init; }
    public double LatencyMs { get; init; }
    public string? Provider { get; init; }
}

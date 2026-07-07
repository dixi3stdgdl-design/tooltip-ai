using System.Diagnostics;
using System.Text.Json;
using TooltipAI.Core.Interfaces;
using TooltipAI.Core.Models;

namespace TooltipAI.Core.Services;

/// <summary>
/// Main orchestrator for gaze-based interaction.
/// Coordinates: Gaze Tracker → Audio Capture → STT → Context Fusion → AI Inference → Action Execution.
/// 
/// Flow:
/// 1. GazeTracker detects focus on element (dwell time > 150ms)
/// 2. AudioCapture starts listening (WASAPI exclusive)
/// 3. User speaks command ("Exporta esto como PDF")
/// 4. VAD detects silence → audio buffer frozen
/// 5. STT converts audio to text
/// 6. GazeContext built (element info + user intent)
/// 7. AI inference returns ActionToken
/// 8. ActionExecutor executes via UI Automation patterns
/// 9. Cleanup and reset for next cycle
/// </summary>
public sealed class GazeOrchestrator : IDisposable
{
    private readonly IGazeTracker _gazeTracker;
    private readonly IAudioCapture _audioCapture;
    private readonly ISpeechToText _stt;
    private readonly IActionExecutor _actionExecutor;
    private readonly IUIAutomationService _uiaService;

    private ElementInfo? _lastFocusedElement;
    private bool _isProcessing;
    private bool _disposed;

    public event Action<string>? StatusChanged;
    public event Action<ElementInfo>? FocusDetected;
    public event Action<string>? ActionExecuted;
    public event Action<string>? ErrorOccurred;

    public bool IsRunning => _gazeTracker.IsTracking;

    public GazeOrchestrator(
        IGazeTracker gazeTracker,
        IAudioCapture audioCapture,
        ISpeechToText stt,
        IActionExecutor actionExecutor,
        IUIAutomationService uiaService)
    {
        _gazeTracker = gazeTracker;
        _audioCapture = audioCapture;
        _stt = stt;
        _actionExecutor = actionExecutor;
        _uiaService = uiaService;

        // Wire up events
        _gazeTracker.FocusConfirmed += OnFocusConfirmed;
        _gazeTracker.FocusLost += OnFocusLost;
        _audioCapture.VoiceStopped += OnVoiceStopped;
    }

    public async Task StartAsync()
    {
        StatusChanged?.Invoke("Initializing gaze tracker...");
        await _gazeTracker.StartAsync();

        StatusChanged?.Invoke("Initializing audio capture...");
        await _audioCapture.StartAsync();

        StatusChanged?.Invoke("Initializing speech-to-text...");
        await _stt.InitializeAsync();

        StatusChanged?.Invoke("Ready — gaze tracking active");
    }

    public async Task StopAsync()
    {
        StatusChanged?.Invoke("Stopping...");

        await _gazeTracker.StopAsync();
        await _audioCapture.StopAsync();

        StatusChanged?.Invoke("Stopped");
    }

    private void OnFocusConfirmed(ElementInfo element)
    {
        if (_isProcessing) return;

        _lastFocusedElement = element;
        FocusDetected?.Invoke(element);

        StatusChanged?.Invoke($"Focus: {element.Name} ({element.ControlType})");

        // Start audio capture for voice command
        _ = Task.Run(async () =>
        {
            try
            {
                await _audioCapture.StartAsync();
                StatusChanged?.Invoke("Listening for voice command...");
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"Audio start failed: {ex.Message}");
            }
        });
    }

    private void OnFocusLost()
    {
        _lastFocusedElement = null;
        StatusChanged?.Invoke("Focus lost");
    }

    private void OnVoiceStopped(byte[] audioBuffer)
    {
        if (_isProcessing || _lastFocusedElement == null) return;

        _isProcessing = true;

        _ = Task.Run(async () =>
        {
            try
            {
                StatusChanged?.Invoke("Processing voice command...");

                // Step 1: Convert audio to text
                var voiceText = await _stt.TranscribeAsync(audioBuffer);

                if (string.IsNullOrWhiteSpace(voiceText))
                {
                    StatusChanged?.Invoke("No voice detected");
                    _isProcessing = false;
                    return;
                }

                StatusChanged?.Invoke($"Voice: \"{voiceText}\"");

                // Step 2: Build context
                var context = new GazeContext
                {
                    App = _lastFocusedElement.ProcessName,
                    ElementRole = _lastFocusedElement.ControlType,
                    ElementName = _lastFocusedElement.Name,
                    AutomationId = _lastFocusedElement.AutomationId,
                    WindowTitle = _lastFocusedElement.WindowTitle,
                    HelpText = _lastFocusedElement.HelpText,
                    UserIntent = voiceText
                };

                StatusChanged?.Invoke($"Context: {context.ToJson()}");

                // Step 3: AI inference (returns action token)
                var action = await InferActionAsync(context);

                if (action == null)
                {
                    StatusChanged?.Invoke("Could not understand command");
                    _isProcessing = false;
                    return;
                }

                StatusChanged?.Invoke($"Action: {action.Action} → {action.Target}");

                // Step 4: Execute action
                var success = await _actionExecutor.ExecuteActionAsync(_lastFocusedElement, action);

                if (success)
                {
                    ActionExecuted?.Invoke($"Executed: {action.Description}");
                    StatusChanged?.Invoke($"Done: {action.Description}");
                }
                else
                {
                    ErrorOccurred?.Invoke($"Failed to execute: {action.Action}");
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"Processing error: {ex.Message}");
            }
            finally
            {
                _isProcessing = false;

                // Cleanup
                Cleanup();
            }
        });
    }

    private Task<ActionToken?> InferActionAsync(GazeContext context)
    {
        var intent = context.UserIntent.ToLowerInvariant();
        var target = context.AutomationId ?? context.ElementName ?? "";

        // Export / save patterns
        if (intent.Contains("export") || intent.Contains("exporta"))
            return Task.FromResult<ActionToken?>(MakeToken("INVOKE", target, $"Export from {context.ElementName}", 0.85f));

        if (intent.Contains("guarda") || intent.Contains("save"))
            return Task.FromResult<ActionToken?>(MakeToken("INVOKE", "Save", "Save document", 0.85f));

        // Click / open patterns
        if (intent.Contains("click") || intent.Contains("haz clic") || intent.Contains("abre") || intent.Contains("open"))
            return Task.FromResult<ActionToken?>(MakeToken("INVOKE", target, $"Click {context.ElementName}", 0.9f));

        // Search
        if (intent.Contains("buscar") || intent.Contains("search"))
            return Task.FromResult<ActionToken?>(MakeToken("INVOKE", "Search", "Open search", 0.8f));

        // Copy / paste / undo / redo
        if (intent.Contains("copiar") || intent.Contains("copy"))
            return Task.FromResult<ActionToken?>(MakeToken("INVOKE", target, $"Copy from {context.ElementName}", 0.8f));

        if (intent.Contains("pegar") || intent.Contains("paste"))
            return Task.FromResult<ActionToken?>(MakeToken("INVOKE", target, $"Paste into {context.ElementName}", 0.8f));

        if (intent.Contains("deshacer") || intent.Contains("undo"))
            return Task.FromResult<ActionToken?>(MakeToken("INVOKE", "Undo", "Undo last action", 0.85f));

        if (intent.Contains("rehacer") || intent.Contains("redo"))
            return Task.FromResult<ActionToken?>(MakeToken("INVOKE", "Redo", "Redo last action", 0.85f));

        // Toggle / check / enable / disable
        if (intent.Contains("activa") || intent.Contains("enable") || intent.Contains("on"))
            return Task.FromResult<ActionToken?>(MakeToken("TOGGLE", target, $"Enable {context.ElementName}", 0.8f));

        if (intent.Contains("desactiva") || intent.Contains("disable") || intent.Contains("off"))
            return Task.FromResult<ActionToken?>(MakeToken("TOGGLE", target, $"Disable {context.ElementName}", 0.8f));

        // Scroll / navigate
        if (intent.Contains("scroll") || intent.Contains("baja") || intent.Contains("down"))
            return Task.FromResult<ActionToken?>(MakeToken("SCROLL", target, "Scroll down", 0.75f));

        if (intent.Contains("sube") || intent.Contains("up"))
            return Task.FromResult<ActionToken?>(MakeToken("SCROLL", target, "Scroll up", 0.75f));

        // Delete / remove
        if (intent.Contains("borra") || intent.Contains("elimina") || intent.Contains("delete") || intent.Contains("remove"))
            return Task.FromResult<ActionToken?>(MakeToken("INVOKE", "Delete", "Delete selected", 0.8f));

        // Print
        if (intent.Contains("imprimir") || intent.Contains("print"))
            return Task.FromResult<ActionToken?>(MakeToken("INVOKE", "Print", "Print document", 0.85f));

        // Type / write
        if (intent.Contains("escribe") || intent.Contains("type") || intent.Contains("escribir"))
        {
            var text = ExtractQuotedText(intent);
            return Task.FromResult<ActionToken?>(MakeToken("TYPE", target, $"Type: {text}", 0.8f, text));
        }

        // Select all
        if (intent.Contains("selecciona todo") || intent.Contains("select all"))
            return Task.FromResult<ActionToken?>(MakeToken("INVOKE", target, "Select all", 0.8f));

        // Close / exit
        if (intent.Contains("cierra") || intent.Contains("close") || intent.Contains("salir") || intent.Contains("exit"))
            return Task.FromResult<ActionToken?>(MakeToken("INVOKE", "Close", "Close window", 0.8f));

        // Generic invoke fallback for known element
        if (!string.IsNullOrEmpty(target))
            return Task.FromResult<ActionToken?>(MakeToken("INVOKE", target, $"Activate {context.ElementName}", 0.6f));

        return Task.FromResult<ActionToken?>(null);
    }

    private static ActionToken MakeToken(string action, string target, string description, float confidence, string? text = null)
    {
        return new ActionToken
        {
            Action = action,
            Target = target,
            Description = description,
            Confidence = confidence,
            Text = text
        };
    }

    private static string ExtractQuotedText(string input)
    {
        var start = input.IndexOf('"');
        var end = input.LastIndexOf('"');
        if (start >= 0 && end > start)
            return input.Substring(start + 1, end - start - 1);

        start = input.IndexOf('\u00AB'); // «
        end = input.LastIndexOf('\u00BB'); // »
        if (start >= 0 && end > start)
            return input.Substring(start + 1, end - start - 1);

        return input;
    }

    private void Cleanup()
    {
        // Stop audio capture
        _ = _audioCapture.StopAsync();

        // Force GC to keep overhead minimal
        GC.Collect(0, GCCollectionMode.Optimized);
        GC.WaitForPendingFinalizers();

        StatusChanged?.Invoke("Ready — waiting for gaze");
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _gazeTracker.FocusConfirmed -= OnFocusConfirmed;
            _gazeTracker.FocusLost -= OnFocusLost;
            _audioCapture.VoiceStopped -= OnVoiceStopped;

            _gazeTracker.Dispose();
            _audioCapture.Dispose();

            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}

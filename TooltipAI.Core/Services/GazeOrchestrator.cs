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
        // In production, this would call a local LLM or cloud AI service
        // For now, implement simple intent parsing

        var intent = context.UserIntent.ToLowerInvariant();

        if (intent.Contains("export") || intent.Contains("exporta"))
        {
            return Task.FromResult<ActionToken?>(new ActionToken
            {
                Action = "INVOKE",
                Target = context.AutomationId,
                Description = $"Export from {context.ElementName}",
                Confidence = 0.85f
            });
        }

        if (intent.Contains("click") || intent.Contains("haz clic") || intent.Contains("abre"))
        {
            return Task.FromResult<ActionToken?>(new ActionToken
            {
                Action = "INVOKE",
                Target = context.AutomationId,
                Description = $"Click {context.ElementName}",
                Confidence = 0.9f
            });
        }

        if (intent.Contains("guarda") || intent.Contains("save"))
        {
            return Task.FromResult<ActionToken?>(new ActionToken
            {
                Action = "INVOKE",
                Target = "Save",
                Description = "Save document",
                Confidence = 0.85f
            });
        }

        if (intent.Contains("buscar") || intent.Contains("search"))
        {
            return Task.FromResult<ActionToken?>(new ActionToken
            {
                Action = "INVOKE",
                Target = "Search",
                Description = "Open search",
                Confidence = 0.8f
            });
        }

        return Task.FromResult<ActionToken?>(null);
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

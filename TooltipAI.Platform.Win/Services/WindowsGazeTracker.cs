using System.Diagnostics;
using System.Runtime.InteropServices;
using TooltipAI.Core.Interfaces;
using TooltipAI.Core.Models;

namespace TooltipAI.Platform.Win.Services;

/// <summary>
/// Windows gaze/focus tracker using Eye Control API or fallback to accessibility tree scanning.
/// Implements dwell-time filtering to confirm focus on UI elements.
/// </summary>
public sealed class WindowsGazeTracker : IGazeTracker
{
    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(POINT point);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    private readonly IUIAutomationService _uiaService;
    private readonly System.Threading.Timer _dwellTimer;
    private readonly System.Threading.Timer _pollTimer;

    private POINT _lastPoint;
    private POINT _stablePoint;
    private DateTime _stableSince;
    private bool _focusConfirmed;
    private ElementInfo? _currentElement;
    private bool _disposed;

    public event Action<ElementInfo>? FocusConfirmed;
    public event Action? FocusLost;

    public ElementInfo? CurrentElement => _currentElement;
    public bool IsTracking { get; private set; }
    public int DwellTimeMs { get; set; } = 150;

    public WindowsGazeTracker(IUIAutomationService uiaService)
    {
        _uiaService = uiaService;

        _dwellTimer = new System.Threading.Timer(DwellTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        _pollTimer = new System.Threading.Timer(PollCallback, null, Timeout.Infinite, Timeout.Infinite);
    }

    public Task StartAsync()
    {
        if (IsTracking) return Task.CompletedTask;

        IsTracking = true;
        _focusConfirmed = false;
        _stableSince = DateTime.UtcNow;

        // Poll cursor position every 16ms (~60fps)
        _pollTimer.Change(0, 16);

        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        if (!IsTracking) return Task.CompletedTask;

        IsTracking = false;
        _pollTimer.Change(Timeout.Infinite, Timeout.Infinite);
        _dwellTimer.Change(Timeout.Infinite, Timeout.Infinite);

        if (_currentElement != null)
        {
            _currentElement = null;
            FocusLost?.Invoke();
        }

        return Task.CompletedTask;
    }

    private void PollCallback(object? state)
    {
        if (!IsTracking || _disposed) return;

        try
        {
            GetCursorPos(out var currentPoint);

            // Check if cursor moved significantly (threshold: 5 pixels)
            if (Math.Abs(currentPoint.X - _lastPoint.X) > 5 ||
                Math.Abs(currentPoint.Y - _lastPoint.Y) > 5)
            {
                // Cursor moved — reset stability
                _stablePoint = currentPoint;
                _stableSince = DateTime.UtcNow;
                _focusConfirmed = false;
                _lastPoint = currentPoint;

                // Start dwell timer
                _dwellTimer.Change(DwellTimeMs, Timeout.Infinite);
            }
            else
            {
                // Cursor stable — check dwell time
                var elapsed = (DateTime.UtcNow - _stableSince).TotalMilliseconds;
                if (elapsed >= DwellTimeMs && !_focusConfirmed)
                {
                    // Dwell time reached — confirm focus
                    ConfirmFocus(currentPoint);
                }
            }
        }
        catch (ObjectDisposedException)
        {
            // Timer disposed during callback
        }
    }

    private void DwellTimerCallback(object? state)
    {
        if (!IsTracking || _disposed) return;

        try
        {
            var elapsed = (DateTime.UtcNow - _stableSince).TotalMilliseconds;
            if (elapsed >= DwellTimeMs && !_focusConfirmed)
            {
                GetCursorPos(out var point);
                ConfirmFocus(point);
            }
        }
        catch (ObjectDisposedException)
        {
            // Timer disposed during callback
        }
    }

    private void ConfirmFocus(POINT point)
    {
        try
        {
            // Get element from point using UI Automation
            var element = _uiaService.GetElementFromPoint(point.X, point.Y);

            if (element != null)
            {
                // Check if this is a different element than current
                if (_currentElement == null ||
                    _currentElement.AutomationId != element.AutomationId ||
                    _currentElement.Name != element.Name)
                {
                    // New element — confirm focus
                    _currentElement = element;
                    _focusConfirmed = true;
                    FocusConfirmed?.Invoke(element);
                }
            }
            else
            {
                // No element found — clear focus
                if (_currentElement != null)
                {
                    _currentElement = null;
                    _focusConfirmed = false;
                    FocusLost?.Invoke();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GazeTracker error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _dwellTimer?.Dispose();
            _pollTimer?.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}

using System.Runtime.InteropServices;
using TooltipAI.Core.Interfaces;
using TooltipAI.Core.Models;
using TooltipAI.Core.Services;
using TooltipAI.Service.Services;

namespace TooltipAI.Service.Workers;

public class MouseMonitorWorker
{
    private readonly IUIAutomationService _uiaService;
    private readonly NamedPipeService _pipeService;
    private readonly MultiMonitorService _multiMonitor;
    private readonly LoggingService _logger;
    private readonly HybridAiService _aiService;
    private readonly SoftwareCategoryClassifier _classifier;
    private readonly object _lock = new();
    private readonly VisualizationDataService _vizService;
    private string _lastElement = string.Empty;
    private DateTime _lastUpdate = DateTime.MinValue;
    private DateTime _lastActivity = DateTime.UtcNow;
    private int _currentThrottleMs = 50;
    private const int ActiveThrottleMs = 50;
    private const int IdleThrottleMs = 200;
    private const int IdleThresholdMs = 5000;
    private Thread? _hookThread;
    private CancellationTokenSource? _cts;

    public MouseMonitorWorker(
        IUIAutomationService uiaService,
        NamedPipeService pipeService,
        MultiMonitorService multiMonitor,
        LoggingService logger,
        HybridAiService aiService)
    {
        _uiaService = uiaService;
        _pipeService = pipeService;
        _multiMonitor = multiMonitor;
        _logger = logger;
        _aiService = aiService;
        _classifier = new SoftwareCategoryClassifier();
        _vizService = new VisualizationDataService();
    }

    public void Start()
    {
        _cts = new CancellationTokenSource();
        _hookThread = new Thread(() => RunHookLoop(_cts.Token));
        _hookThread.IsBackground = true;
        _hookThread.Start();
    }

    public void Stop()
    {
        _cts?.Cancel();
        _hookThread?.Join(2000);
    }

    private void RunHookLoop(CancellationToken ct)
    {
        MouseHookService? mouseHook = null;

        try
        {
            _logger.LogInfo("Hook thread starting...", "MouseMonitor");
            mouseHook = new MouseHookService(OnMouseMove);
            mouseHook.Start();
            _logger.LogInfo("Mouse hook installed successfully", "MouseMonitor");

            NativeMethods.MSG msg;
            while (!ct.IsCancellationRequested)
            {
                while (NativeMethods.PeekMessage(out msg, IntPtr.Zero, 0, 0, 1))
                {
                    NativeMethods.TranslateMessage(ref msg);
                    NativeMethods.DispatchMessage(ref msg);
                }
                Thread.Sleep(1);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Hook thread exception: {ex.Message}", ex, "MouseMonitor");
        }
        finally
        {
            mouseHook?.Dispose();
            _logger.LogInfo("Hook thread exiting", "MouseMonitor");
        }
    }

    private async void OnMouseMove(int x, int y)
    {
        try
        {
            var now = DateTime.UtcNow;

            var timeSinceLastActivity = (now - _lastActivity).TotalMilliseconds;
            _currentThrottleMs = timeSinceLastActivity > IdleThresholdMs ? IdleThrottleMs : ActiveThrottleMs;
            _lastActivity = now;

            if ((now - _lastUpdate).TotalMilliseconds < _currentThrottleMs)
                return;

            _lastUpdate = now;

            var hWnd = WindowFromPoint(new NativeMethods.POINT { X = x, Y = y });
            if (hWnd == IntPtr.Zero) return;

            var nameLength = GetWindowTextLength(hWnd);
            var nameBuilder = new System.Text.StringBuilder(nameLength + 1);
            GetWindowText(hWnd, nameBuilder, nameBuilder.Capacity);
            var windowTitle = nameBuilder.ToString();

            var classNameBuilder = new System.Text.StringBuilder(256);
            GetClassName(hWnd, classNameBuilder, classNameBuilder.Capacity);
            var className = classNameBuilder.ToString();

            var processName = _classifier.GetProcessNameFromHwnd(hWnd);

            var category = _classifier.Classify(className, windowTitle, processName ?? "");

            var element = _uiaService.GetElementFromPoint(x, y);
            var currentKey = element?.AutomationId ?? element?.Name ?? windowTitle ?? string.Empty;

            lock (_lock)
            {
                if (currentKey == _lastElement || string.IsNullOrEmpty(currentKey))
                    return;
                _lastElement = currentKey;
            }

            if (element is null)
            {
                element = new ElementInfo
                {
                    Name = windowTitle,
                    ControlType = "Window",
                    ClassName = className,
                    IsEnabled = true,
                    HelpText = string.Empty,
                    AutomationId = string.Empty,
                    IsKeyboardFocusable = true
                };
            }

            var theme = TooltipColorTheme.GetTheme(category);

            var tooltipData = new TooltipData
            {
                Element = element,
                AiContext = _aiService.GetLocalContext(element),
                AiDescription = _aiService.GetLocalDescription(element),
                SoftwareCategory = category.ToString(),
                CategoryLabel = theme.CategoryLabel,
                GestureHint = _aiService.GetGestureHint(element, category),
                QualityTip = _aiService.GetQualityTip(element, category),
                MoveGuide = _aiService.GetMoveGuide(element, category),
                DataInsight = _aiService.GetDataInsight(element, category),
                ProcessName = processName,
                WindowTitle = windowTitle,
                BorderColor = theme.BorderPrimary,
                AccentColor = theme.AccentColor,
                GlowColor = theme.GlowColor
            };

            _vizService.PopulateVisualization(tooltipData, category, processName ?? "Unknown");

            await _pipeService.SendTooltipDataAsync(tooltipData, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError($"OnMouseMove error: {ex.Message}", ex, "MouseMonitor");
        }
    }

    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(NativeMethods.POINT point);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);

    private static class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT { public int X, Y; }

        [DllImport("user32.dll")]
        public static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        [DllImport("user32.dll")]
        public static extern bool TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        public static extern IntPtr DispatchMessage(ref MSG lpMsg);
    }
}

using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace TooltipAI.Core.Hotkeys;

/// <summary>
/// Manages global hotkeys for Tooltip AI.
/// Ctrl+Shift+T: Toggle between Tooltip and Translate modes
/// </summary>
public sealed class GlobalHotkeyManager : IDisposable
{
    private readonly ILogger<GlobalHotkeyManager> _logger;
    private IntPtr _hookId = IntPtr.Zero;
    private bool _isTranslateMode = false;
    private bool _isRegistered = false;

    public event Action<HotkeyEvent>? HotkeyPressed;
    public bool IsTranslateMode => _isTranslateMode;

    public GlobalHotkeyManager(ILogger<GlobalHotkeyManager> logger)
    {
        _logger = logger;
    }

    public bool Register()
    {
        if (_isRegistered)
            return true;

        try
        {
            _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, HookCallback, IntPtr.Zero, 0);
            _isRegistered = _hookId != IntPtr.Zero;

            if (_isRegistered)
                _logger.LogInformation("Global hotkey registered: Ctrl+Shift+T");
            else
                _logger.LogWarning("Failed to register global hotkey");

            return _isRegistered;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering hotkey");
            return false;
        }
    }

    public void Unregister()
    {
        if (_hookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
            _isRegistered = false;
        }
    }

    public bool ToggleMode()
    {
        _isTranslateMode = !_isTranslateMode;
        _logger.LogInformation("Mode toggled: {Mode}", _isTranslateMode ? "Translate" : "Tooltip");
        return _isTranslateMode;
    }

    public void SetMode(bool translateMode)
    {
        _isTranslateMode = translateMode;
    }

    public void Dispose()
    {
        Unregister();
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            var kbStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
            int vkCode = (int)kbStruct.vkCode;

            // Check for Ctrl+Shift+T
            bool ctrlPressed = (GetKeyState(VK_CONTROL) & 0x8000) != 0;
            bool shiftPressed = (GetKeyState(VK_SHIFT) & 0x8000) != 0;
            bool tPressed = vkCode == VK_T;

            if (ctrlPressed && shiftPressed && tPressed)
            {
                ToggleMode();
                HotkeyPressed?.Invoke(new HotkeyEvent
                {
                    Type = HotkeyType.ToggleMode,
                    IsTranslateMode = _isTranslateMode,
                    Timestamp = DateTime.UtcNow
                });
                return (IntPtr)1; // Consume the key
            }
        }

        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    #region P/Invoke

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int VK_CONTROL = 0x11;
    private const int VK_SHIFT = 0x10;
    private const int VK_T = 0x54;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern short GetKeyState(int nVirtKey);

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    #endregion
}

public sealed class HotkeyEvent
{
    public HotkeyType Type { get; init; }
    public bool IsTranslateMode { get; init; }
    public DateTime Timestamp { get; init; }
}

public enum HotkeyType
{
    ToggleMode,
    CopyTranslation,
    ChangeLanguage
}

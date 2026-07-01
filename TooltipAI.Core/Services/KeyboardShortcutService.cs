using System.Runtime.InteropServices;

namespace TooltipAI.Core.Services;

public class KeyboardShortcutService : IDisposable
{
    private IntPtr _hookId = IntPtr.Zero;
    private readonly NativeMethods.HookProc _hookProc;
    private readonly Dictionary<int, Action> _shortcuts = new();
    private bool _isEnabled = true;

    public event Action<bool>? OnToggleStateChanged;

    public KeyboardShortcutService()
    {
        _hookProc = HookCallback;
    }

    public void RegisterShortcut(int keyCode, Action action)
    {
        _shortcuts[keyCode] = action;
    }

    public void RegisterDefaultShortcuts()
    {
        // Ctrl+Alt+T: Toggle tooltip on/off
        RegisterShortcut(0x54, () => // T key
        {
            _isEnabled = !_isEnabled;
            OnToggleStateChanged?.Invoke(_isEnabled);
        });
    }

    public void Start()
    {
        _hookId = NativeMethods.SetWindowsHookEx(13, _hookProc, IntPtr.Zero, 0);
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)0x0100) // WM_KEYDOWN
        {
            var hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
            var modifiers = GetCurrentModifiers();
            var keyWithModifiers = (int)(modifiers | (int)hookStruct.vkCode);

            if (_shortcuts.TryGetValue(keyWithModifiers, out var action))
            {
                action();
                return (IntPtr)1; // Suppress key
            }
        }
        return NativeMethods.CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private int GetCurrentModifiers()
    {
        int modifiers = 0;
        if ((NativeMethods.GetAsyncKeyState(0x10) & 0x8000) != 0) modifiers |= 0x10; // Shift
        if ((NativeMethods.GetAsyncKeyState(0x11) & 0x8000) != 0) modifiers |= 0x11; // Ctrl
        if ((NativeMethods.GetAsyncKeyState(0x12) & 0x8000) != 0) modifiers |= 0x12; // Alt
        return modifiers;
    }

    public void Dispose()
    {
        if (_hookId != IntPtr.Zero)
        {
            NativeMethods.UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    private static class NativeMethods
    {
        public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(int vKey);
    }
}

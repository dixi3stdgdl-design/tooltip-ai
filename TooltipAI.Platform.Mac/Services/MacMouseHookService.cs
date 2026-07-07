using System.Runtime.InteropServices;
using TooltipAI.Core.Interfaces;

namespace TooltipAI.Platform.Mac.Services;

/// <summary>
/// macOS mouse hook using CGEventTap (Core Graphics).
/// Intercepts mouse movement at the kernel level via Accessibility APIs.
/// </summary>
public sealed class MacMouseHookService : IMouseHookService, IDisposable
{
    private IntPtr _eventTap;
    private IntPtr _runLoopSource;
    private Action<int, int>? _onMouseMove;
    private bool _disposed;

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern IntPtr CGEventTapCreate(
        CGEventTapLocation tapLocation,
        CGEventTapPlacement tapPlacement,
        CGEventTapOption tapOption,
        ulong eventsOfInterest,
        CGEventTapCallBack callback,
        IntPtr userInfo);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern void CGEventTapEnable(IntPtr tap, bool enable);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern void CFRunLoopAddSource(IntPtr runLoop, IntPtr source, IntPtr runLoopMode);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern IntPtr CFRunLoopGetCurrent();

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern IntPtr CFMachPortCreateRunLoopSource(IntPtr allocator, IntPtr port, int order);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern IntPtr CGEventGetLocation(IntPtr eventRef);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern int CGEventGetIntegerValueField(IntPtr eventRef, int field);

    private delegate IntPtr CGEventTapCallBack(
        IntPtr proxy,
        CGEventType type,
        IntPtr eventRef,
        IntPtr userInfo);

    private CGEventTapCallBack? _callbackDelegate;

    public void Start(Action<int, int> onMouseMove)
    {
        _onMouseMove = onMouseMove;
        _callbackDelegate = EventTapCallback;

        const ulong mouseMovedMask = 1 << 5; // kCGEventMouseMoved
        const ulong leftMouseDownMask = 1 << 1; // kCGEventLeftMouseDown

        _eventTap = CGEventTapCreate(
            CGEventTapLocation.Session,
            CGEventTapPlacement.HeadInsert,
            CGEventTapOption.Default,
            mouseMovedMask | leftMouseDownMask,
            _callbackDelegate,
            IntPtr.Zero);

        if (_eventTap == IntPtr.Zero)
        {
            throw new InvalidOperationException(
                "Failed to create CGEventTap. Ensure Accessibility permissions are granted in System Preferences > Privacy & Security > Accessibility.");
        }

        _runLoopSource = CFMachPortCreateRunLoopSource(IntPtr.Zero, _eventTap, 0);
        var runLoop = CFRunLoopGetCurrent();
        CFRunLoopAddSource(runLoop, _runLoopSource, IntPtr.Zero);
        CGEventTapEnable(_eventTap, true);
    }

    private IntPtr EventTapCallback(
        IntPtr proxy,
        CGEventType type,
        IntPtr eventRef,
        IntPtr userInfo)
    {
        if (type == CGEventType.MouseMoved)
        {
            var location = CGEventGetLocation(eventRef);
            var x = (int)location.X;
            var y = (int)location.Y;

            _onMouseMove?.Invoke(x, y);
        }

        return eventRef;
    }

    public void Stop()
    {
        if (_eventTap != IntPtr.Zero)
        {
            CGEventTapEnable(_eventTap, false);
            _eventTap = IntPtr.Zero;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Stop();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    private enum CGEventTapLocation
    {
        Session = 0,
        AnnotatedSession = 1
    }

    private enum CGEventTapPlacement
    {
        HeadInsert = 0,
        TailAppend = 1
    }

    private enum CGEventTapOption
    {
        Default = 0,
        ListenOnly = 1
    }

    private enum CGEventType
    {
        Null = 0,
        LeftMouseDown = 1,
        LeftMouseUp = 2,
        RightMouseDown = 3,
        RightMouseUp = 4,
        MouseMoved = 5,
        LeftMouseDragged = 6,
        RightMouseDragged = 7,
        KeyDown = 10,
        KeyUp = 11,
        FlagsChanged = 12,
        ScrollWheel = 22,
        TabletPointer = 23,
        TabletProximity = 24,
        OtherMouseDown = 25,
        OtherMouseUp = 26,
        OtherMouseDragged = 27
    }
}

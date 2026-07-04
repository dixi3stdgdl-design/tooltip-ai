using System.Runtime.InteropServices;
using TooltipAI.Core.Models;

namespace TooltipAI.Platform.Mac.Views;

/// <summary>
/// macOS tooltip overlay using AppKit (NSWindow).
/// Renders contextual information in-place next to the cursor.
/// </summary>
public sealed class MacTooltipWindow : IDisposable
{
    private IntPtr _window;
    private IntPtr _titleLabel;
    private IntPtr _contextLabel;
    private bool _disposed;

    [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, float value);

    [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr value);

    [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, bool value);

    [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, float x, float y, float w, float h);

    [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
    private static extern IntPtr sel_registerName(string name);

    [DllImport("/System/Library/Frameworks/objc/libobjc.dylib")]
    private static extern IntPtr objc_getClass(string name);

    [DllImport("/System/Library/Frameworks/objc/libobjc.dylib")]
    private static extern IntPtr class_getInstanceVariable(IntPtr classHandle, string name);

    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    private static extern IntPtr CFStringCreateWithCString(IntPtr allocator, string cStr, int encoding);

    private static readonly IntPtr _nsWindowClass = objc_getClass("NSWindow");
    private static readonly IntPtr _nsTextFieldClass = objc_getClass("NSTextField");
    private static readonly IntPtr _nsLabelClass = objc_getClass("NSTextField");

    private static readonly IntPtr _allocSel = sel_registerName("alloc");
    private static readonly IntPtr _initSel = sel_registerName("initWithContentRect:styleMask:backing:defer:");
    private static readonly IntPtr _setTitleSel = sel_registerName("setTitle:");
    private static readonly IntPtr _orderFrontSel = sel_registerName("orderFront:");
    private static readonly IntPtr _setIsOpaqueSel = sel_registerName("setOpaque:");
    private static readonly IntPtr _setLevelSel = sel_registerName("setLevel:");
    private static readonly IntPtr _setBackgroundColorSel = sel_registerName("setBackgroundColor:");
    private static readonly IntPtr _setFrameOriginSel = sel_registerName("setFrameOrigin:");
    private static readonly IntPtr _setStringValueSel = sel_registerName("setStringValue:");
    private static readonly IntPtr _setFontSizeSel = sel_registerName("setFontSize:");
    private static readonly IntPtr _setBackgroundColorClearSel = sel_registerName("setBackgroundColor:");
    private static readonly IntPtr _setBorderedSel = sel_registerName("setBordered:");
    private static readonly IntPtr _setEditableSel = sel_registerName("setEditable:");
    private static readonly IntPtr _setSelectableSel = sel_registerName("setSelectable:");
    private static readonly IntPtr _addSubviewSel = sel_registerName("addSubview:");
    private static readonly IntPtr _contentViewSel = sel_registerName("contentView");

    public MacTooltipWindow()
    {
        _window = CreateWindow();
        _titleLabel = CreateLabel(14, true);
        _contextLabel = CreateLabel(12, false);

        var contentView = objc_msgSend(_window, _contentViewSel);
        objc_msgSend(contentView, _addSubviewSel, _titleLabel);
        objc_msgSend(contentView, _addSubviewSel, _contextLabel);
    }

    public void Show(TooltipData data, int x, int y)
    {
        var titleText = $"{data.ElementType}: {data.ElementName}";
        var contextText = data.Context;

        var titlePtr = CFStringCreateWithCString(IntPtr.Zero, titleText, 0x08000100);
        var contextPtr = CFStringCreateWithCString(IntPtr.Zero, contextText, 0x08000100);

        objc_msgSend(_titleLabel, _setStringValueSel, titlePtr);
        objc_msgSend(_contextLabel, _setStringValueSel, contextPtr);

        var screenHeight = GetScreenHeight();
        var adjustedX = Math.Min(x + 15, 1600);
        var adjustedY = Math.Max(screenHeight - y - 130, 10);

        objc_msgSend(_window, _setFrameOriginSel, (float)adjustedX, (float)adjustedY, 0, 0);
        objc_msgSend(_window, _orderFrontSel, IntPtr.Zero);
    }

    public void Hide()
    {
        var closeSel = sel_registerName("close");
        objc_msgSend(_window, closeSel);
    }

    private IntPtr CreateWindow()
    {
        var window = objc_msgSend(_nsWindowClass, _allocSel);
        var styleMask = 0; // NSBorderlessWindowMask
        objc_msgSend(window, _initSel,
            (float)0, (float)0, (float)320, (float)120,
            styleMask, 0, false);

        objc_msgSend(window, _setIsOpaqueSel, false);
        objc_msgSend(window, _setLevelSel, (float)4); // NSFloatingWindowLevel

        return window;
    }

    private IntPtr CreateLabel(int fontSize, bool bold)
    {
        var label = objc_msgSend(_nsTextFieldClass, _allocSel);
        objc_msgSend(label, _initSel,
            (float)10, (float)(bold ? 80 : 10), (float)300, (float)(bold ? 20 : 60),
            0, 0, false);

        objc_msgSend(label, _setFontSizeSel, (float)fontSize);
        objc_msgSend(label, _setBorderedSel, false);
        objc_msgSend(label, _setEditableSel, false);
        objc_msgSend(label, _setSelectableSel, false);

        return label;
    }

    private float GetScreenHeight()
    {
        var mainScreenSel = sel_registerName("mainScreen");
        var mainScreen = objc_msgSend(objc_getClass("NSScreen"), mainScreenSel);
        var frameSel = sel_registerName("frame");
        // Simplified — in production use NSRect
        return 1080;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Hide();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}

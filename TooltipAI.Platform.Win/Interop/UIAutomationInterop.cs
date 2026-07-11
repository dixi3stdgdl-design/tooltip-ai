using System.Runtime.InteropServices;

namespace TooltipAI.Platform.Win.Interop;

/// <summary>
/// Raw COM Interop for Windows UI Automation (UIAutomationCore.dll).
/// Uses P/Invoke and COM interfaces directly so the project compiles
/// on any platform (though it only functions on Windows).
/// </summary>
internal static class UIAutomationInterop
{
    private const string UIACoreDll = "UIAutomationCore.dll";

    #region COM Interface GUIDs

    private static readonly Guid CUIAutomation_Clsid = new("FF48DBA4-60EF-4201-AA87-54103EEF594E");
    private static readonly Guid IUIAutomation_Iid = new("30C1081D-CD1C-4C02-9F3B-A9F40AC11C5D");
    private static readonly Guid IUIAutomationElement_Iid = new("D827D3BC-A77E-41CC-8AF7-D6D24957C23E");

    #endregion

    #region COM Structs

    [StructLayout(LayoutKind.Sequential)]
    public struct UIAPoint
    {
        public int x;
        public int y;
    }

    #endregion

    #region COM Interface Definitions

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("30C1081D-CD1C-4C02-9F3B-A9F40AC11C5D")]
    public interface IUIAutomation
    {
        [PreserveSig]
        int ElementFromPoint(UIAPoint pt, out IUIAutomationElement? element);

        [PreserveSig]
        int GetFocusedElement(out IUIAutomationElement? element);

        // Remaining vtable entries (unused) to maintain correct vtable layout
        [PreserveSig]
        int ElementFromHandle(IntPtr hwnd, out IUIAutomationElement? element);
        [PreserveSig]
        int GetRootElement(out IUIAutomationElement? element);
        [PreserveSig]
        int CreatePropertyCondition(int propertyId, object value, out IntPtr condition);
        [PreserveSig]
        int CreateTrueCondition(out IntPtr condition);
        [PreserveSig]
        int CreateFalseCondition(out IntPtr condition);
        [PreserveSig]
        int CreateOrConditionFromConditions(IntPtr[] conditions, int conditionCount, out IntPtr condition);
        [PreserveSig]
        int CreateOrCondition(IntPtr condition1, IntPtr condition2, out IntPtr condition);
        [PreserveSig]
        int CreateAndCondition(IntPtr condition1, IntPtr condition2, out IntPtr condition);
        [PreserveSig]
        int CreateNotCondition(IntPtr condition, out IntPtr notCondition);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("D827D3BC-A77E-41CC-8AF7-D6D24957C23E")]
    public interface IUIAutomationElement
    {
        [PreserveSig]
        int SetFocus();

        [PreserveSig]
        int GetRuntimeId(out int[]? runtimeId);

        [PreserveSig]
        int FindFirst(int scope, IntPtr condition, out IUIAutomationElement? found);

        [PreserveSig]
        int FindAll(int scope, IntPtr condition, out IntPtr elements);

        [PreserveSig]
        int FindFirstBuildCache(int scope, IntPtr condition, IntPtr cacheRequest, out IUIAutomationElement? found);

        [PreserveSig]
        int FindAllBuildCache(int scope, IntPtr condition, IntPtr cacheRequest, out IntPtr elements);

        [PreserveSig]
        int BuildUpdatedCache(IntPtr cacheRequest, out IUIAutomationElement? updatedElement);

        [PreserveSig]
        int GetCurrentPropertyValue(int propertyId, out object? value);

        [PreserveSig]
        int GetCurrentPropertyValueEx(int propertyId, int flags, out object? value);

        [PreserveSig]
        int GetCachedPropertyValue(int propertyId, out object? value);

        [PreserveSig]
        int GetCachedPropertyValueEx(int propertyId, int flags, out object? value);

        [PreserveSig]
        int GetClickablePoint(out UIAPoint point, [MarshalAs(UnmanagedType.Bool)] out bool isClickable);

        [PreserveSig]
        int GetSupportedPatterns(out int[] patternIds, out int patternCount);

        [PreserveSig]
        int GetCachedPatterns(out int[] patternIds, out int patternCount);

        [PreserveSig]
        int GetItem(int patternId, out IntPtr item);

        [PreserveSig]
        int GetCurrentPattern(int patternId, out IntPtr patternObject);

        [PreserveSig]
        int GetCachedPattern(int patternId, out IntPtr patternObject);

        [PreserveSig]
        int DisconnectAllPatterns();

        [PreserveSig]
        int get_CurrentName([MarshalAs(UnmanagedType.BStr)] out string? name);

        [PreserveSig]
        int get_CurrentControlType(out int controlType);

        [PreserveSig]
        int get_CurrentLocalizedControlType([MarshalAs(UnmanagedType.BStr)] out string? localizedType);

        [PreserveSig]
        int get_CurrentHelpText([MarshalAs(UnmanagedType.BStr)] out string? helpText);

        [PreserveSig]
        int get_CurrentAutomationId([MarshalAs(UnmanagedType.BStr)] out string? automationId);

        [PreserveSig]
        int get_CurrentClassName([MarshalAs(UnmanagedType.BStr)] out string? className);

        [PreserveSig]
        int get_CurrentIsEnabled([MarshalAs(UnmanagedType.Bool)] out bool isEnabled);

        [PreserveSig]
        int get_CurrentIsKeyboardFocusable([MarshalAs(UnmanagedType.Bool)] out bool isKeyboardFocusable);

        [PreserveSig]
        int get_CurrentIsOffscreen([MarshalAs(UnmanagedType.Bool)] out bool isOffscreen);

        [PreserveSig]
        int get_CurrentOrientation(out int orientation);

        [PreserveSig]
        int get_CurrentBoundingRectangle(out tagRECT rect);

        [PreserveSig]
        int get_CurrentLabeledBy(out IUIAutomationElement? labeledBy);

        [PreserveSig]
        int get_CurrentAriaRole([MarshalAs(UnmanagedType.BStr)] out string? ariaRole);

        [PreserveSig]
        int get_CurrentAriaProperties([MarshalAs(UnmanagedType.BStr)] out string? ariaProperties);

        [PreserveSig]
        int get_CurrentIsDataValidForForm([MarshalAs(UnmanagedType.Bool)] out bool isValid);

        [PreserveSig]
        int get_CurrentProviderDescription([MarshalAs(UnmanagedType.BStr)] out string? providerDesc);

        [PreserveSig]
        int get_CurrentProcessId(out int processId);

        [PreserveSig]
        int get_CurrentProcessName([MarshalAs(UnmanagedType.BStr)] out string? processName);

        [PreserveSig]
        int get_CurrentAutomationDepth(out int depth);

        [PreserveSig]
        int get_CachedName([MarshalAs(UnmanagedType.BStr)] out string? name);

        [PreserveSig]
        int get_CachedControlType(out int controlType);

        [PreserveSig]
        int get_CachedLocalizedControlType([MarshalAs(UnmanagedType.BStr)] out string? localizedType);

        [PreserveSig]
        int get_CachedHelpText([MarshalAs(UnmanagedType.BStr)] out string? helpText);

        [PreserveSig]
        int get_CachedAutomationId([MarshalAs(UnmanagedType.BStr)] out string? automationId);

        [PreserveSig]
        int get_CachedClassName([MarshalAs(UnmanagedType.BStr)] out string? className);

        [PreserveSig]
        int get_CachedIsEnabled([MarshalAs(UnmanagedType.Bool)] out bool isEnabled);

        [PreserveSig]
        int get_CachedIsKeyboardFocusable([MarshalAs(UnmanagedType.Bool)] out bool isKeyboardFocusable);

        [PreserveSig]
        int get_CachedIsOffscreen([MarshalAs(UnmanagedType.Bool)] out bool isOffscreen);

        [PreserveSig]
        int get_CachedOrientation(out int orientation);

        [PreserveSig]
        int get_CachedBoundingRectangle(out tagRECT rect);

        [PreserveSig]
        int get_CachedLabeledBy(out IUIAutomationElement? labeledBy);

        [PreserveSig]
        int get_CachedAriaRole([MarshalAs(UnmanagedType.BStr)] out string? ariaRole);

        [PreserveSig]
        int get_CachedAriaProperties([MarshalAs(UnmanagedType.BStr)] out string? ariaProperties);

        [PreserveSig]
        int get_CachedIsDataValidForForm([MarshalAs(UnmanagedType.Bool)] out bool isValid);

        [PreserveSig]
        int get_CachedProviderDescription([MarshalAs(UnmanagedType.BStr)] out string? providerDesc);

        [PreserveSig]
        int CachedParent(out IUIAutomationElement? parent);

        [PreserveSig]
        int CachedChildren(out IntPtr children);

        [PreserveSig]
        int get_CurrentItemStatus([MarshalAs(UnmanagedType.BStr)] out string? itemStatus);

        [PreserveSig]
        int get_CurrentItemType([MarshalAs(UnmanagedType.BStr)] out string? itemType);

        [PreserveSig]
        int get_CurrentIsProtected([MarshalAs(UnmanagedType.Bool)] out bool isProtected);

        [PreserveSig]
        int get_CurrentLiveSetting(out int liveSetting);

        [PreserveSig]
        int get_CurrentOptimizeForVisualContent([MarshalAs(UnmanagedType.Bool)] out bool optimize);

        [PreserveSig]
        int get_CachedItemStatus([MarshalAs(UnmanagedType.BStr)] out string? itemStatus);

        [PreserveSig]
        int get_CachedItemType([MarshalAs(UnmanagedType.BStr)] out string? itemType);

        [PreserveSig]
        int get_CachedIsProtected([MarshalAs(UnmanagedType.Bool)] out bool isProtected);

        [PreserveSig]
        int get_CachedLiveSetting(out int liveSetting);

        [PreserveSig]
        int get_CachedOptimizeForVisualContent([MarshalAs(UnmanagedType.Bool)] out bool optimize);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tagRECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    #endregion

    #region Control Type Constants

    public const int UIA_ButtonControlTypeId = 50000;
    public const int UIA_CalendarControlTypeId = 50001;
    public const int UIA_CheckBoxControlTypeId = 50002;
    public const int UIA_ComboBoxControlTypeId = 50003;
    public const int UIA_EditControlTypeId = 50004;
    public const int UIA_HyperlinkControlTypeId = 50005;
    public const int UIA_ImageControlTypeId = 50006;
    public const int UIA_ListItemControlTypeId = 50007;
    public const int UIA_ListControlTypeId = 50008;
    public const int UIA_MenuControlTypeId = 50009;
    public const int UIA_MenuBarControlTypeId = 50010;
    public const int UIA_MenuItemControlTypeId = 50011;
    public const int UIA_ProgressBarControlTypeId = 50012;
    public const int UIA_RadioButtonControlTypeId = 50013;
    public const int UIA_ScrollBarControlTypeId = 50014;
    public const int UIA_SliderControlTypeId = 50015;
    public const int UIA_SpinnerControlTypeId = 50016;
    public const int UIA_StatusBarControlTypeId = 50017;
    public const int UIA_TabControlTypeId = 50018;
    public const int UIA_TabItemControlTypeId = 50019;
    public const int UIA_TextControlTypeId = 50020;
    public const int UIA_ToolBarControlTypeId = 50021;
    public const int UIA_ToolTipControlTypeId = 50022;
    public const int UIA_TreeControlTypeId = 50023;
    public const int UIA_TreeItemControlTypeId = 50024;
    public const int UIA_DataGridControlTypeId = 50028;
    public const int UIA_DataItemControlTypeId = 50029;
    public const int UIA_DocumentControlTypeId = 50030;
    public const int UIA_SplitButtonControlTypeId = 50031;
    public const int UIA_WindowControlTypeId = 50032;
    public const int UIA_PaneControlTypeId = 50033;
    public const int UIA_HeaderControlTypeId = 50034;
    public const int UIA_HeaderItemControlTypeId = 50035;
    public const int UIA_TableControlTypeId = 50036;
    public const int UIA_ThumbControlTypeId = 50037;
    public const int UIA_DataGridRowControlTypeId = 50038;
    public const int UIA_DataGridCellControlTypeId = 50039;
    public const int UIA_GroupControlTypeId = 50040;
    public const int UIA_ThumbControlTypeId2 = 50041;
    public const int UIA_DataGridRowHeaderControlTypeId = 50042;
    public const int UIA_SemanticZoomControlTypeId = 50043;
    public const int UIA_AppBarControlTypeId = 50044;

    #endregion

    #region Property IDs

    public const int UIA_NamePropertyId = 30005;
    public const int UIA_ControlTypePropertyId = 30003;
    public const int UIA_AutomationIdPropertyId = 30011;
    public const int UIA_ClassNamePropertyId = 30012;
    public const int UIA_HelpTextPropertyId = 30013;
    public const int UIA_IsEnabledPropertyId = 30010;
    public const int UIA_IsKeyboardFocusablePropertyId = 30009;
    public const int UIA_IsOffscreenPropertyId = 30022;
    public const int UIA_ProcessIdPropertyId = 30002;
    public const int UIA_BoundingRectanglePropertyId = 30001;

    #endregion

    #region Static Helpers

    private static IUIAutomation? _automation;

    /// <summary>
    /// Gets or creates the UIA automation instance.
    /// </summary>
    public static IUIAutomation GetAutomation()
    {
        if (_automation != null)
            return _automation;

        var type = Type.GetTypeFromCLSID(CUIAutomation_Clsid)
            ?? throw new InvalidOperationException("UIAutomationCore not available on this system.");

        _automation = (IUIAutomation)Activator.CreateInstance(type)
            ?? throw new InvalidOperationException("Failed to create UIAutomation instance.");

        return _automation;
    }

    /// <summary>
    /// Gets the UIA element at the given screen coordinates.
    /// Returns null if no element is found.
    /// </summary>
    public static IUIAutomationElement? ElementFromPoint(int x, int y)
    {
        try
        {
            var automation = GetAutomation();
            var pt = new UIAPoint { x = x, y = y };
            var hr = automation.ElementFromPoint(pt, out var element);
            return hr == 0 ? element : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the currently focused UIA element.
    /// </summary>
    public static IUIAutomationElement? GetFocusedElement()
    {
        try
        {
            var automation = GetAutomation();
            var hr = automation.GetFocusedElement(out var element);
            return hr == 0 ? element : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Safely reads a string property from a UIA element.
    /// </summary>
    public static string GetStringProperty(IUIAutomationElement element, int propertyId)
    {
        try
        {
            var hr = element.GetCurrentPropertyValue(propertyId, out var value);
            return hr == 0 && value is string s ? s : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Safely reads a boolean property from a UIA element.
    /// </summary>
    public static bool GetBoolProperty(IUIAutomationElement element, int propertyId)
    {
        try
        {
            var hr = element.GetCurrentPropertyValue(propertyId, out var value);
            if (hr == 0 && value is bool b)
                return b;
            if (hr == 0 && value is int i)
                return i != 0;
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Maps a UIA ControlType ID to a human-readable string.
    /// </summary>
    public static string MapControlType(int controlTypeId)
    {
        return controlTypeId switch
        {
            UIA_ButtonControlTypeId => "Button",
            UIA_CalendarControlTypeId => "Calendar",
            UIA_CheckBoxControlTypeId => "CheckBox",
            UIA_ComboBoxControlTypeId => "ComboBox",
            UIA_EditControlTypeId => "Edit",
            UIA_HyperlinkControlTypeId => "Hyperlink",
            UIA_ImageControlTypeId => "Image",
            UIA_ListItemControlTypeId => "ListItem",
            UIA_ListControlTypeId => "List",
            UIA_MenuControlTypeId => "Menu",
            UIA_MenuBarControlTypeId => "MenuBar",
            UIA_MenuItemControlTypeId => "MenuItem",
            UIA_ProgressBarControlTypeId => "ProgressBar",
            UIA_RadioButtonControlTypeId => "RadioButton",
            UIA_ScrollBarControlTypeId => "ScrollBar",
            UIA_SliderControlTypeId => "Slider",
            UIA_SpinnerControlTypeId => "Spinner",
            UIA_StatusBarControlTypeId => "StatusBar",
            UIA_TabControlTypeId => "Tab",
            UIA_TabItemControlTypeId => "TabItem",
            UIA_TextControlTypeId => "Text",
            UIA_ToolBarControlTypeId => "ToolBar",
            UIA_ToolTipControlTypeId => "ToolTip",
            UIA_TreeControlTypeId => "Tree",
            UIA_TreeItemControlTypeId => "TreeItem",
            UIA_DataGridControlTypeId => "DataGrid",
            UIA_DataItemControlTypeId => "DataItem",
            UIA_DocumentControlTypeId => "Document",
            UIA_SplitButtonControlTypeId => "SplitButton",
            UIA_WindowControlTypeId => "Window",
            UIA_PaneControlTypeId => "Pane",
            UIA_HeaderControlTypeId => "Header",
            UIA_HeaderItemControlTypeId => "HeaderItem",
            UIA_TableControlTypeId => "Table",
            UIA_ThumbControlTypeId => "Thumb",
            UIA_DataGridRowControlTypeId => "DataGridRow",
            UIA_DataGridCellControlTypeId => "DataGridCell",
            UIA_GroupControlTypeId => "Group",
            UIA_SemanticZoomControlTypeId => "SemanticZoom",
            UIA_AppBarControlTypeId => "AppBar",
            _ => "Unknown"
        };
    }

    #endregion
}

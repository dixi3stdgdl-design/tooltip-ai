using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TooltipAI.Core.Services;

public enum SoftwareCategory
{
    Unknown,
    WindowsSystem,
    WindowsShell,
    Development,
    Creative,
    Audio,
    Video,
    Browser,
    Terminal,
    Office,
    Gaming,
    Security,
    Network,
    FileExplorer,
    Settings,
    TextEditor
}

public class SoftwareCategoryClassifier
{
    private static readonly Dictionary<string, SoftwareCategory> ClassNameMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Shell_TrayWnd"] = SoftwareCategory.WindowsShell,
        ["Progman"] = SoftwareCategory.WindowsShell,
        ["WorkerW"] = SoftwareCategory.WindowsShell,
        ["CabinetWClass"] = SoftwareCategory.FileExplorer,
        ["ExploreWClass"] = SoftwareCategory.FileExplorer,
        ["Notepad"] = SoftwareCategory.TextEditor,
        ["WordPadClass"] = SoftwareCategory.TextEditor,
        ["MSPaintApp"] = SoftwareCategory.Creative,
        ["MMCMainFrame"] = SoftwareCategory.WindowsSystem,
        ["#32770"] = SoftwareCategory.WindowsSystem,
        ["Shell_DocObjectView"] = SoftwareCategory.FileExplorer,
        ["TreeView"] = SoftwareCategory.FileExplorer,
        ["SysListView32"] = SoftwareCategory.FileExplorer,
        ["Edit"] = SoftwareCategory.TextEditor,
        ["RICHEDIT50W"] = SoftwareCategory.TextEditor,
        ["Scintilla"] = SoftwareCategory.TextEditor,
        ["Chrome_WidgetWin_1"] = SoftwareCategory.Browser,
        ["MozillaWindowClass"] = SoftwareCategory.Browser,
        ["ApplicationFrameWindow"] = SoftwareCategory.WindowsSystem,
        ["Windows.UI.Core.CoreWindow"] = SoftwareCategory.WindowsSystem,
        ["XamlWindow"] = SoftwareCategory.WindowsSystem,
        ["ConsoleWindowClass"] = SoftwareCategory.Terminal,
        ["Tab_Auto_Suggest"] = SoftwareCategory.Terminal,
    };

    private static readonly Dictionary<string, SoftwareCategory> ProcessNameMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["explorer"] = SoftwareCategory.FileExplorer,
        ["notepad"] = SoftwareCategory.TextEditor,
        ["notepad++"] = SoftwareCategory.TextEditor,
        ["code"] = SoftwareCategory.Development,
        ["devenv"] = SoftwareCategory.Development,
        ["rider64"] = SoftwareCategory.Development,
        ["cursor"] = SoftwareCategory.Development,
        ["sublime_text"] = SoftwareCategory.TextEditor,
        ["atom"] = SoftwareCategory.TextEditor,
        ["chrome"] = SoftwareCategory.Browser,
        ["firefox"] = SoftwareCategory.Browser,
        ["msedge"] = SoftwareCategory.Browser,
        ["opera"] = SoftwareCategory.Browser,
        ["brave"] = SoftwareCategory.Browser,
        ["spotify"] = SoftwareCategory.Audio,
        ["audacity"] = SoftwareCategory.Audio,
        ["audition"] = SoftwareCategory.Audio,
        ["reaper"] = SoftwareCategory.Audio,
        ["fl studio"] = SoftwareCategory.Audio,
        ["ableton"] = SoftwareCategory.Audio,
        ["reason"] = SoftwareCategory.Audio,
        ["logic pro"] = SoftwareCategory.Audio,
        ["cubase"] = SoftwareCategory.Audio,
        ["presonus"] = SoftwareCategory.Audio,
        ["photoshop"] = SoftwareCategory.Creative,
        ["illustrator"] = SoftwareCategory.Creative,
        ["canva"] = SoftwareCategory.Creative,
        ["inkscape"] = SoftwareCategory.Creative,
        ["gimp"] = SoftwareCategory.Creative,
        ["figma"] = SoftwareCategory.Creative,
        ["blender"] = SoftwareCategory.Creative,
        ["maya"] = SoftwareCategory.Creative,
        ["premiere"] = SoftwareCategory.Video,
        ["aftereffects"] = SoftwareCategory.Video,
        ["davinci"] = SoftwareCategory.Video,
        ["resolve"] = SoftwareCategory.Video,
        ["obs64"] = SoftwareCategory.Video,
        ["obs32"] = SoftwareCategory.Video,
        ["vlc"] = SoftwareCategory.Video,
        ["mpv"] = SoftwareCategory.Video,
        ["iina"] = SoftwareCategory.Video,
        ["potplayer"] = SoftwareCategory.Video,
        ["obsidian"] = SoftwareCategory.TextEditor,
        ["terminal"] = SoftwareCategory.Terminal,
        ["cmd"] = SoftwareCategory.Terminal,
        ["powershell"] = SoftwareCategory.Terminal,
        ["wt"] = SoftwareCategory.Terminal,
        ["alacritty"] = SoftwareCategory.Terminal,
        ["wezterm"] = SoftwareCategory.Terminal,
        ["word"] = SoftwareCategory.Office,
        ["excel"] = SoftwareCategory.Office,
        ["powerpoint"] = SoftwareCategory.Office,
        ["outlook"] = SoftwareCategory.Office,
        ["teams"] = SoftwareCategory.Office,
        ["slack"] = SoftwareCategory.Office,
        ["discord"] = SoftwareCategory.Office,
        ["steam"] = SoftwareCategory.Gaming,
        ["epicgameslauncher"] = SoftwareCategory.Gaming,
        ["valorant"] = SoftwareCategory.Gaming,
        ["cs2"] = SoftwareCategory.Gaming,
        ["taskmgr"] = SoftwareCategory.WindowsSystem,
        ["mspaint"] = SoftwareCategory.Creative,
        ["snippingtool"] = SoftwareCategory.Creative,
        ["calc"] = SoftwareCategory.WindowsSystem,
        ["mssettings"] = SoftwareCategory.Settings,
        ["control"] = SoftwareCategory.Settings,
    };

    private static readonly Dictionary<string, SoftwareCategory> TitleKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["visual studio"] = SoftwareCategory.Development,
        ["vs code"] = SoftwareCategory.Development,
        ["intellij"] = SoftwareCategory.Development,
        ["pycharm"] = SoftwareCategory.Development,
        ["android studio"] = SoftwareCategory.Development,
        ["webstorm"] = SoftwareCategory.Development,
        ["github"] = SoftwareCategory.Development,
        ["git"] = SoftwareCategory.Development,
        ["docker"] = SoftwareCategory.Development,
        ["kubernetes"] = SoftwareCategory.Development,
        ["terminal"] = SoftwareCategory.Terminal,
        ["powershell"] = SoftwareCategory.Terminal,
        ["command"] = SoftwareCategory.Terminal,
        ["bash"] = SoftwareCategory.Terminal,
        ["zsh"] = SoftwareCategory.Terminal,
        ["reason"] = SoftwareCategory.Audio,
        ["propellerhead"] = SoftwareCategory.Audio,
        ["korg"] = SoftwareCategory.Audio,
        ["roland"] = SoftwareCategory.Audio,
        ["moog"] = SoftwareCategory.Audio,
        ["ableton"] = SoftwareCategory.Audio,
        ["fl studio"] = SoftwareCategory.Audio,
        ["cubase"] = SoftwareCategory.Audio,
        ["protools"] = SoftwareCategory.Audio,
        ["logic"] = SoftwareCategory.Audio,
        ["photoshop"] = SoftwareCategory.Creative,
        ["illustrator"] = SoftwareCategory.Creative,
        ["premiere"] = SoftwareCategory.Video,
        ["after effects"] = SoftwareCategory.Video,
        ["obs studio"] = SoftwareCategory.Video,
        ["obs"] = SoftwareCategory.Video,
        ["vlc"] = SoftwareCategory.Video,
        ["davinci resolve"] = SoftwareCategory.Video,
        ["media player"] = SoftwareCategory.Video,
        ["blender"] = SoftwareCategory.Creative,
        ["maya"] = SoftwareCategory.Creative,
        ["windows security"] = SoftwareCategory.Security,
        ["firewall"] = SoftwareCategory.Security,
        ["vpn"] = SoftwareCategory.Network,
        ["network"] = SoftwareCategory.Network,
        ["wifi"] = SoftwareCategory.Network,
        ["paint"] = SoftwareCategory.Creative,
        ["snipping"] = SoftwareCategory.Creative,
    };

    public SoftwareCategory Classify(string className, string windowTitle, string processName)
    {
        if (!string.IsNullOrEmpty(className) && ClassNameMap.TryGetValue(className, out var classCat))
            return classCat;

        if (!string.IsNullOrEmpty(processName))
        {
            var procBase = Path.GetFileNameWithoutExtension(processName);
            if (ProcessNameMap.TryGetValue(procBase, out var procCat))
                return procCat;
        }

        if (!string.IsNullOrEmpty(windowTitle))
        {
            foreach (var kvp in TitleKeywords)
            {
                if (windowTitle.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                    return kvp.Value;
            }
        }

        if (!string.IsNullOrEmpty(className))
        {
            if (className.Contains("Chrome", StringComparison.OrdinalIgnoreCase) ||
                className.Contains("Firefox", StringComparison.OrdinalIgnoreCase))
                return SoftwareCategory.Browser;

            if (className.Contains("Terminal", StringComparison.OrdinalIgnoreCase) ||
                className.Contains("Console", StringComparison.OrdinalIgnoreCase))
                return SoftwareCategory.Terminal;
        }

        return SoftwareCategory.Unknown;
    }

    public static string GetCategoryDisplayName(SoftwareCategory category)
    {
        return category switch
        {
            SoftwareCategory.WindowsSystem => "Windows System",
            SoftwareCategory.WindowsShell => "Windows Shell",
            SoftwareCategory.Development => "Development",
            SoftwareCategory.Creative => "Creative",
            SoftwareCategory.Audio => "Audio",
            SoftwareCategory.Video => "Video",
            SoftwareCategory.Browser => "Browser",
            SoftwareCategory.Terminal => "Terminal",
            SoftwareCategory.Office => "Office",
            SoftwareCategory.Gaming => "Gaming",
            SoftwareCategory.Security => "Security",
            SoftwareCategory.Network => "Network",
            SoftwareCategory.FileExplorer => "File Explorer",
            SoftwareCategory.Settings => "Settings",
            SoftwareCategory.TextEditor => "Text Editor",
            _ => "Unknown"
        };
    }

    public string? GetProcessNameFromHwnd(IntPtr hwnd)
    {
        try
        {
            GetWindowThreadProcessId(hwnd, out var processId);
            if (processId == 0) return null;
            var process = Process.GetProcessById((int)processId);
            return process.ProcessName;
        }
        catch
        {
            return null;
        }
    }

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
}

namespace TooltipAI.Core.Services;

public class TooltipColorTheme
{
    public uint BorderPrimary { get; set; }
    public uint BorderSecondary { get; set; }
    public uint BackgroundStart { get; set; }
    public uint BackgroundEnd { get; set; }
    public uint TitleColor { get; set; }
    public uint AccentColor { get; set; }
    public uint GlowColor { get; set; }
    public uint SeparatorColor { get; set; }
    public string CategoryLabel { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = string.Empty;
    public string FontFamily { get; set; } = "Consolas";
    public uint GlowOuter { get; set; }
    public uint PanelBackground { get; set; }

    private static readonly Dictionary<SoftwareCategory, TooltipColorTheme> Themes = new()
    {
        [SoftwareCategory.WindowsSystem] = new()
        {
            BorderPrimary = 0x00E0E0E0,
            BorderSecondary = 0x00A0A0A0,
            BackgroundStart = 0x001A1A2E,
            BackgroundEnd = 0x0016213E,
            TitleColor = 0x00FFFFFF,
            AccentColor = 0x0000D4FF,
            GlowColor = 0x0000D4FF,
            SeparatorColor = 0x0000D4FF,
            CategoryLabel = "WINDOWS",
            CategoryIcon = "\u25A0",
            GlowOuter = 0x000088AA,
            PanelBackground = 0x000A0A12
        },
        [SoftwareCategory.WindowsShell] = new()
        {
            BorderPrimary = 0x004CC9F0,
            BorderSecondary = 0x004895EF,
            BackgroundStart = 0x000A1628,
            BackgroundEnd = 0x000D2137,
            TitleColor = 0x00FFFFFF,
            AccentColor = 0x004CC9F0,
            GlowColor = 0x004CC9F0,
            SeparatorColor = 0x004CC9F0,
            CategoryLabel = "SHELL",
            CategoryIcon = "\u25CB",
            GlowOuter = 0x002A6A88,
            PanelBackground = 0x000A0A12
        },
        [SoftwareCategory.Development] = new()
        {
            BorderPrimary = 0x00B24BF3,
            BorderSecondary = 0x007B2FF7,
            BackgroundStart = 0x001A0A2E,
            BackgroundEnd = 0x00150D2E,
            TitleColor = 0x00FFFFFF,
            AccentColor = 0x00B24BF3,
            GlowColor = 0x007B2FF7,
            SeparatorColor = 0x00B24BF3,
            CategoryLabel = "DEV",
            CategoryIcon = "</>",
            GlowOuter = 0x005A25A0,
            PanelBackground = 0x000A0A12
        },
        [SoftwareCategory.Creative] = new()
        {
            BorderPrimary = 0x00FF6B6B,
            BorderSecondary = 0x00EE5A24,
            BackgroundStart = 0x002E0A1A,
            BackgroundEnd = 0x002E150D,
            TitleColor = 0x00FFFFFF,
            AccentColor = 0x00FF6B6B,
            GlowColor = 0x00EE5A24,
            SeparatorColor = 0x00FF6B6B,
            CategoryLabel = "CREATIVE",
            CategoryIcon = "\u2605",
            GlowOuter = 0x00AA3535,
            PanelBackground = 0x0012080A
        },
        [SoftwareCategory.Audio] = new()
        {
            BorderPrimary = 0x0000FF88,
            BorderSecondary = 0x0000CC6A,
            BackgroundStart = 0x000A1E0D,
            BackgroundEnd = 0x000D2E15,
            TitleColor = 0x00FFFFFF,
            AccentColor = 0x0000FF88,
            GlowColor = 0x0000CC6A,
            SeparatorColor = 0x0000FF88,
            CategoryLabel = "AUDIO",
            CategoryIcon = "\u266B",
            GlowOuter = 0x00009955,
            PanelBackground = 0x00060E08
        },
        [SoftwareCategory.Video] = new()
        {
            BorderPrimary = 0x00FF9F43,
            BorderSecondary = 0x00EE5A24,
            BackgroundStart = 0x002E1A0A,
            BackgroundEnd = 0x002E150D,
            TitleColor = 0x00FFFFFF,
            AccentColor = 0x00FF9F43,
            GlowColor = 0x00EE5A24,
            SeparatorColor = 0x00FF9F43,
            CategoryLabel = "VIDEO",
            CategoryIcon = "\u25B6",
            GlowOuter = 0x00AA6622,
            PanelBackground = 0x00120A04
        },
        [SoftwareCategory.Browser] = new()
        {
            BorderPrimary = 0x0000A8FF,
            BorderSecondary = 0x000080CC,
            BackgroundStart = 0x000A1428,
            BackgroundEnd = 0x000D1A37,
            TitleColor = 0x00FFFFFF,
            AccentColor = 0x0000A8FF,
            GlowColor = 0x000080CC,
            SeparatorColor = 0x0000A8FF,
            CategoryLabel = "BROWSER",
            CategoryIcon = "\u25C6",
            GlowOuter = 0x00005588,
            PanelBackground = 0x00060A14
        },
        [SoftwareCategory.Terminal] = new()
        {
            BorderPrimary = 0x0000FF00,
            BorderSecondary = 0x0000CC00,
            BackgroundStart = 0x000A1E0A,
            BackgroundEnd = 0x000D2E0D,
            TitleColor = 0x0000FF00,
            AccentColor = 0x0000FF00,
            GlowColor = 0x0000CC00,
            SeparatorColor = 0x0000FF00,
            CategoryLabel = "TERMINAL",
            CategoryIcon = ">_",
            GlowOuter = 0x00009900,
            PanelBackground = 0x00060E06
        },
        [SoftwareCategory.Office] = new()
        {
            BorderPrimary = 0x002196F3,
            BorderSecondary = 0x001976D2,
            BackgroundStart = 0x000A1428,
            BackgroundEnd = 0x000D1A37,
            TitleColor = 0x00FFFFFF,
            AccentColor = 0x002196F3,
            GlowColor = 0x001976D2,
            SeparatorColor = 0x002196F3,
            CategoryLabel = "OFFICE",
            CategoryIcon = "\u2709",
            GlowOuter = 0x00105599,
            PanelBackground = 0x00060A14
        },
        [SoftwareCategory.Gaming] = new()
        {
            BorderPrimary = 0x00FF1744,
            BorderSecondary = 0x00D50000,
            BackgroundStart = 0x002E0A0D,
            BackgroundEnd = 0x002E0D10,
            TitleColor = 0x00FFFFFF,
            AccentColor = 0x00FF1744,
            GlowColor = 0x00D50000,
            SeparatorColor = 0x00FF1744,
            CategoryLabel = "GAMING",
            CategoryIcon = "\u25B3",
            GlowOuter = 0x00AA0A22,
            PanelBackground = 0x00120608
        },
        [SoftwareCategory.Security] = new()
        {
            BorderPrimary = 0x00FFD600,
            BorderSecondary = 0x00FFC107,
            BackgroundStart = 0x001E1A0A,
            BackgroundEnd = 0x002E250D,
            TitleColor = 0x00FFFFFF,
            AccentColor = 0x00FFD600,
            GlowColor = 0x00FFC107,
            SeparatorColor = 0x00FFD600,
            CategoryLabel = "SECURITY",
            CategoryIcon = "\u26A0",
            GlowOuter = 0x00AA8800,
            PanelBackground = 0x000E0C06
        },
        [SoftwareCategory.Network] = new()
        {
            BorderPrimary = 0x0000BCD4,
            BorderSecondary = 0x000097A7,
            BackgroundStart = 0x000A1A1E,
            BackgroundEnd = 0x000D2528,
            TitleColor = 0x00FFFFFF,
            AccentColor = 0x0000BCD4,
            GlowColor = 0x000097A7,
            SeparatorColor = 0x0000BCD4,
            CategoryLabel = "NETWORK",
            CategoryIcon = "\u2B21",
            GlowOuter = 0x00006677,
            PanelBackground = 0x00060A0E
        },
        [SoftwareCategory.FileExplorer] = new()
        {
            BorderPrimary = 0x00FFC107,
            BorderSecondary = 0x00FFB300,
            BackgroundStart = 0x001E1A0A,
            BackgroundEnd = 0x002E250D,
            TitleColor = 0x00FFFFFF,
            AccentColor = 0x00FFC107,
            GlowColor = 0x00FFB300,
            SeparatorColor = 0x00FFC107,
            CategoryLabel = "FILES",
            CategoryIcon = "\u25A1",
            GlowOuter = 0x00AA8805,
            PanelBackground = 0x000E0C06
        },
        [SoftwareCategory.Settings] = new()
        {
            BorderPrimary = 0x009E9E9E,
            BorderSecondary = 0x00757575,
            BackgroundStart = 0x001A1A1A,
            BackgroundEnd = 0x00212121,
            TitleColor = 0x00FFFFFF,
            AccentColor = 0x009E9E9E,
            GlowColor = 0x00757575,
            SeparatorColor = 0x009E9E9E,
            CategoryLabel = "SETTINGS",
            CategoryIcon = "\u2699",
            GlowOuter = 0x00555555,
            PanelBackground = 0x000A0A0A
        },
        [SoftwareCategory.TextEditor] = new()
        {
            BorderPrimary = 0x0080CBC4,
            BorderSecondary = 0x004DB6AC,
            BackgroundStart = 0x000A1E1A,
            BackgroundEnd = 0x000D2E25,
            TitleColor = 0x00FFFFFF,
            AccentColor = 0x0080CBC4,
            GlowColor = 0x004DB6AC,
            SeparatorColor = 0x0080CBC4,
            CategoryLabel = "EDITOR",
            CategoryIcon = "\u270E",
            GlowOuter = 0x00406560,
            PanelBackground = 0x00060E0C
        },
        [SoftwareCategory.Unknown] = new()
        {
            BorderPrimary = 0x00607D8B,
            BorderSecondary = 0x00455A64,
            BackgroundStart = 0x001A1A2E,
            BackgroundEnd = 0x0016213E,
            TitleColor = 0x00FFFFFF,
            AccentColor = 0x00607D8B,
            GlowColor = 0x00455A64,
            SeparatorColor = 0x00607D8B,
            CategoryLabel = "APP",
            CategoryIcon = "\u25CF",
            GlowOuter = 0x00334455,
            PanelBackground = 0x0008080C
        }
    };

    public static TooltipColorTheme GetTheme(SoftwareCategory category)
    {
        return Themes.TryGetValue(category, out var theme) ? theme : Themes[SoftwareCategory.Unknown];
    }

    public static uint LerpColor(uint color1, uint color2, float t)
    {
        var r1 = (color1 >> 16) & 0xFF;
        var g1 = (color1 >> 8) & 0xFF;
        var b1 = color1 & 0xFF;

        var r2 = (color2 >> 16) & 0xFF;
        var g2 = (color2 >> 8) & 0xFF;
        var b2 = color2 & 0xFF;

        var r = (uint)(r1 + (r2 - r1) * t);
        var g = (uint)(g1 + (g2 - g1) * t);
        var b = (uint)(b1 + (b2 - b1) * t);

        return (r << 16) | (g << 8) | b;
    }
}

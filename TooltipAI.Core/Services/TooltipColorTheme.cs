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
            BorderPrimary = 0xFFE0E0E0,
            BorderSecondary = 0xFFA0A0A0,
            BackgroundStart = 0xFF1A1A2E,
            BackgroundEnd = 0xFF16213E,
            TitleColor = 0xFFFFFFFF,
            AccentColor = 0xFF00D4FF,
            GlowColor = 0xFF00D4FF,
            SeparatorColor = 0xFF00D4FF,
            CategoryLabel = "WINDOWS",
            CategoryIcon = "\u25A0",
            GlowOuter = 0xFF0088AA,
            PanelBackground = 0xFF0A0A12
        },
        [SoftwareCategory.WindowsShell] = new()
        {
            BorderPrimary = 0xFF4CC9F0,
            BorderSecondary = 0xFF4895EF,
            BackgroundStart = 0xFF0A1628,
            BackgroundEnd = 0xFF0D2137,
            TitleColor = 0xFFFFFFFF,
            AccentColor = 0xFF4CC9F0,
            GlowColor = 0xFF4CC9F0,
            SeparatorColor = 0xFF4CC9F0,
            CategoryLabel = "SHELL",
            CategoryIcon = "\u25CB",
            GlowOuter = 0xFF2A6A88,
            PanelBackground = 0xFF0A0A12
        },
        [SoftwareCategory.Development] = new()
        {
            BorderPrimary = 0xFFB24BF3,
            BorderSecondary = 0xFF7B2FF7,
            BackgroundStart = 0xFF1A0A2E,
            BackgroundEnd = 0xFF150D2E,
            TitleColor = 0xFFFFFFFF,
            AccentColor = 0xFFB24BF3,
            GlowColor = 0xFF7B2FF7,
            SeparatorColor = 0xFFB24BF3,
            CategoryLabel = "DEV",
            CategoryIcon = "</>",
            GlowOuter = 0xFF5A25A0,
            PanelBackground = 0xFF0A0A12
        },
        [SoftwareCategory.Creative] = new()
        {
            BorderPrimary = 0xFFFF6B6B,
            BorderSecondary = 0xFFEE5A24,
            BackgroundStart = 0xFF2E0A1A,
            BackgroundEnd = 0xFF2E150D,
            TitleColor = 0xFFFFFFFF,
            AccentColor = 0xFFFF6B6B,
            GlowColor = 0xFFEE5A24,
            SeparatorColor = 0xFFFF6B6B,
            CategoryLabel = "CREATIVE",
            CategoryIcon = "\u2605",
            GlowOuter = 0xFFAA3535,
            PanelBackground = 0xFF12080A
        },
        [SoftwareCategory.Audio] = new()
        {
            BorderPrimary = 0xFF00FF88,
            BorderSecondary = 0xFF00CC6A,
            BackgroundStart = 0xFF0A1E0D,
            BackgroundEnd = 0xFF0D2E15,
            TitleColor = 0xFFFFFFFF,
            AccentColor = 0xFF00FF88,
            GlowColor = 0xFF00CC6A,
            SeparatorColor = 0xFF00FF88,
            CategoryLabel = "AUDIO",
            CategoryIcon = "\u266B",
            GlowOuter = 0xFF009955,
            PanelBackground = 0xFF060E08
        },
        [SoftwareCategory.Video] = new()
        {
            BorderPrimary = 0xFFFF9F43,
            BorderSecondary = 0xFFEE5A24,
            BackgroundStart = 0xFF2E1A0A,
            BackgroundEnd = 0xFF2E150D,
            TitleColor = 0xFFFFFFFF,
            AccentColor = 0xFFFF9F43,
            GlowColor = 0xFFEE5A24,
            SeparatorColor = 0xFFFF9F43,
            CategoryLabel = "VIDEO",
            CategoryIcon = "\u25B6",
            GlowOuter = 0xFFAA6622,
            PanelBackground = 0xFF120A04
        },
        [SoftwareCategory.Browser] = new()
        {
            BorderPrimary = 0xFF00A8FF,
            BorderSecondary = 0xFF0080CC,
            BackgroundStart = 0xFF0A1428,
            BackgroundEnd = 0xFF0D1A37,
            TitleColor = 0xFFFFFFFF,
            AccentColor = 0xFF00A8FF,
            GlowColor = 0xFF0080CC,
            SeparatorColor = 0xFF00A8FF,
            CategoryLabel = "BROWSER",
            CategoryIcon = "\u25C6",
            GlowOuter = 0xFF005588,
            PanelBackground = 0xFF060A14
        },
        [SoftwareCategory.Terminal] = new()
        {
            BorderPrimary = 0xFF00FF00,
            BorderSecondary = 0xFF00CC00,
            BackgroundStart = 0xFF0A1E0A,
            BackgroundEnd = 0xFF0D2E0D,
            TitleColor = 0xFF00FF00,
            AccentColor = 0xFF00FF00,
            GlowColor = 0xFF00CC00,
            SeparatorColor = 0xFF00FF00,
            CategoryLabel = "TERMINAL",
            CategoryIcon = ">_",
            GlowOuter = 0xFF009900,
            PanelBackground = 0xFF060E06
        },
        [SoftwareCategory.Office] = new()
        {
            BorderPrimary = 0xFF2196F3,
            BorderSecondary = 0xFF1976D2,
            BackgroundStart = 0xFF0A1428,
            BackgroundEnd = 0xFF0D1A37,
            TitleColor = 0xFFFFFFFF,
            AccentColor = 0xFF2196F3,
            GlowColor = 0xFF1976D2,
            SeparatorColor = 0xFF2196F3,
            CategoryLabel = "OFFICE",
            CategoryIcon = "\u2709",
            GlowOuter = 0xFF105599,
            PanelBackground = 0xFF060A14
        },
        [SoftwareCategory.Gaming] = new()
        {
            BorderPrimary = 0xFFFF1744,
            BorderSecondary = 0xFFD50000,
            BackgroundStart = 0xFF2E0A0D,
            BackgroundEnd = 0xFF2E0D10,
            TitleColor = 0xFFFFFFFF,
            AccentColor = 0xFFFF1744,
            GlowColor = 0xFFD50000,
            SeparatorColor = 0xFFFF1744,
            CategoryLabel = "GAMING",
            CategoryIcon = "\u25B3",
            GlowOuter = 0xFFAA0A22,
            PanelBackground = 0xFF120608
        },
        [SoftwareCategory.Security] = new()
        {
            BorderPrimary = 0xFFFFD600,
            BorderSecondary = 0xFFFFC107,
            BackgroundStart = 0xFF1E1A0A,
            BackgroundEnd = 0xFF2E250D,
            TitleColor = 0xFFFFFFFF,
            AccentColor = 0xFFFFD600,
            GlowColor = 0xFFFFC107,
            SeparatorColor = 0xFFFFD600,
            CategoryLabel = "SECURITY",
            CategoryIcon = "\u26A0",
            GlowOuter = 0xFFAA8800,
            PanelBackground = 0xFF0E0C06
        },
        [SoftwareCategory.Network] = new()
        {
            BorderPrimary = 0xFF00BCD4,
            BorderSecondary = 0xFF0097A7,
            BackgroundStart = 0xFF0A1A1E,
            BackgroundEnd = 0xFF0D2528,
            TitleColor = 0xFFFFFFFF,
            AccentColor = 0xFF00BCD4,
            GlowColor = 0xFF0097A7,
            SeparatorColor = 0xFF00BCD4,
            CategoryLabel = "NETWORK",
            CategoryIcon = "\u2B21",
            GlowOuter = 0xFF006677,
            PanelBackground = 0xFF060A0E
        },
        [SoftwareCategory.FileExplorer] = new()
        {
            BorderPrimary = 0xFFFFC107,
            BorderSecondary = 0xFFFFB300,
            BackgroundStart = 0xFF1E1A0A,
            BackgroundEnd = 0xFF2E250D,
            TitleColor = 0xFFFFFFFF,
            AccentColor = 0xFFFFC107,
            GlowColor = 0xFFFFB300,
            SeparatorColor = 0xFFFFC107,
            CategoryLabel = "FILES",
            CategoryIcon = "\u25A1",
            GlowOuter = 0xFFAA8805,
            PanelBackground = 0xFF0E0C06
        },
        [SoftwareCategory.Settings] = new()
        {
            BorderPrimary = 0xFF9E9E9E,
            BorderSecondary = 0xFF757575,
            BackgroundStart = 0xFF1A1A1A,
            BackgroundEnd = 0xFF212121,
            TitleColor = 0xFFFFFFFF,
            AccentColor = 0xFF9E9E9E,
            GlowColor = 0xFF757575,
            SeparatorColor = 0xFF9E9E9E,
            CategoryLabel = "SETTINGS",
            CategoryIcon = "\u2699",
            GlowOuter = 0xFF555555,
            PanelBackground = 0xFF0A0A0A
        },
        [SoftwareCategory.TextEditor] = new()
        {
            BorderPrimary = 0xFF80CBC4,
            BorderSecondary = 0xFF4DB6AC,
            BackgroundStart = 0xFF0A1E1A,
            BackgroundEnd = 0xFF0D2E25,
            TitleColor = 0xFFFFFFFF,
            AccentColor = 0xFF80CBC4,
            GlowColor = 0xFF4DB6AC,
            SeparatorColor = 0xFF80CBC4,
            CategoryLabel = "EDITOR",
            CategoryIcon = "\u270E",
            GlowOuter = 0xFF406560,
            PanelBackground = 0xFF060E0C
        },
        [SoftwareCategory.Unknown] = new()
        {
            BorderPrimary = 0xFF607D8B,
            BorderSecondary = 0xFF455A64,
            BackgroundStart = 0xFF1A1A2E,
            BackgroundEnd = 0xFF16213E,
            TitleColor = 0xFFFFFFFF,
            AccentColor = 0xFF607D8B,
            GlowColor = 0xFF455A64,
            SeparatorColor = 0xFF607D8B,
            CategoryLabel = "APP",
            CategoryIcon = "\u25CF",
            GlowOuter = 0xFF334455,
            PanelBackground = 0xFF08080C
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

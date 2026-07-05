namespace TooltipAI.Core.Models;

/// <summary>
/// Visual style presets for the glassmorphic overlay.
/// Each concept has its own rendering characteristics.
/// </summary>
public enum GlassmorphicStyle
{
    /// <summary>
    /// Quantum Pill — Compact, centered, Isla Dinámica inspired.
    /// Symmetric horizontal pill below cursor with neon gradient border.
    /// </summary>
    QuantumPill = 0,

    /// <summary>
    /// Contextual Aura — Focus perimetral, no floating element.
    /// Glowing frame that wraps the target UI element.
    /// </summary>
    ContextualAura = 1,

    /// <summary>
    /// The Blade — Asymmetric vertical panel for enterprise.
    /// Premium frosted glass card anchored opposite to cursor direction.
    /// </summary>
    TheBlade = 2,

    /// <summary>
    /// Gaze Bracket — Minimal, high-performance, cyberpunk.
    /// Four corner brackets framing the target element.
    /// </summary>
    GazeBracket = 3
}

/// <summary>
/// Configuration for each glassmorphic visual style.
/// </summary>
public class GlassmorphicConfig
{
    public GlassmorphicStyle Style { get; set; } = GlassmorphicStyle.QuantumPill;

    // Dimensions
    public int Width { get; set; } = 200;
    public int Height { get; set; } = 48;
    public int BorderRadius { get; set; } = 24;
    public int OffsetX { get; set; } = 0;
    public int OffsetY { get; set; } = 15;

    // Glass effect
    public int BlurRadius { get; set; } = 20;
    public float BackgroundOpacity { get; set; } = 0.7f;
    public float BorderOpacity { get; set; } = 0.3f;

    // Colors (ARGB)
    public uint BackgroundColor { get; set; } = 0xCC1A1A2E;    // Dark glass
    public uint BorderColorStart { get; set; } = 0xFF3B82F6;   // Blue
    public uint BorderColorEnd { get; set; } = 0xFF8B5CF6;     // Violet
    public uint TextColor { get; set; } = 0xFFFFFFFF;          // White
    public uint AccentColor { get; set; } = 0xFF3B82F6;        // Blue accent

    // Animation
    public int FadeInMs { get; set; } = 50;
    public int FadeOutMs { get; set; } = 30;
    public int PulseMinOpacity { get; set; } = 40;
    public int PulseMaxOpacity { get; set; } = 90;

    // Voice visualization
    public bool ShowWaveform { get; set; } = true;
    public int WaveformHeight { get; set; } = 20;
    public int WaveformBarWidth { get; set; } = 2;
    public int WaveformBarGap { get; set; } = 2;

    /// <summary>
    /// Get default config for each style.
    /// </summary>
    public static GlassmorphicConfig GetDefault(GlassmorphicStyle style)
    {
        return style switch
        {
            GlassmorphicStyle.QuantumPill => new GlassmorphicConfig
            {
                Style = GlassmorphicStyle.QuantumPill,
                Width = 240,
                Height = 48,
                BorderRadius = 24,
                OffsetY = 15,
                BlurRadius = 24,
                BackgroundOpacity = 0.8f,
                BorderColorStart = 0xFF3B82F6,
                BorderColorEnd = 0xFF8B5CF6,
                ShowWaveform = true
            },
            GlassmorphicStyle.ContextualAura => new GlassmorphicConfig
            {
                Style = GlassmorphicStyle.ContextualAura,
                Width = 0, // Wraps element
                Height = 0,
                BorderRadius = 8,
                BlurRadius = 32,
                BackgroundOpacity = 0f, // No solid background
                BorderOpacity = 0.5f,
                BorderColorStart = 0xFF3B82F6,
                BorderColorEnd = 0xFF8B5CF6,
                PulseMinOpacity = 40,
                PulseMaxOpacity = 90,
                ShowWaveform = false
            },
            GlassmorphicStyle.TheBlade => new GlassmorphicConfig
            {
                Style = GlassmorphicStyle.TheBlade,
                Width = 280,
                Height = 140,
                BorderRadius = 12,
                OffsetY = 0,
                BlurRadius = 40,
                BackgroundOpacity = 0.85f,
                BackgroundColor = 0xE6F8F9FA, // Light glass
                TextColor = 0xFF1F2937, // Dark text
                AccentColor = 0xFF3B82F6,
                ShowWaveform = true,
                WaveformHeight = 8
            },
            GlassmorphicStyle.GazeBracket => new GlassmorphicConfig
            {
                Style = GlassmorphicStyle.GazeBracket,
                Width = 0, // Wraps element
                Height = 0,
                BorderRadius = 0,
                BlurRadius = 0,
                BackgroundOpacity = 0f,
                BorderOpacity = 0.75f,
                BorderColorStart = 0xFFFFFFFF,
                BorderColorEnd = 0xFFFFFFFF, // Monochrome
                TextColor = 0xBFFFFFFF, // 75% white
                ShowWaveform = false
            },
            _ => new GlassmorphicConfig()
        };
    }
}

/// <summary>
/// State of the glassmorphic overlay.
/// </summary>
public enum OverlayState
{
    Hidden,
    FadingIn,
    Visible,
    Listening,   // Voice active
    Processing,  // AI inference
    Executing,   // Action running
    FadingOut,
    PulseBreathing // Contextual Aura breathing
}

/// <summary>
/// Real-time state of the overlay for rendering.
/// </summary>
public class OverlayRenderState
{
    public OverlayState State { get; set; } = OverlayState.Hidden;
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public float CurrentOpacity { get; set; }
    public float TargetOpacity { get; set; }
    public float[]? WaveformData { get; set; }
    public string? StatusText { get; set; }
    public string? ActionText { get; set; }
    public float Progress { get; set; } // 0.0 to 1.0
    public long Timestamp { get; set; }
}

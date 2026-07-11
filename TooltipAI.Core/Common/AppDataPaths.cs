namespace TooltipAI.Core.Common;

/// <summary>
/// Centralizes resolution of the per-user application data directory used by
/// TooltipAI (<c>%AppData%/TooltipAI</c> on Windows and the platform equivalent
/// elsewhere) and of paths within it.
/// </summary>
public static class AppDataPaths
{
    /// <summary>Name of the TooltipAI folder under the user's application data directory.</summary>
    public const string AppFolderName = "TooltipAI";

    /// <summary>
    /// The root TooltipAI data folder. The directory is not guaranteed to
    /// exist; call <see cref="EnsureRoot"/> when it must be present.
    /// </summary>
    public static string Root => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        AppFolderName);

    /// <summary>Combines <paramref name="parts"/> onto the <see cref="Root"/> folder.</summary>
    public static string Combine(params string[] parts)
    {
        var all = new string[parts.Length + 1];
        all[0] = Root;
        Array.Copy(parts, 0, all, 1, parts.Length);
        return Path.Combine(all);
    }

    /// <summary>Creates the <see cref="Root"/> folder if needed and returns its path.</summary>
    public static string EnsureRoot()
    {
        Directory.CreateDirectory(Root);
        return Root;
    }
}

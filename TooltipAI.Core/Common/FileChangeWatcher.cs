namespace TooltipAI.Core.Common;

/// <summary>
/// Creates <see cref="FileSystemWatcher"/> instances that monitor a single file
/// for writes, using consistent notify filters and tolerating environments
/// where file watching is unavailable.
/// </summary>
public static class FileChangeWatcher
{
    /// <summary>
    /// Creates a watcher for <paramref name="filePath"/> that invokes
    /// <paramref name="onChanged"/> when the file is written to or resized.
    /// Returns <c>null</c> when a watcher cannot be created (for example in
    /// sandboxed contexts where <see cref="FileSystemWatcher"/> is unsupported).
    /// </summary>
    public static FileSystemWatcher? TryWatch(string filePath, FileSystemEventHandler onChanged)
    {
        try
        {
            var watcher = new FileSystemWatcher(
                Path.GetDirectoryName(filePath)!, Path.GetFileName(filePath))
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };
            watcher.Changed += onChanged;
            return watcher;
        }
        catch
        {
            // FileSystemWatcher is not available in all contexts.
            return null;
        }
    }
}

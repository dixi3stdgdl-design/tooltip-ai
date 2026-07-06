using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;

namespace TooltipAI.Core.Translate;

/// <summary>
/// Win32 implementation for detecting text selection.
/// Uses clipboard approach for cross-app compatibility.
/// </summary>
public sealed class Win32TextDetector : ITextDetector
{
    private readonly ILogger<Win32TextDetector> _logger;

    public Win32TextDetector(ILogger<Win32TextDetector> logger)
    {
        _logger = logger;
    }

    public async Task<TextSelection?> GetSelectionAsync()
    {
        try
        {
            return await Task.Run(() =>
            {
                var text = GetTextFromClipboard();
                if (string.IsNullOrEmpty(text))
                    return null;

                return new TextSelection
                {
                    Text = text,
                    AppName = GetActiveAppName(),
                    Timestamp = DateTime.UtcNow
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get text selection");
            return null;
        }
    }

    public async Task<string?> GetTextUnderCursorAsync()
    {
        try
        {
            return await Task.Run(() => GetTextFromClipboard());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get text under cursor");
            return null;
        }
    }

    public async Task<bool> HasTextSelectionAsync()
    {
        var selection = await GetSelectionAsync();
        return selection != null && !string.IsNullOrEmpty(selection.Text);
    }

    private string? GetTextFromClipboard()
    {
        try
        {
            if (OpenClipboard(IntPtr.Zero))
            {
                try
                {
                    var handle = GetClipboardData(CF_UNICODETEXT);
                    if (handle != IntPtr.Zero)
                    {
                        var ptr = GlobalLock(handle);
                        if (ptr != IntPtr.Zero)
                        {
                            try
                            {
                                var text = Marshal.PtrToStringUni(ptr);
                                return text;
                            }
                            finally
                            {
                                GlobalUnlock(handle);
                            }
                        }
                    }
                }
                finally
                {
                    CloseClipboard();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Clipboard access failed");
        }
        return null;
    }

    private string GetActiveAppName()
    {
        try
        {
            var hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero)
                return "Unknown";

            GetWindowThreadProcessId(hwnd, out var processId);
            var process = System.Diagnostics.Process.GetProcessById((int)processId);
            return process.ProcessName;
        }
        catch
        {
            return "Unknown";
        }
    }

    #region P/Invoke

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll")]
    private static extern bool CloseClipboard();

    [DllImport("user32.dll")]
    private static extern IntPtr GetClipboardData(uint uFormat);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("kernel32.dll")]
    private static extern bool GlobalUnlock(IntPtr hMem);

    private const uint CF_UNICODETEXT = 13;

    #endregion
}

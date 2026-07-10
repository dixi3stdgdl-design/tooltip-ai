using System.Net.Http.Json;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace TooltipAI.Core.Services;

/// <summary>
/// Checks GitHub Releases for new versions and downloads updates.
/// </summary>
public class UpdateService
{
    private readonly string _repoOwner = "dixi3stdgdl-design";
    private readonly string _repoName = "tooltip-ai";
    private readonly HttpClient _http;
    private readonly string _installPath;
    private readonly ILogger? _logger;

    public event Action<string>? UpdateAvailable;
    public event Action<double>? DownloadProgress;

    public UpdateService(HttpClient? httpClient = null, string? installPath = null, ILogger? logger = null)
    {
        _logger = logger;
        _http = httpClient ?? new HttpClient();
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("TooltipAI-Updater");
        _installPath = installPath ?? AppContext.BaseDirectory;
    }

    public Version CurrentVersion =>
        Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);

    public async Task<ReleaseInfo?> CheckForUpdateAsync(CancellationToken ct = default)
    {
        try
        {
            var url = $"https://api.github.com/repos/{_repoOwner}/{_repoName}/releases/latest";
            _logger?.LogDebug("Checking for updates at {Url}", url);
            var release = await _http.GetFromJsonAsync<GitHubRelease>(url, ct);

            if (release == null || string.IsNullOrEmpty(release.TagName))
                return null;

            var versionStr = release.TagName.TrimStart('v');
            if (!Version.TryParse(versionStr, out var remoteVersion))
                return null;

            if (remoteVersion <= CurrentVersion)
                return null;

            var asset = release.Assets?.FirstOrDefault(a =>
                a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));

            _logger?.LogInformation("Update available: v{Version}", remoteVersion);
            return new ReleaseInfo
            {
                Version = remoteVersion,
                ReleaseNotes = release.Body ?? "",
                DownloadUrl = asset?.BrowserDownloadUrl ?? "",
                FileName = asset?.Name ?? "",
                PublishedAt = release.PublishedAt
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to check for updates");
            return null;
        }
    }

    public async Task<bool> DownloadAndInstallAsync(ReleaseInfo release, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(release.DownloadUrl))
            return false;

        try
        {
            var zipPath = Path.Combine(Path.GetTempPath(), $"tooltip-ai-{release.Version}.zip");
            var backupPath = Path.Combine(Path.GetTempPath(), $"tooltip-ai-backup-{DateTime.Now:yyyyMMddHHmmss}");

            // Backup current
            CopyDirectory(_installPath, backupPath);

            // Download
            using var response = await _http.GetAsync(release.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1;
            var totalRead = 0L;

            await using var contentStream = await response.Content.ReadAsStreamAsync(ct);
            await using var fileStream = File.Create(zipPath);

            var buffer = new byte[8192];
            int bytesRead;
            while ((bytesRead = await contentStream.ReadAsync(buffer, ct)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
                totalRead += bytesRead;
                if (totalBytes > 0)
                    DownloadProgress?.Invoke((double)totalRead / totalBytes * 100);
            }

            fileStream.Close();

            // Extract
            System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, _installPath, overwriteFiles: true);

            // Cleanup
            File.Delete(zipPath);

            UpdateAvailable?.Invoke($"Updated to v{release.Version}");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to download and install update v{Version}", release.Version);
            return false;
        }
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (var file in Directory.GetFiles(source))
            File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), true);
        foreach (var dir in Directory.GetDirectories(source))
            CopyDirectory(dir, Path.Combine(destination, Path.GetFileName(dir)));
    }
}

public class ReleaseInfo
{
    public Version Version { get; set; } = new(1, 0, 0);
    public string ReleaseNotes { get; set; } = "";
    public string DownloadUrl { get; set; } = "";
    public string FileName { get; set; } = "";
    public DateTimeOffset PublishedAt { get; set; }
}

internal class GitHubRelease
{
    public string TagName { get; set; } = "";
    public string Body { get; set; } = "";
    public DateTimeOffset PublishedAt { get; set; }
    public List<GitHubAsset>? Assets { get; set; }
}

internal class GitHubAsset
{
    public string Name { get; set; } = "";
    public string BrowserDownloadUrl { get; set; } = "";
}

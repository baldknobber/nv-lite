using System.Diagnostics;

namespace NVLite.Core.Drivers;

public sealed class DriverDownloader
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromHours(2), // Large driver downloads can take a while
    };

    private static readonly string[] AllowedDownloadHosts =
    [
        "us.download.nvidia.com",
        "international.download.nvidia.com",
        "developer.download.nvidia.com",
    ];

    public async Task<string> DownloadAsync(string url, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        var uri = new Uri(url);
        if (!uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Only HTTPS download URLs are allowed.");

        if (!AllowedDownloadHosts.Any(h => uri.Host.Equals(h, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException($"Untrusted download host: {uri.Host}");

        var downloadsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        Directory.CreateDirectory(downloadsFolder);

        var fileName = Path.GetFileName(uri.AbsolutePath);
        if (string.IsNullOrWhiteSpace(fileName) || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            fileName = "nvidia-driver.exe";

        var filePath = Path.Combine(downloadsFolder, fileName);

        using var response = await HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1;
        long downloadedBytes = 0;
        double lastReportedPercent = -1;

        await using var contentStream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);

        var buffer = new byte[81920]; // 80KB buffer instead of 8KB — fewer iterations
        int bytesRead;
        while ((bytesRead = await contentStream.ReadAsync(buffer, ct).ConfigureAwait(false)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), ct).ConfigureAwait(false);
            downloadedBytes += bytesRead;

            // Throttle progress reports to ~1% increments to avoid flooding the UI thread
            if (totalBytes > 0)
            {
                var percent = Math.Floor((double)downloadedBytes / totalBytes * 100);
                if (percent > lastReportedPercent)
                {
                    lastReportedPercent = percent;
                    progress?.Report((double)downloadedBytes / totalBytes);
                }
            }
        }

        progress?.Report(1.0);
        return filePath;
    }

    /// <summary>
    /// Launches the NVIDIA installer with its own UI (Express mode).
    /// </summary>
    public void LaunchExpressInstaller(string installerPath, bool cleanInstall = false)
    {
        var canonicalPath = ValidateInstallerPath(installerPath);
        var args = cleanInstall ? "-clean -noreboot" : "-noreboot";
        Process.Start(new ProcessStartInfo
        {
            FileName = canonicalPath,
            Arguments = args,
            UseShellExecute = true,
        });
    }

    /// <summary>
    /// Runs the NVIDIA installer silently (Minimal mode) and waits for completion.
    /// Returns the exit code: 0 = success, 1 = reboot needed (still success), 2 = failure.
    /// </summary>
    public async Task<int> InstallSilentAsync(string installerPath, bool cleanInstall = false,
        CancellationToken ct = default)
    {
        var canonicalPath = ValidateInstallerPath(installerPath);

        var args = "-s -noreboot -noeula";
        if (cleanInstall)
            args += " -clean";

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = canonicalPath,
            Arguments = args,
            UseShellExecute = false,
            CreateNoWindow = true,
        }) ?? throw new InvalidOperationException("Failed to start installer process.");

        await process.WaitForExitAsync(ct).ConfigureAwait(false);
        return process.ExitCode;
    }

    private static string ValidateInstallerPath(string installerPath)
    {
        if (!File.Exists(installerPath))
            throw new FileNotFoundException("Installer file not found.", installerPath);

        if (!installerPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Installer must be an .exe file.");

        var downloadsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        var canonicalPath = Path.GetFullPath(installerPath);
        if (!canonicalPath.StartsWith(downloadsFolder, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Installer must be located in the Downloads folder.");

        return canonicalPath;
    }

    public static async Task ClearShaderCacheAsync()
    {
        await Task.Run(() =>
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var cachePaths = new[]
            {
                Path.Combine(localAppData, "NVIDIA", "DXCache"),
                Path.Combine(localAppData, "NVIDIA", "GLCache"),
            };

            foreach (var path in cachePaths)
            {
                if (!Directory.Exists(path)) continue;

                // Skip if the path is a symlink/reparse point to prevent symlink attacks
                var dirInfo = new DirectoryInfo(path);
                if (dirInfo.Attributes.HasFlag(FileAttributes.ReparsePoint)) continue;

                try { Directory.Delete(path, recursive: true); } catch { /* Best effort */ }
            }
        }).ConfigureAwait(false);
    }
}

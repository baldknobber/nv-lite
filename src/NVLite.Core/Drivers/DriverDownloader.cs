using System.Diagnostics;

namespace NVLite.Core.Drivers;

public sealed class DriverDownloader
{
    private static readonly HttpClient HttpClient = new();

    public async Task<string> DownloadAsync(string url, IProgress<double>? progress = null, CancellationToken ct = default)
    {
        var downloadsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        Directory.CreateDirectory(downloadsFolder);

        var fileName = Path.GetFileName(new Uri(url).AbsolutePath);
        if (string.IsNullOrWhiteSpace(fileName))
            fileName = "nvidia-driver.exe";

        var filePath = Path.Combine(downloadsFolder, fileName);

        using var response = await HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1;
        long downloadedBytes = 0;

        await using var contentStream = await response.Content.ReadAsStreamAsync(ct);
        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        var buffer = new byte[8192];
        int bytesRead;
        while ((bytesRead = await contentStream.ReadAsync(buffer, ct)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
            downloadedBytes += bytesRead;

            if (totalBytes > 0)
                progress?.Report((double)downloadedBytes / totalBytes);
        }

        progress?.Report(1.0);
        return filePath;
    }

    public void LaunchInstaller(string installerPath, bool cleanInstall = false)
    {
        var args = cleanInstall ? "-clean" : "";
        Process.Start(new ProcessStartInfo
        {
            FileName = installerPath,
            Arguments = args,
            UseShellExecute = true,
        });
    }

    public static void ClearShaderCache()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var cachePaths = new[]
        {
            Path.Combine(localAppData, "NVIDIA", "DXCache"),
            Path.Combine(localAppData, "NVIDIA", "GLCache"),
        };

        foreach (var path in cachePaths)
        {
            if (Directory.Exists(path))
            {
                try { Directory.Delete(path, recursive: true); } catch { /* Best effort */ }
            }
        }
    }
}

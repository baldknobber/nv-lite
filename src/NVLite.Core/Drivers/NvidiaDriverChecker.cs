using System.Text.Json;
using Microsoft.Win32;

namespace NVLite.Core.Drivers;

public sealed class NvidiaDriverChecker
{
    private static readonly HttpClient HttpClient = new();

    public string? GetInstalledDriverVersion()
    {
        // Try reading from the registry (most reliable on Windows)
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            if (key is not null)
            {
                foreach (var subKeyName in key.GetSubKeyNames())
                {
                    using var subKey = key.OpenSubKey(subKeyName);
                    var displayName = subKey?.GetValue("DisplayName")?.ToString();
                    if (displayName is not null && displayName.Contains("NVIDIA Graphics Driver", StringComparison.OrdinalIgnoreCase))
                    {
                        var version = subKey?.GetValue("DisplayVersion")?.ToString();
                        if (version is not null) return version;
                    }
                }
            }
        }
        catch { /* Fall through to alternate method */ }

        // Fallback: try NVIDIA-specific registry key
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\NVIDIA Corporation\Installer2\Drivers");
            if (key is not null)
            {
                var subKeys = key.GetSubKeyNames();
                if (subKeys.Length > 0)
                {
                    // Get the latest version subkey
                    var latest = subKeys.OrderByDescending(s => s).First();
                    return latest;
                }
            }
        }
        catch { /* Unable to read registry */ }

        return null;
    }

    public async Task<DriverInfo?> GetLatestDriverInfoAsync(CancellationToken ct = default)
    {
        // Use NVIDIA's driver lookup API
        // Parameters: product type 1 (GeForce), product series, OS (Windows 10 64-bit = 57)
        var url = "https://gfwsl.geforce.com/services_toolkit/services/com/nvidia/services/AjaxDriverService.php"
            + "?func=DriverManualLookup"
            + "&pfid=933"     // GeForce RTX series (generic)
            + "&osID=57"      // Windows 10/11 64-bit
            + "&languageCode=1033"
            + "&isWHQL=1"
            + "&dch=1"
            + "&sort1=0"
            + "&numberOfResults=1";

        try
        {
            var response = await HttpClient.GetStringAsync(url, ct);
            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            if (root.TryGetProperty("IDS", out var ids) && ids.GetArrayLength() > 0)
            {
                var driver = ids[0];
                var downloadInfo = driver.GetProperty("downloadInfo");
                var version = downloadInfo.GetProperty("Version").GetString() ?? "";
                var downloadUrl = downloadInfo.GetProperty("DownloadURL").GetString() ?? "";

                // Ensure HTTPS
                if (downloadUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                    downloadUrl = "https://" + downloadUrl[7..];

                return new DriverInfo
                {
                    Version = version,
                    DownloadUrl = downloadUrl,
                    ReleaseDate = downloadInfo.TryGetProperty("ReleaseDateTime", out var rd) ? rd.GetString() : null,
                };
            }
        }
        catch (HttpRequestException)
        {
            // Network error — caller handles null
        }
        catch (JsonException)
        {
            // API response changed — caller handles null
        }

        return null;
    }
}

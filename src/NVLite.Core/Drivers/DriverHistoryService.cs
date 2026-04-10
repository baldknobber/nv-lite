using System.Text.Json;

namespace NVLite.Core.Drivers;

public sealed class DriverHistoryService
{
    private static readonly HttpClient HttpClient = new();
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _cacheDir;
    private List<DriverReleaseInfo>? _cache;

    public DriverHistoryService(string? cacheDir = null)
    {
        _cacheDir = cacheDir ?? AppContext.BaseDirectory;
    }

    public async Task<List<DriverReleaseInfo>> GetRecentReleasesAsync(int count = 10, CancellationToken ct = default)
    {
        if (_cache is not null)
            return _cache;

        // Try loading from cache file first
        var cachePath = Path.Combine(_cacheDir, "driver-history.json");
        _cache = LoadFromCache(cachePath);

        // Fetch fresh data from NVIDIA API
        var fetched = await FetchFromNvidiaAsync(count, ct);
        if (fetched.Count > 0)
        {
            _cache = fetched;
            SaveToCache(cachePath, _cache);
        }

        return _cache ?? [];
    }

    private static async Task<List<DriverReleaseInfo>> FetchFromNvidiaAsync(int count, CancellationToken ct)
    {
        var url = "https://gfwsl.geforce.com/services_toolkit/services/com/nvidia/services/AjaxDriverService.php"
            + "?func=DriverManualLookup"
            + "&pfid=933"
            + "&osID=57"
            + "&languageCode=1033"
            + "&isWHQL=1"
            + "&dch=1"
            + "&sort1=0"
            + $"&numberOfResults={count}";

        try
        {
            var response = await HttpClient.GetStringAsync(url, ct);
            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            var results = new List<DriverReleaseInfo>();

            if (root.TryGetProperty("IDS", out var ids))
            {
                foreach (var driver in ids.EnumerateArray())
                {
                    var info = driver.GetProperty("downloadInfo");
                    var version = info.GetProperty("Version").GetString() ?? "";
                    var downloadUrl = info.GetProperty("DownloadURL").GetString() ?? "";

                    if (downloadUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                        downloadUrl = "https://" + downloadUrl[7..];

                    var releaseDate = info.TryGetProperty("ReleaseDateTime", out var rd)
                        ? rd.GetString() : null;

                    var name = info.TryGetProperty("Name", out var n) ? n.GetString() : null;
                    var branch = name is not null && name.Contains("Studio", StringComparison.OrdinalIgnoreCase)
                        ? "Studio" : "Game Ready";

                    var detailsUrl = info.TryGetProperty("DetailsURL", out var detUrl)
                        ? detUrl.GetString() : null;
                    if (detailsUrl is not null && detailsUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                        detailsUrl = "https://" + detailsUrl[7..];

                    results.Add(new DriverReleaseInfo
                    {
                        Version = version,
                        DownloadUrl = downloadUrl,
                        ReleaseDate = releaseDate,
                        Branch = branch,
                        DetailsUrl = detailsUrl,
                    });
                }
            }

            return results;
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException)
        {
            return [];
        }
    }

    private static List<DriverReleaseInfo>? LoadFromCache(string path)
    {
        try
        {
            if (!File.Exists(path)) return null;

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<DriverReleaseInfo>>(json);
        }
        catch
        {
            return null;
        }
    }

    private static void SaveToCache(string path, List<DriverReleaseInfo> releases)
    {
        try
        {
            var json = JsonSerializer.Serialize(releases, JsonOptions);
            File.WriteAllText(path, json);
        }
        catch (IOException)
        {
            // Best effort — may be read-only location
        }
    }
}

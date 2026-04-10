using System.Text.Json;

namespace NVLite.Core.Profiles;

public sealed class CommunityProfile
{
    public string ProfileName { get; set; } = "";
    public string GameName { get; set; } = "";
    public string? GameExecutable { get; set; }
    public List<string> GpuSeries { get; set; } = [];
    public string? DriverVersionTested { get; set; }
    public Dictionary<string, uint> Settings { get; set; } = [];
    public string Contributor { get; set; } = "";
    public string? Description { get; set; }
    public List<string> Tags { get; set; } = [];
    public string? DateAdded { get; set; }
}

public sealed class CommunityProfileIndex
{
    public int Version { get; set; }
    public List<CommunityProfileEntry> Profiles { get; set; } = [];
}

public sealed class CommunityProfileEntry
{
    public string Name { get; set; } = "";
    public string GameName { get; set; } = "";
    public string Contributor { get; set; } = "";
    public string? Description { get; set; }
    public List<string> GpuSeries { get; set; } = [];
    public string FileName { get; set; } = "";
}

public sealed class CommunityProfileService
{
    private static readonly HttpClient HttpClient = new();
    private const string BaseUrl = "https://raw.githubusercontent.com/baldknobber/nv-lite/main/community-profiles/";
    private const long MaxResponseBytes = 5 * 1024 * 1024; // 5 MB

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<List<CommunityProfileEntry>> GetIndexAsync(CancellationToken ct = default)
    {
        try
        {
            var json = await GetStringSafeAsync(BaseUrl + "profiles-index.json", ct);
            var index = JsonSerializer.Deserialize<CommunityProfileIndex>(json, JsonOptions);
            return index?.Profiles ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<CommunityProfile?> GetProfileAsync(string fileName, CancellationToken ct = default)
    {
        try
        {
            if (!IsValidFileName(fileName))
                return null;

            var json = await GetStringSafeAsync(BaseUrl + fileName, ct);
            return JsonSerializer.Deserialize<CommunityProfile>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static bool IsValidFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return false;
        if (fileName.Contains("..") || fileName.Contains('/') || fileName.Contains('\\'))
            return false;
        if (Path.GetFileName(fileName) != fileName)
            return false;
        return fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<string> GetStringSafeAsync(string url, CancellationToken ct)
    {
        using var response = await HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        if (response.Content.Headers.ContentLength > MaxResponseBytes)
            throw new InvalidOperationException("Response too large.");

        var json = await response.Content.ReadAsStringAsync(ct);
        if (json.Length > MaxResponseBytes)
            throw new InvalidOperationException("Response too large.");

        return json;
    }
}

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

    public async Task<List<CommunityProfileEntry>> GetIndexAsync(CancellationToken ct = default)
    {
        try
        {
            var json = await HttpClient.GetStringAsync(BaseUrl + "profiles-index.json", ct);
            var index = JsonSerializer.Deserialize<CommunityProfileIndex>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
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
            var json = await HttpClient.GetStringAsync(BaseUrl + fileName, ct);
            return JsonSerializer.Deserialize<CommunityProfile>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }
}

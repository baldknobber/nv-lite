namespace NVLite.Core.Drivers;

public sealed class DriverInfo
{
    public required string Version { get; init; }
    public required string DownloadUrl { get; init; }
    public string? ReleaseDate { get; init; }
    public string? ReleaseNotes { get; init; }
}

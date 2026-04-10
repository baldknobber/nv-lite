namespace NVLite.Core.Drivers;

public sealed class DriverReleaseInfo
{
    public required string Version { get; init; }
    public string? ReleaseDate { get; init; }
    public string Branch { get; init; } = "Game Ready";
    public string? DownloadUrl { get; init; }
    public List<string> Highlights { get; init; } = [];
    public List<string> FixedIssues { get; init; } = [];
    public List<KnownIssue> KnownIssues { get; init; } = [];
}

public sealed class KnownIssue
{
    public required string GameName { get; init; }
    public required string Description { get; init; }
    public string? Workaround { get; init; }
    public IssueSeverity Severity { get; init; } = IssueSeverity.Medium;
}

public enum IssueSeverity
{
    Low,
    Medium,
    High
}

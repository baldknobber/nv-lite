namespace NVLite.Core.Profiles;

public sealed class ProfileInfo
{
    public string Name { get; init; } = "";
    public bool IsPredefined { get; init; }
    public int SettingCount { get; init; }
    public int AppCount { get; init; }
}

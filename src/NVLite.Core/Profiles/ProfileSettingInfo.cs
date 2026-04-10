namespace NVLite.Core.Profiles;

public sealed class ProfileSettingInfo
{
    public uint Id { get; init; }
    public string Name { get; init; } = "";
    public string ValueString { get; init; } = "";
    public uint RawValue { get; init; }
    public bool IsPredefined { get; init; }
}

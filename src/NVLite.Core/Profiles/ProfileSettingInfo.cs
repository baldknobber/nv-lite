namespace NVLite.Core.Profiles;

public sealed class ProfileSettingInfo
{
    public uint Id { get; init; }
    public string Name { get; init; } = "";
    public string ValueString { get; init; } = "";
    public uint RawValue { get; init; }
    public bool IsPredefined { get; init; }

    /// <summary>Human-readable value when available (e.g. "On", "Prefer Maximum Performance").</summary>
    public string? FriendlyValue { get; init; }

    /// <summary>Display string: friendly value if known, otherwise hex.</summary>
    public string DisplayValue => FriendlyValue ?? ValueString;

    /// <summary>True if this is a well-known setting with a friendly name.</summary>
    public bool IsKnown { get; init; }

    /// <summary>True if this is one of the ~10 key settings users care about most.</summary>
    public bool IsKeySetting { get; init; }

    /// <summary>True if this is an internal/driver setting not useful to most users.</summary>
    public bool IsInternal { get; init; }

    /// <summary>Known value options for this setting (for dropdown display).</summary>
    public Dictionary<uint, string>? ValueOptions { get; init; }
}

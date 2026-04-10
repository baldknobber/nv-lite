namespace NVLite.Core.Profiles;

/// <summary>
/// Maps well-known NVIDIA DRS setting IDs to friendly names, value labels, and categories.
/// IDs sourced from NvApiDriverSettings.h and NvAPIWrapper KnownSettingId.
/// </summary>
internal static class KnownDrsSettings
{
    internal enum Category { KeySetting, Display, Filtering, Sync, Performance, Compatibility, Internal }

    internal record SettingMeta(string Name, Category Category, Dictionary<uint, string>? Values = null);

    // ----- Key settings that users actually care about -----
    private static readonly Dictionary<uint, string> PowerModeValues = new()
    {
        [0] = "Adaptive",
        [1] = "Prefer Maximum Performance",
        [2] = "Optimal Power",
    };

    private static readonly Dictionary<uint, string> ThreadedOptValues = new()
    {
        [0] = "Auto", [1] = "On", [2] = "Off",
    };

    private static readonly Dictionary<uint, string> VSyncValues = new()
    {
        [0x96861077] = "Off",
        [0x60925292] = "On",
        [0x609C93C2] = "Adaptive",
        [0x994D209A] = "Adaptive (Half Refresh)",
        [0] = "Use 3D Application Setting",
    };

    private static readonly Dictionary<uint, string> TripleBufferValues = new()
    {
        [0] = "Off", [1] = "On",
    };

    private static readonly Dictionary<uint, string> OnOffValues = new()
    {
        [0] = "Off", [1] = "On",
    };

    private static readonly Dictionary<uint, string> TextureQualityValues = new()
    {
        [0] = "Default", [1] = "High Quality", [2] = "Quality",
        [3] = "Performance", [4] = "High Performance",
    };

    private static readonly Dictionary<uint, string> AnisotropicValues = new()
    {
        [0] = "Application Controlled", [1] = "Off",
        [2] = "2x", [4] = "4x", [8] = "8x", [16] = "16x",
    };

    private static readonly Dictionary<uint, string> AAModeValues = new()
    {
        [0] = "Off", [1] = "Application Controlled",
        [2] = "Override", [3] = "Enhance",
    };

    private static readonly Dictionary<uint, string> LowLatencyValues = new()
    {
        [0] = "Off", [1] = "On", [2] = "Ultra",
    };

    private static readonly Dictionary<uint, string> AOModeValues = new()
    {
        [0] = "Off", [1] = "Low", [2] = "Medium", [3] = "High",
    };

    private static readonly Dictionary<uint, string> FrameRateLimiterValues = new()
    {
        [0] = "Off",
        [20] = "20 FPS", [30] = "30 FPS", [60] = "60 FPS",
        [90] = "90 FPS", [120] = "120 FPS", [144] = "144 FPS",
        [165] = "165 FPS", [240] = "240 FPS", [360] = "360 FPS",
    };

    internal static readonly Dictionary<uint, SettingMeta> Settings = new()
    {
        // ===== Key Settings (shown by default) =====
        [0x1057EB71] = new("Power Management Mode", Category.KeySetting, PowerModeValues),
        [0x00A879CF] = new("Vertical Sync", Category.KeySetting, VSyncValues),
        [0x10835002] = new("Frame Rate Limiter", Category.KeySetting, FrameRateLimiterValues),
        [0x20C1221E] = new("Threaded Optimization", Category.KeySetting, ThreadedOptValues),
        [0x00CE2691] = new("Texture Filtering - Quality", Category.KeySetting, TextureQualityValues),
        [0x101E61A9] = new("Anisotropic Filtering", Category.KeySetting, AnisotropicValues),
        [0x1095F170] = new("Low Latency Mode", Category.KeySetting, LowLatencyValues),
        [0x00198FFF] = new("Shader Cache", Category.KeySetting, OnOffValues),
        [0x20FDD1F9] = new("Triple Buffering", Category.KeySetting, TripleBufferValues),
        [0x007BA09E] = new("Maximum Pre-Rendered Frames", Category.KeySetting),

        // ===== Sync & Refresh =====
        [0x005A375C] = new("VSync Tear Control", Category.Sync),
        [0x10FDEC23] = new("VSync Behavior Flags", Category.Sync),
        [0x101AE763] = new("VSync Smooth AFR", Category.Sync),
        [0x10A879CE] = new("Variable Refresh Rate", Category.Sync),
        [0x10835016] = new("Idle App Max FPS Limit", Category.Sync),

        // ===== Display / G-SYNC =====
        [0x10A879CF] = new("G-SYNC Application Override", Category.Display),
        [0x1194F158] = new("G-SYNC Global Mode", Category.Display),
        [0x1095F16F] = new("VRR Overlay Indicator", Category.Display),
        [0x0064B541] = new("Preferred Refresh Rate", Category.Display),

        // ===== Antialiasing =====
        [0x107EFC5B] = new("Antialiasing - Mode", Category.Filtering, AAModeValues),
        [0x10D773D2] = new("Antialiasing - Setting", Category.Filtering),
        [0x10FC2D9C] = new("Antialiasing - Transparency Multisampling", Category.Filtering),
        [0x10D48A85] = new("Antialiasing - Transparency Supersampling", Category.Filtering),
        [0x107D639D] = new("Antialiasing - Gamma Correction", Category.Filtering, OnOffValues),
        [0x2089BF6C] = new("Antialiasing - Line Gamma", Category.Filtering),
        [0x0098C1AC] = new("MFAA (Multi-Frame AA)", Category.Filtering, OnOffValues),

        // ===== Texture Filtering =====
        [0x10D2BB16] = new("Anisotropic Filtering Mode", Category.Filtering),
        [0x00E73211] = new("Anisotropic Sample Optimization", Category.Filtering, OnOffValues),
        [0x0084CD70] = new("Anisotropic Filter Optimization", Category.Filtering, OnOffValues),
        [0x002ECAF2] = new("Trilinear Optimization", Category.Filtering, OnOffValues),
        [0x0019BB68] = new("Negative LOD Bias", Category.Filtering),
        [0x00638E8F] = new("Driver Controlled LOD Bias", Category.Filtering),

        // ===== Performance =====
        [0x10D1EF29] = new("Maximum GPU Power", Category.Performance),
        [0x10115C8C] = new("Battery Boost App FPS", Category.Performance),
        [0x00AC8497] = new("Shader Cache Maximum Size", Category.Performance),

        // ===== FXAA =====
        [0x1034CB89] = new("FXAA Usage", Category.Filtering, OnOffValues),
        [0x1074C972] = new("Enable FXAA", Category.Filtering, OnOffValues),

        // ===== Ambient Occlusion =====
        [0x00667329] = new("Ambient Occlusion", Category.Filtering, AOModeValues),
        [0x00664339] = new("Ambient Occlusion Usage", Category.Filtering),

        // ===== DLSS / NGX =====
        [0x10E41DF4] = new("Override DLSS to DLAA", Category.KeySetting, OnOffValues),
        [0x10E41E01] = new("DLSS-SR Override", Category.Performance, OnOffValues),
        [0x10E41E03] = new("DLSS-FG Override", Category.Performance, OnOffValues),
        [0x10E41E02] = new("DLSS-RR Override", Category.Performance, OnOffValues),

        // ===== Ansel =====
        [0x1035DB89] = new("Ansel Usage", Category.Compatibility, OnOffValues),
        [0x1075D972] = new("Enable Ansel", Category.Compatibility, OnOffValues),

        // ===== Optimus / Hybrid GPU =====
        [0x10F9DC81] = new("Enable Application for Optimus", Category.Compatibility),
        [0x10F9DC84] = new("Optimus Shim Rendering Options", Category.Compatibility),
        [0x10F9DC83] = new("Optimus Max AA", Category.Compatibility),
        [0x10F9DC80] = new("Optimus Flags", Category.Compatibility),

        // ===== Internal =====
        [0x106D5CFF] = new("Hide from Control Panel", Category.Internal),
        [0x108F0841] = new("Export Performance Counters", Category.Internal),
        [0x107CDDBC] = new("Steam Application ID", Category.Internal),
        [0x104554B6] = new("Profile Notification Timeout", Category.Internal),
    };

    /// <summary>Returns the friendly name for a setting, or null if unknown.</summary>
    internal static string? GetName(uint settingId) =>
        Settings.TryGetValue(settingId, out var meta) ? meta.Name : null;

    /// <summary>Returns a friendly value label, or null if unknown.</summary>
    internal static string? GetValueLabel(uint settingId, uint value) =>
        Settings.TryGetValue(settingId, out var meta) && meta.Values is not null
        && meta.Values.TryGetValue(value, out var label) ? label : null;

    /// <summary>Returns true if this setting should be hidden from the default view.</summary>
    internal static bool IsInternal(uint settingId) =>
        Settings.TryGetValue(settingId, out var meta) && meta.Category == Category.Internal;

    /// <summary>Returns true if this is a key user-facing setting.</summary>
    internal static bool IsKeySetting(uint settingId) =>
        Settings.TryGetValue(settingId, out var meta) && meta.Category == Category.KeySetting;
}

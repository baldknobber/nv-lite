using System.Runtime.InteropServices;
using System.Text;
using NVLite.Core.Profiles.Nvapi;

namespace NVLite.Core.Profiles;

public sealed class ProfileService
{
    private bool _available;
    private string? _initError;

    public ProfileService()
    {
        try
        {
            _available = NvapiDrs.TryInitialize();
            if (!_available)
                _initError = "NVAPI initialization returned false";
        }
        catch (DllNotFoundException)
        {
            _available = false;
            _initError = "nvapi64.dll not found â€” NVIDIA drivers may not be installed";
        }
        catch (Exception ex)
        {
            _available = false;
            _initError = ex.Message;
        }
    }

    public bool IsAvailable => _available;
    public string? InitError => _initError;

    /// <summary>
    /// Gets key global driver settings from the current global profile.
    /// Uses EnumSettings for explicitly-set values and known defaults for the rest.
    /// </summary>
    public (List<ProfileSettingInfo> Settings, string Diagnostics) GetBaseProfileSettings()
    {
        var settings = new List<ProfileSettingInfo>();
        var diag = new StringBuilder();

        if (!_available) { diag.Append("NVAPI N/A"); return (settings, diag.ToString()); }
        if (NvapiDrs.DRS_CreateSession is null) { diag.Append("CreateSession=null"); return (settings, diag.ToString()); }

        if (NvapiDrs.DRS_CreateSession(out var hSession) != NvapiDrs.NVAPI_OK) { diag.Append("CreateSession fail"); return (settings, diag.ToString()); }
        try
        {
            if (NvapiDrs.DRS_LoadSettings is null || NvapiDrs.DRS_LoadSettings(hSession) != NvapiDrs.NVAPI_OK)
            { diag.Append("LoadSettings fail"); return (settings, diag.ToString()); }

            var version = (uint)Marshal.SizeOf<NVDRS_SETTING>() | (1 << 16);

            // Get the global profile (NVIDIA Control Panel "Global Settings")
            IntPtr hProfile = IntPtr.Zero;
            if (NvapiDrs.DRS_GetCurrentGlobalProfile is not null)
            {
                var s = NvapiDrs.DRS_GetCurrentGlobalProfile(hSession, out hProfile);
                if (s != NvapiDrs.NVAPI_OK) hProfile = IntPtr.Zero;
            }
            if (hProfile == IntPtr.Zero && NvapiDrs.DRS_GetBaseProfile is not null)
            {
                NvapiDrs.DRS_GetBaseProfile(hSession, out hProfile);
            }
            if (hProfile == IntPtr.Zero) { diag.Append("No profile"); return (settings, diag.ToString()); }

            // Enumerate all explicitly-set settings from the profile
            var explicitSettings = new Dictionary<uint, NVDRS_SETTING>();
            if (NvapiDrs.DRS_EnumSettings is not null)
            {
                uint enumCount = 512;
                var enumBuf = new NVDRS_SETTING[enumCount];
                for (int i = 0; i < enumBuf.Length; i++)
                    enumBuf[i].version = version;

                var esStatus = NvapiDrs.DRS_EnumSettings(hSession, hProfile, 0, ref enumCount, enumBuf);
                if (esStatus == NvapiDrs.NVAPI_OK || esStatus == NvapiDrs.NVAPI_END_ENUMERATION)
                {
                    for (uint i = 0; i < enumCount; i++)
                        explicitSettings[enumBuf[i].settingId] = enumBuf[i];
                }
                diag.Append($"Enum={enumCount} explicit; ");
            }

            // Build settings list: for each key setting, use explicit value or show default
            var keySettings = KnownDrsSettings.Settings
                .Where(kv => KnownDrsSettings.IsKeySetting(kv.Key))
                .ToList();

            foreach (var (settingId, meta) in keySettings)
            {
                uint rawValue;
                bool isPredefined;

                if (explicitSettings.TryGetValue(settingId, out var found))
                {
                    rawValue = found.CurrentDwordValue;
                    isPredefined = found.isCurrentPredefined != 0;
                }
                else
                {
                    rawValue = GetDefaultValue(settingId);
                    isPredefined = true;
                }

                var friendlyValue = KnownDrsSettings.GetValueLabel(settingId, rawValue);
                Dictionary<uint, string>? valueOptions = meta.Values;

                settings.Add(new ProfileSettingInfo
                {
                    Id = settingId,
                    Name = meta.Name,
                    ValueString = $"0x{rawValue:X8}",
                    RawValue = rawValue,
                    IsPredefined = isPredefined,
                    FriendlyValue = friendlyValue,
                    IsKnown = true,
                    IsKeySetting = true,
                    IsInternal = false,
                    ValueOptions = valueOptions,
                });
            }
        }
        finally
        {
            NvapiDrs.DRS_DestroySession?.Invoke(hSession);
        }

        return (settings, diag.ToString());
    }

    private static uint GetDefaultValue(uint settingId) => settingId switch
    {
        0x1057EB71 => 0,  // Power Management: Adaptive
        0x00A879CF => 0,  // VSync: Use 3D Application Setting
        0x10835002 => 0,  // Frame Rate Limiter: Off
        0x20C1221E => 0,  // Threaded Optimization: Auto
        0x00CE2691 => 0,  // Texture Quality: Default
        0x101E61A9 => 0,  // Anisotropic Filtering: App Controlled
        0x1095F170 => 0,  // Low Latency: Off
        0x00198FFF => 1,  // Shader Cache: On
        0x20FDD1F9 => 0,  // Triple Buffering: Off
        0x007BA09E => 1,  // Max Pre-Rendered Frames: Use application setting
        0x10E41DF4 => 0,  // Override DLSS to DLAA: Off
        _ => 0,
    };

    public bool SetGlobalSetting(uint settingId, uint value)
    {
        if (!_available) return false;
        if (NvapiDrs.DRS_CreateSession is null || NvapiDrs.DRS_SetSetting is null) return false;

        if (NvapiDrs.DRS_CreateSession(out var hSession) != NvapiDrs.NVAPI_OK) return false;
        try
        {
            if (NvapiDrs.DRS_LoadSettings is null || NvapiDrs.DRS_LoadSettings(hSession) != NvapiDrs.NVAPI_OK) return false;

            IntPtr hProfile = IntPtr.Zero;
            if (NvapiDrs.DRS_GetCurrentGlobalProfile is not null)
                NvapiDrs.DRS_GetCurrentGlobalProfile(hSession, out hProfile);
            if (hProfile == IntPtr.Zero) return false;

            var setting = new NVDRS_SETTING();
            setting.version = (uint)Marshal.SizeOf<NVDRS_SETTING>() | (1 << 16);
            setting.settingId = settingId;
            setting.settingType = NvapiDrs.NVDRS_DWORD_TYPE;
            setting.predefinedValueData = new byte[4100];
            setting.currentValueData = new byte[4100];
            BitConverter.TryWriteBytes(setting.currentValueData, value);

            if (NvapiDrs.DRS_SetSetting(hSession, hProfile, ref setting) != NvapiDrs.NVAPI_OK) return false;
            if (NvapiDrs.DRS_SaveSettings is null || NvapiDrs.DRS_SaveSettings(hSession) != NvapiDrs.NVAPI_OK) return false;

            return true;
        }
        finally
        {
            NvapiDrs.DRS_DestroySession?.Invoke(hSession);
        }
    }

    public bool RestoreDefaultSettings(uint[] settingIds)
    {
        if (!_available) return false;
        if (NvapiDrs.DRS_CreateSession is null || NvapiDrs.DRS_RestoreProfileDefaultSetting is null) return false;

        if (NvapiDrs.DRS_CreateSession(out var hSession) != NvapiDrs.NVAPI_OK) return false;
        try
        {
            if (NvapiDrs.DRS_LoadSettings is null || NvapiDrs.DRS_LoadSettings(hSession) != NvapiDrs.NVAPI_OK) return false;

            IntPtr hProfile = IntPtr.Zero;
            if (NvapiDrs.DRS_GetCurrentGlobalProfile is not null)
                NvapiDrs.DRS_GetCurrentGlobalProfile(hSession, out hProfile);
            if (hProfile == IntPtr.Zero) return false;

            bool anyFailed = false;
            foreach (var id in settingIds)
            {
                if (NvapiDrs.DRS_RestoreProfileDefaultSetting(hSession, hProfile, id) != NvapiDrs.NVAPI_OK)
                    anyFailed = true;
            }

            if (NvapiDrs.DRS_SaveSettings is null || NvapiDrs.DRS_SaveSettings(hSession) != NvapiDrs.NVAPI_OK) return false;
            return !anyFailed;
        }
        finally
        {
            NvapiDrs.DRS_DestroySession?.Invoke(hSession);
        }
    }
}

using System.Runtime.InteropServices;
using System.Text.Json;
using NVLite.Core.Profiles.Nvapi;

namespace NVLite.Core.Profiles;

public sealed class ProfileService
{
    private bool _available;

    public ProfileService()
    {
        try
        {
            _available = NvapiDrs.TryInitialize();
        }
        catch
        {
            _available = false;
        }
    }

    public bool IsAvailable => _available;

    public List<ProfileInfo> GetAllProfiles()
    {
        var profiles = new List<ProfileInfo>();
        if (!_available) return profiles;
        if (NvapiDrs.DRS_CreateSession is null) return profiles;

        if (NvapiDrs.DRS_CreateSession(out var hSession) != NvapiDrs.NVAPI_OK) return profiles;
        try
        {
            if (NvapiDrs.DRS_LoadSettings is null || NvapiDrs.DRS_LoadSettings(hSession) != NvapiDrs.NVAPI_OK) return profiles;

            for (uint i = 0; ; i++)
            {
                if (NvapiDrs.DRS_EnumProfiles is null) break;
                var status = NvapiDrs.DRS_EnumProfiles(hSession, i, out var hProfile);
                if (status == NvapiDrs.NVAPI_END_ENUMERATION || status != NvapiDrs.NVAPI_OK)
                    break;

                var profileInfo = new NVDRS_PROFILE();
                profileInfo.version = (uint)Marshal.SizeOf<NVDRS_PROFILE>() | (1 << 16);

                if (NvapiDrs.DRS_GetProfileInfo is not null
                    && NvapiDrs.DRS_GetProfileInfo(hSession, hProfile, ref profileInfo) == NvapiDrs.NVAPI_OK)
                {
                    profiles.Add(new ProfileInfo
                    {
                        Name = profileInfo.profileName?.TrimEnd('\0') ?? $"Profile {i}",
                        IsPredefined = profileInfo.isPredefined != 0,
                        SettingCount = (int)profileInfo.numOfSettings,
                        AppCount = (int)profileInfo.numOfApps,
                    });
                }
            }
        }
        finally
        {
            NvapiDrs.DRS_DestroySession?.Invoke(hSession);
        }

        return profiles;
    }

    public List<ProfileSettingInfo> GetProfileSettings(string profileName)
    {
        var settings = new List<ProfileSettingInfo>();
        if (!_available) return settings;
        if (NvapiDrs.DRS_CreateSession is null) return settings;

        if (NvapiDrs.DRS_CreateSession(out var hSession) != NvapiDrs.NVAPI_OK) return settings;
        try
        {
            if (NvapiDrs.DRS_LoadSettings is null || NvapiDrs.DRS_LoadSettings(hSession) != NvapiDrs.NVAPI_OK) return settings;
            if (NvapiDrs.DRS_FindProfileByName is null) return settings;

            if (NvapiDrs.DRS_FindProfileByName(hSession, profileName, out var hProfile) != NvapiDrs.NVAPI_OK)
                return settings;

            if (NvapiDrs.DRS_EnumSettings is null) return settings;

            uint settingCount = 256;
            var nativeSettings = new NVDRS_SETTING[settingCount];
            for (int i = 0; i < nativeSettings.Length; i++)
                nativeSettings[i].version = (uint)Marshal.SizeOf<NVDRS_SETTING>() | (1 << 16);

            var status = NvapiDrs.DRS_EnumSettings(hSession, hProfile, 0, ref settingCount, nativeSettings);
            if (status != NvapiDrs.NVAPI_OK && status != NvapiDrs.NVAPI_END_ENUMERATION)
                return settings;

            for (uint i = 0; i < settingCount; i++)
            {
                var s = nativeSettings[i];
                settings.Add(new ProfileSettingInfo
                {
                    Id = s.settingId,
                    Name = !string.IsNullOrEmpty(s.settingName) ? s.settingName.TrimEnd('\0') : $"0x{s.settingId:X8}",
                    ValueString = $"0x{s.currentValue:X8}",
                    RawValue = s.currentValue,
                    IsPredefined = s.isCurrentPredefined != 0,
                });
            }
        }
        finally
        {
            NvapiDrs.DRS_DestroySession?.Invoke(hSession);
        }

        return settings;
    }

    public void ExportProfilesToJson(string filePath)
    {
        var profiles = GetAllProfiles();
        var export = new Dictionary<string, List<ProfileSettingInfo>>();

        foreach (var profile in profiles.Where(p => !p.IsPredefined))
        {
            var settings = GetProfileSettings(profile.Name);
            if (settings.Count > 0)
                export[profile.Name] = settings;
        }

        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(export, options);
        File.WriteAllText(filePath, json);
    }

    public void ImportProfilesFromJson(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var import = JsonSerializer.Deserialize<Dictionary<string, List<ProfileSettingInfo>>>(json);
        if (import is null || !_available) return;

        foreach (var (profileName, settings) in import)
        {
            // Create profile if it doesn't exist (skip if it does)
            CreateProfile(profileName);
            foreach (var setting in settings)
            {
                SetSetting(profileName, setting.Id, setting.RawValue);
            }
        }
    }

    public bool SetSetting(string profileName, uint settingId, uint value)
    {
        if (!_available) return false;
        if (NvapiDrs.DRS_CreateSession is null || NvapiDrs.DRS_SetSetting is null) return false;

        if (NvapiDrs.DRS_CreateSession(out var hSession) != NvapiDrs.NVAPI_OK) return false;
        try
        {
            if (NvapiDrs.DRS_LoadSettings is null || NvapiDrs.DRS_LoadSettings(hSession) != NvapiDrs.NVAPI_OK) return false;
            if (NvapiDrs.DRS_FindProfileByName is null) return false;
            if (NvapiDrs.DRS_FindProfileByName(hSession, profileName, out var hProfile) != NvapiDrs.NVAPI_OK) return false;

            var setting = new NVDRS_SETTING();
            setting.version = (uint)Marshal.SizeOf<NVDRS_SETTING>() | (1 << 16);
            setting.settingId = settingId;
            setting.settingType = NvapiDrs.NVDRS_DWORD_TYPE;
            setting.currentValue = value;

            if (NvapiDrs.DRS_SetSetting(hSession, hProfile, ref setting) != NvapiDrs.NVAPI_OK) return false;
            if (NvapiDrs.DRS_SaveSettings is null || NvapiDrs.DRS_SaveSettings(hSession) != NvapiDrs.NVAPI_OK) return false;

            return true;
        }
        finally
        {
            NvapiDrs.DRS_DestroySession?.Invoke(hSession);
        }
    }

    public bool CreateProfile(string profileName)
    {
        if (!_available) return false;
        if (NvapiDrs.DRS_CreateSession is null || NvapiDrs.DRS_CreateProfile is null) return false;

        if (NvapiDrs.DRS_CreateSession(out var hSession) != NvapiDrs.NVAPI_OK) return false;
        try
        {
            if (NvapiDrs.DRS_LoadSettings is null || NvapiDrs.DRS_LoadSettings(hSession) != NvapiDrs.NVAPI_OK) return false;

            // Check if profile already exists
            if (NvapiDrs.DRS_FindProfileByName is not null
                && NvapiDrs.DRS_FindProfileByName(hSession, profileName, out _) == NvapiDrs.NVAPI_OK)
                return true; // Already exists

            var profile = new NVDRS_PROFILE();
            profile.version = (uint)Marshal.SizeOf<NVDRS_PROFILE>() | (1 << 16);
            profile.profileName = profileName;
            profile.isPredefined = 0;

            if (NvapiDrs.DRS_CreateProfile(hSession, ref profile, out _) != NvapiDrs.NVAPI_OK) return false;
            if (NvapiDrs.DRS_SaveSettings is null || NvapiDrs.DRS_SaveSettings(hSession) != NvapiDrs.NVAPI_OK) return false;

            return true;
        }
        finally
        {
            NvapiDrs.DRS_DestroySession?.Invoke(hSession);
        }
    }

    public bool DeleteProfile(string profileName)
    {
        if (!_available) return false;
        if (NvapiDrs.DRS_CreateSession is null || NvapiDrs.DRS_DeleteProfile is null) return false;

        if (NvapiDrs.DRS_CreateSession(out var hSession) != NvapiDrs.NVAPI_OK) return false;
        try
        {
            if (NvapiDrs.DRS_LoadSettings is null || NvapiDrs.DRS_LoadSettings(hSession) != NvapiDrs.NVAPI_OK) return false;
            if (NvapiDrs.DRS_FindProfileByName is null) return false;
            if (NvapiDrs.DRS_FindProfileByName(hSession, profileName, out var hProfile) != NvapiDrs.NVAPI_OK) return false;

            if (NvapiDrs.DRS_DeleteProfile(hSession, hProfile) != NvapiDrs.NVAPI_OK) return false;
            if (NvapiDrs.DRS_SaveSettings is null || NvapiDrs.DRS_SaveSettings(hSession) != NvapiDrs.NVAPI_OK) return false;

            return true;
        }
        finally
        {
            NvapiDrs.DRS_DestroySession?.Invoke(hSession);
        }
    }
}

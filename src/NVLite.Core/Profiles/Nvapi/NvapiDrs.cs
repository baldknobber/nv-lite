using System.Runtime.InteropServices;

namespace NVLite.Core.Profiles.Nvapi;

// NVAPI DRS (Driver Settings) P/Invoke declarations
// Based on NVIDIA Profile Inspector (MIT) patterns and official NVAPI headers

internal static class NvapiDrs
{
    private const string NvapiDll = "nvapi64.dll";

    // NVAPI Status codes
    internal const int NVAPI_OK = 0;
    internal const int NVAPI_ERROR = -1;
    internal const int NVAPI_END_ENUMERATION = -7;
    internal const int NVAPI_NO_IMPLEMENTATION = -3;

    // Special setting type values
    internal const uint NVDRS_DWORD_TYPE = 0;
    internal const uint NVDRS_BINARY_TYPE = 1;
    internal const uint NVDRS_STRING_TYPE = 2;
    internal const uint NVDRS_WSTRING_TYPE = 3;

    // Max lengths — NVAPI_UNICODE_STRING_MAX is 2048 wchars (matching nvapi.h)
    internal const int NVAPI_UNICODE_STRING_MAX = 2048;
    internal const int NVAPI_SETTING_MAX_VALUES = 100;

    // Function pointer IDs for nvapi_QueryInterface
    private const uint NvAPI_Initialize_ID = 0x0150E828;
    private const uint NvAPI_Unload_ID = 0xD22BDD7E;
    private const uint NvAPI_DRS_CreateSession_ID = 0x0694D52E;
    private const uint NvAPI_DRS_DestroySession_ID = 0xDAD9CFF8;
    private const uint NvAPI_DRS_LoadSettings_ID = 0x375DBD6B;
    private const uint NvAPI_DRS_SaveSettings_ID = 0xFCBC7E14;
    private const uint NvAPI_DRS_EnumProfiles_ID = 0xBC371EE0;
    private const uint NvAPI_DRS_GetProfileInfo_ID = 0x61CD6FD6;
    private const uint NvAPI_DRS_GetBaseProfile_ID = 0xDA8466A0;
    private const uint NvAPI_DRS_GetCurrentGlobalProfile_ID = 0x617BFF9F;
    private const uint NvAPI_DRS_EnumSettings_ID = 0xAE3039DA;
    private const uint NvAPI_DRS_GetSetting_ID = 0xEA99498D;
    private const uint NvAPI_DRS_GetSetting_ID_FALLBACK = 0x73BF8338;
    private const uint NvAPI_DRS_SetSetting_ID = 0x8A2CF5F5;
    private const uint NvAPI_DRS_SetSetting_ID_FALLBACK = 0x577DD202;
    private const uint NvAPI_DRS_FindProfileByName_ID = 0x7E4A9A0B;
    private const uint NvAPI_DRS_EnumAvailableSettingIds_ID = 0xF020614A;
    private const uint NvAPI_DRS_CreateProfile_ID = 0xCC176068;
    private const uint NvAPI_DRS_DeleteProfile_ID = 0x17093206;
    private const uint NvAPI_DRS_GetNumProfiles_ID = 0x1DAE4FBC;
    private const uint NvAPI_DRS_DeleteProfileSetting_ID = 0xE4A26362;
    private const uint NvAPI_DRS_RestoreProfileDefault_ID = 0xFA5F6134;
    private const uint NvAPI_DRS_RestoreProfileDefaultSetting_ID = 0x53F0381E;

    [DllImport("nvapi64.dll", EntryPoint = "nvapi_QueryInterface", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr QueryInterface(uint id);

    // Delegate types for NVAPI functions
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int NvAPI_InitializeDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int NvAPI_UnloadDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int NvAPI_DRS_CreateSessionDelegate(out IntPtr hSession);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int NvAPI_DRS_DestroySessionDelegate(IntPtr hSession);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int NvAPI_DRS_LoadSettingsDelegate(IntPtr hSession);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int NvAPI_DRS_SaveSettingsDelegate(IntPtr hSession);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int NvAPI_DRS_EnumProfilesDelegate(IntPtr hSession, uint index, out IntPtr hProfile);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int NvAPI_DRS_GetProfileInfoDelegate(IntPtr hSession, IntPtr hProfile, ref NVDRS_PROFILE profileInfo);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int NvAPI_DRS_GetBaseProfileDelegate(IntPtr hSession, out IntPtr hProfile);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int NvAPI_DRS_GetCurrentGlobalProfileDelegate(IntPtr hSession, out IntPtr hProfile);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int NvAPI_DRS_EnumSettingsDelegate(IntPtr hSession, IntPtr hProfile, uint startIndex, ref uint settingCount, [In, Out] NVDRS_SETTING[] settings);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int NvAPI_DRS_GetSettingDelegate(IntPtr hSession, IntPtr hProfile, uint settingId, ref NVDRS_SETTING setting, ref uint reserved);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int NvAPI_DRS_GetSettingOldDelegate(IntPtr hSession, IntPtr hProfile, uint settingId, ref NVDRS_SETTING setting);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int NvAPI_DRS_FindProfileByNameDelegate(IntPtr hSession, NvApiUnicodeString profileName, out IntPtr hProfile);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int NvAPI_DRS_GetNumProfilesDelegate(IntPtr hSession, out uint numProfiles);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int NvAPI_DRS_SetSettingDelegate(IntPtr hSession, IntPtr hProfile, ref NVDRS_SETTING setting);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int NvAPI_DRS_CreateProfileDelegate(IntPtr hSession, ref NVDRS_PROFILE profileInfo, out IntPtr hProfile);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int NvAPI_DRS_DeleteProfileDelegate(IntPtr hSession, IntPtr hProfile);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int NvAPI_DRS_DeleteProfileSettingDelegate(IntPtr hSession, IntPtr hProfile, uint settingId);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int NvAPI_DRS_RestoreProfileDefaultDelegate(IntPtr hSession, IntPtr hProfile);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int NvAPI_DRS_RestoreProfileDefaultSettingDelegate(IntPtr hSession, IntPtr hProfile, uint settingId);

    // Function pointers (resolved at runtime)
    internal static NvAPI_InitializeDelegate? Initialize;
    internal static NvAPI_UnloadDelegate? Unload;
    internal static NvAPI_DRS_CreateSessionDelegate? DRS_CreateSession;
    internal static NvAPI_DRS_DestroySessionDelegate? DRS_DestroySession;
    internal static NvAPI_DRS_LoadSettingsDelegate? DRS_LoadSettings;
    internal static NvAPI_DRS_SaveSettingsDelegate? DRS_SaveSettings;
    internal static NvAPI_DRS_EnumProfilesDelegate? DRS_EnumProfiles;
    internal static NvAPI_DRS_GetProfileInfoDelegate? DRS_GetProfileInfo;
    internal static NvAPI_DRS_GetBaseProfileDelegate? DRS_GetBaseProfile;
    internal static NvAPI_DRS_GetCurrentGlobalProfileDelegate? DRS_GetCurrentGlobalProfile;
    internal static NvAPI_DRS_EnumSettingsDelegate? DRS_EnumSettings;
    internal static NvAPI_DRS_GetSettingDelegate? DRS_GetSetting;
    internal static NvAPI_DRS_GetSettingOldDelegate? DRS_GetSettingOld;
    internal static NvAPI_DRS_FindProfileByNameDelegate? DRS_FindProfileByName;
    internal static NvAPI_DRS_SetSettingDelegate? DRS_SetSetting;
    internal static NvAPI_DRS_CreateProfileDelegate? DRS_CreateProfile;
    internal static NvAPI_DRS_DeleteProfileDelegate? DRS_DeleteProfile;
    internal static NvAPI_DRS_DeleteProfileSettingDelegate? DRS_DeleteProfileSetting;
    internal static NvAPI_DRS_RestoreProfileDefaultDelegate? DRS_RestoreProfileDefault;
    internal static NvAPI_DRS_RestoreProfileDefaultSettingDelegate? DRS_RestoreProfileDefaultSetting;
    internal static NvAPI_DRS_GetNumProfilesDelegate? DRS_GetNumProfiles;

    private static bool _initialized;
    internal static string? InitDiagnostics { get; private set; }

    internal static bool TryInitialize()
    {
        if (_initialized) return true;

        try
        {
            var diag = new System.Text.StringBuilder();

            Initialize = GetDelegate<NvAPI_InitializeDelegate>(NvAPI_Initialize_ID);
            if (Initialize == null) return false;

            var status = Initialize();
            if (status != NVAPI_OK) return false;

            Unload = GetDelegate<NvAPI_UnloadDelegate>(NvAPI_Unload_ID);
            DRS_CreateSession = GetDelegate<NvAPI_DRS_CreateSessionDelegate>(NvAPI_DRS_CreateSession_ID);
            DRS_DestroySession = GetDelegate<NvAPI_DRS_DestroySessionDelegate>(NvAPI_DRS_DestroySession_ID);
            DRS_LoadSettings = GetDelegate<NvAPI_DRS_LoadSettingsDelegate>(NvAPI_DRS_LoadSettings_ID);
            DRS_SaveSettings = GetDelegate<NvAPI_DRS_SaveSettingsDelegate>(NvAPI_DRS_SaveSettings_ID);
            DRS_EnumProfiles = GetDelegate<NvAPI_DRS_EnumProfilesDelegate>(NvAPI_DRS_EnumProfiles_ID);
            DRS_GetProfileInfo = GetDelegate<NvAPI_DRS_GetProfileInfoDelegate>(NvAPI_DRS_GetProfileInfo_ID);
            DRS_GetBaseProfile = GetDelegate<NvAPI_DRS_GetBaseProfileDelegate>(NvAPI_DRS_GetBaseProfile_ID);
            DRS_GetCurrentGlobalProfile = GetDelegate<NvAPI_DRS_GetCurrentGlobalProfileDelegate>(NvAPI_DRS_GetCurrentGlobalProfile_ID);
            DRS_EnumSettings = GetDelegate<NvAPI_DRS_EnumSettingsDelegate>(NvAPI_DRS_EnumSettings_ID);
            DRS_GetSetting = GetDelegate<NvAPI_DRS_GetSettingDelegate>(NvAPI_DRS_GetSetting_ID)
                         ?? GetDelegate<NvAPI_DRS_GetSettingDelegate>(NvAPI_DRS_GetSetting_ID_FALLBACK);
            DRS_GetSettingOld = GetDelegate<NvAPI_DRS_GetSettingOldDelegate>(NvAPI_DRS_GetSetting_ID_FALLBACK);
            DRS_FindProfileByName = GetDelegate<NvAPI_DRS_FindProfileByNameDelegate>(NvAPI_DRS_FindProfileByName_ID);
            DRS_SetSetting = GetDelegate<NvAPI_DRS_SetSettingDelegate>(NvAPI_DRS_SetSetting_ID_FALLBACK);
            DRS_CreateProfile = GetDelegate<NvAPI_DRS_CreateProfileDelegate>(NvAPI_DRS_CreateProfile_ID);
            DRS_DeleteProfile = GetDelegate<NvAPI_DRS_DeleteProfileDelegate>(NvAPI_DRS_DeleteProfile_ID);
            DRS_DeleteProfileSetting = GetDelegate<NvAPI_DRS_DeleteProfileSettingDelegate>(NvAPI_DRS_DeleteProfileSetting_ID);
            DRS_RestoreProfileDefault = GetDelegate<NvAPI_DRS_RestoreProfileDefaultDelegate>(NvAPI_DRS_RestoreProfileDefault_ID);
            DRS_RestoreProfileDefaultSetting = GetDelegate<NvAPI_DRS_RestoreProfileDefaultSettingDelegate>(NvAPI_DRS_RestoreProfileDefaultSetting_ID);
            DRS_GetNumProfiles = GetDelegate<NvAPI_DRS_GetNumProfilesDelegate>(NvAPI_DRS_GetNumProfiles_ID);

            // Log which functions resolved
            diag.Append($"CreateSession={DRS_CreateSession is not null}, ");
            diag.Append($"LoadSettings={DRS_LoadSettings is not null}, ");
            diag.Append($"EnumProfiles={DRS_EnumProfiles is not null}, ");
            diag.Append($"GetProfileInfo={DRS_GetProfileInfo is not null}, ");
            diag.Append($"GetBaseProfile={DRS_GetBaseProfile is not null}, ");
            diag.Append($"FindByName={DRS_FindProfileByName is not null}, ");
            diag.Append($"EnumSettings={DRS_EnumSettings is not null}");

            // Try raw pointer check for EnumProfiles
            var rawPtr = QueryInterface(NvAPI_DRS_EnumProfiles_ID);
            diag.Append($" | EnumProfiles raw ptr=0x{rawPtr:X}");

            InitDiagnostics = diag.ToString();
            _initialized = true;
            return true;
        }
        catch (DllNotFoundException)
        {
            return false;
        }
        catch (EntryPointNotFoundException)
        {
            return false;
        }
    }

    private static T? GetDelegate<T>(uint id) where T : Delegate
    {
        var ptr = QueryInterface(id);
        return ptr == IntPtr.Zero ? null : Marshal.GetDelegateForFunctionPointer<T>(ptr);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct NvApiUnicodeString
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NvapiDrs.NVAPI_UNICODE_STRING_MAX)]
    private string _value;

    public NvApiUnicodeString(string value) => _value = value ?? string.Empty;
    public override string ToString() => _value?.TrimEnd('\0') ?? string.Empty;
}

[StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
internal struct NVDRS_PROFILE
{
    public uint version;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NvapiDrs.NVAPI_UNICODE_STRING_MAX)]
    public string profileName;
    public uint gpuSupport;
    public uint isPredefined;
    public uint numOfApps;
    public uint numOfSettings;
}

[StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
internal struct NVDRS_SETTING
{
    public uint version;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NvapiDrs.NVAPI_UNICODE_STRING_MAX)]
    public string settingName;
    public uint settingId;
    public uint settingType;
    public uint settingLocation;
    public uint isCurrentPredefined;
    public uint isPredefinedValid;
    // Value unions — predefined comes BEFORE current per NVAPI header
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4100)]
    public byte[] predefinedValueData;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4100)]
    public byte[] currentValueData;

    public uint CurrentDwordValue =>
        currentValueData is { Length: >= 4 }
            ? BitConverter.ToUInt32(currentValueData, 0)
            : 0;

    public uint PredefinedDwordValue =>
        predefinedValueData is { Length: >= 4 }
            ? BitConverter.ToUInt32(predefinedValueData, 0)
            : 0;
}

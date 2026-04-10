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

    // Max lengths
    internal const int NVAPI_UNICODE_STRING_MAX = 4096;
    internal const int NVAPI_SETTING_MAX_VALUES = 100;

    // Function pointer IDs for nvapi_QueryInterface
    private const uint NvAPI_Initialize_ID = 0x0150E828;
    private const uint NvAPI_Unload_ID = 0xD22BDD7E;
    private const uint NvAPI_DRS_CreateSession_ID = 0x0694D52E;
    private const uint NvAPI_DRS_DestroySession_ID = 0xDAD9CFF8;
    private const uint NvAPI_DRS_LoadSettings_ID = 0x375DBD6B;
    private const uint NvAPI_DRS_SaveSettings_ID = 0xFCBC7E14;
    private const uint NvAPI_DRS_EnumProfiles_ID = 0x7AE3A515;
    private const uint NvAPI_DRS_GetProfileInfo_ID = 0x61CD6FD6;
    private const uint NvAPI_DRS_GetBaseProfile_ID = 0xDA8466A0;
    private const uint NvAPI_DRS_EnumSettings_ID = 0xAE3039DA;
    private const uint NvAPI_DRS_GetSetting_ID = 0x73BF8338;
    private const uint NvAPI_DRS_SetSetting_ID = 0x577DD202;
    private const uint NvAPI_DRS_FindProfileByName_ID = 0x7E4A9A0B;
    private const uint NvAPI_DRS_EnumAvailableSettingIds_ID = 0xF020614A;

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
    internal delegate int NvAPI_DRS_EnumSettingsDelegate(IntPtr hSession, IntPtr hProfile, uint startIndex, ref uint settingCount, [In, Out] NVDRS_SETTING[] settings);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int NvAPI_DRS_FindProfileByNameDelegate(IntPtr hSession, [MarshalAs(UnmanagedType.LPWStr)] string profileName, out IntPtr hProfile);

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
    internal static NvAPI_DRS_EnumSettingsDelegate? DRS_EnumSettings;
    internal static NvAPI_DRS_FindProfileByNameDelegate? DRS_FindProfileByName;

    private static bool _initialized;

    internal static bool TryInitialize()
    {
        if (_initialized) return true;

        try
        {
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
            DRS_EnumSettings = GetDelegate<NvAPI_DRS_EnumSettingsDelegate>(NvAPI_DRS_EnumSettings_ID);
            DRS_FindProfileByName = GetDelegate<NvAPI_DRS_FindProfileByNameDelegate>(NvAPI_DRS_FindProfileByName_ID);

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
    // Union: for simplicity, treat as DWORD (covers most settings)
    public uint currentValue;
    public uint predefinedValue;
}

using System.Runtime.InteropServices;

namespace NVLite.Core.Monitoring;

public sealed class DisplayInfo
{
    public string MonitorName { get; init; } = "Unknown";
    public int Width { get; init; }
    public int Height { get; init; }
    public int RefreshRate { get; init; }
    public string Resolution => $"{Width} × {Height}";
    public string RefreshRateText => $"{RefreshRate} Hz";
}

public static class DisplayInfoService
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DEVMODEW
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        public ushort dmSpecVersion;
        public ushort dmDriverVersion;
        public ushort dmSize;
        public ushort dmDriverExtra;
        public uint dmFields;
        // Position
        public int dmPositionX;
        public int dmPositionY;
        public uint dmDisplayOrientation;
        public uint dmDisplayFixedOutput;
        // Color
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        public ushort dmLogPixels;
        public uint dmBitsPerPel;
        public uint dmPelsWidth;
        public uint dmPelsHeight;
        public uint dmDisplayFlags;
        public uint dmDisplayFrequency;
        // ICM
        public uint dmICMMethod;
        public uint dmICMIntent;
        public uint dmMediaType;
        public uint dmDitherType;
        public uint dmReserved1;
        public uint dmReserved2;
        public uint dmPanningWidth;
        public uint dmPanningHeight;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DISPLAY_DEVICEW
    {
        public uint cb;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;
        public uint StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }

    private const uint DISPLAY_DEVICE_ACTIVE = 0x1;
    private const int ENUM_CURRENT_SETTINGS = -1;

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool EnumDisplayDevicesW(string? lpDevice, uint iDevNum, ref DISPLAY_DEVICEW lpDisplayDevice, uint dwFlags);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool EnumDisplaySettingsW(string lpszDeviceName, int iModeNum, ref DEVMODEW lpDevMode);

    public static List<DisplayInfo> GetDisplays()
    {
        var displays = new List<DisplayInfo>();

        var adapter = new DISPLAY_DEVICEW { cb = (uint)Marshal.SizeOf<DISPLAY_DEVICEW>() };
        for (uint i = 0; EnumDisplayDevicesW(null, i, ref adapter, 0); i++)
        {
            if ((adapter.StateFlags & DISPLAY_DEVICE_ACTIVE) == 0)
            {
                adapter = new DISPLAY_DEVICEW { cb = (uint)Marshal.SizeOf<DISPLAY_DEVICEW>() };
                continue;
            }

            // Get the monitor name from the child device
            var monitor = new DISPLAY_DEVICEW { cb = (uint)Marshal.SizeOf<DISPLAY_DEVICEW>() };
            string monitorName = adapter.DeviceString;
            if (EnumDisplayDevicesW(adapter.DeviceName, 0, ref monitor, 0))
                monitorName = monitor.DeviceString;

            var devMode = new DEVMODEW { dmSize = (ushort)Marshal.SizeOf<DEVMODEW>() };
            if (EnumDisplaySettingsW(adapter.DeviceName, ENUM_CURRENT_SETTINGS, ref devMode))
            {
                displays.Add(new DisplayInfo
                {
                    MonitorName = string.IsNullOrWhiteSpace(monitorName) ? "Display" : monitorName,
                    Width = (int)devMode.dmPelsWidth,
                    Height = (int)devMode.dmPelsHeight,
                    RefreshRate = (int)devMode.dmDisplayFrequency,
                });
            }

            adapter = new DISPLAY_DEVICEW { cb = (uint)Marshal.SizeOf<DISPLAY_DEVICEW>() };
        }

        return displays;
    }
}

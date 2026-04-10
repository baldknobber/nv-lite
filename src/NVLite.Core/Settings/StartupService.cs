using Microsoft.Win32;

namespace NVLite.Core.Settings;

public static class StartupService
{
    private const string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "NVLite";

    public static bool IsEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey);
            return key?.GetValue(AppName) is not null;
        }
        catch
        {
            return false;
        }
    }

    public static void Set(bool enabled)
    {
        try
        {
            if (enabled)
            {
                var exePath = Environment.ProcessPath;
                if (exePath is null) return;

                using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
                key?.SetValue(AppName, $"\"{exePath}\"");
            }
            else
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
                key?.DeleteValue(AppName, throwOnMissingValue: false);
            }
        }
        catch
        {
            // May fail if registry is restricted — silently ignore
        }
    }
}

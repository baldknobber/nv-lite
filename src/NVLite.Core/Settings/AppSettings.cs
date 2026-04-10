namespace NVLite.Core.Settings;

public sealed class AppSettings
{
    public string Theme { get; set; } = "System";
    public int PollingIntervalSeconds { get; set; } = 1;
    public bool CheckDriverOnStartup { get; set; }
    public double WindowWidth { get; set; }
    public double WindowHeight { get; set; }
    public int WindowX { get; set; } = -1;
    public int WindowY { get; set; } = -1;
    public bool IsMaximized { get; set; } = true;

    // System
    public bool MinimizeToTray { get; set; }
    public bool StartWithWindows { get; set; }

    // Temperature alerts
    public bool EnableTempAlerts { get; set; }
    public int GpuTempAlertThreshold { get; set; } = 85;

    // Auto driver check (0 = disabled)
    public int DriverCheckIntervalHours { get; set; }

    // Temperature display
    public bool UseFahrenheit { get; set; }
}

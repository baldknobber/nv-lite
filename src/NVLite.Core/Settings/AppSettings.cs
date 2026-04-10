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
}

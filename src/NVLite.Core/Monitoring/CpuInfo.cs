namespace NVLite.Core.Monitoring;

public sealed class CpuInfo
{
    public string Name { get; init; } = "Unknown CPU";
    public float? PackageTemperature { get; init; }
    public float? Usage { get; init; }
    public float? Frequency { get; init; }
}

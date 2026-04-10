namespace NVLite.Core.Monitoring;

public sealed class GpuInfo
{
    public string Name { get; init; } = "Unknown GPU";
    public float? Temperature { get; init; }
    public float? CoreClock { get; init; }
    public float? MemoryClock { get; init; }
    public float? Usage { get; init; }
    public float? PowerDraw { get; init; }
    public float? MemoryUsed { get; init; }
    public float? MemoryTotal { get; init; }
    public float? FanSpeed { get; init; }
    public string? DriverVersion { get; init; }
}

namespace NVLite.Core.Monitoring;

public sealed class CpuInfo
{
    public string Name { get; init; } = "Unknown CPU";
    public float? PackageTemperature { get; init; }
    public float? Usage { get; init; }
    public float? Frequency { get; init; }
    public float? Voltage { get; init; }
    public float? PowerDraw { get; init; }
    public int CoreCount { get; init; }
    public int ThreadCount { get; init; }
    public List<CoreDetail> Cores { get; init; } = [];
}

public sealed class CoreDetail
{
    public string Name { get; init; } = "";
    public float? Temperature { get; init; }
    public float? Clock { get; init; }
    public float? Load { get; init; }
}

using NVLite.Core.Monitoring;
using Xunit;

namespace NVLite.Core.Tests.Monitoring;

public class GpuInfoTests
{
    [Fact]
    public void GpuInfo_DefaultValues_AreCorrect()
    {
        var info = new GpuInfo();

        Assert.Equal("Unknown GPU", info.Name);
        Assert.Null(info.Temperature);
        Assert.Null(info.CoreClock);
        Assert.Null(info.MemoryClock);
        Assert.Null(info.Usage);
        Assert.Null(info.PowerDraw);
        Assert.Null(info.MemoryUsed);
        Assert.Null(info.MemoryTotal);
        Assert.Null(info.FanSpeed);
        Assert.Null(info.DriverVersion);
    }

    [Fact]
    public void GpuInfo_WithValues_RoundTrips()
    {
        var info = new GpuInfo
        {
            Name = "NVIDIA GeForce RTX 4080",
            Temperature = 65.5f,
            CoreClock = 2100f,
            MemoryClock = 10000f,
            Usage = 87.3f,
            PowerDraw = 280.5f,
            MemoryUsed = 8192f,
            MemoryTotal = 16384f,
            FanSpeed = 1500f,
            DriverVersion = "572.16",
        };

        Assert.Equal("NVIDIA GeForce RTX 4080", info.Name);
        Assert.Equal(65.5f, info.Temperature);
        Assert.Equal(2100f, info.CoreClock);
        Assert.Equal(87.3f, info.Usage);
        Assert.Equal(280.5f, info.PowerDraw);
        Assert.Equal(8192f, info.MemoryUsed);
        Assert.Equal(16384f, info.MemoryTotal);
        Assert.Equal(1500f, info.FanSpeed);
    }
}

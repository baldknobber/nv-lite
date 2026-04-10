using NVLite.Core.Monitoring;
using Xunit;

namespace NVLite.Core.Tests.Monitoring;

public class CpuInfoTests
{
    [Fact]
    public void CpuInfo_DefaultValues_AreCorrect()
    {
        var info = new CpuInfo();

        Assert.Equal("Unknown CPU", info.Name);
        Assert.Null(info.PackageTemperature);
        Assert.Null(info.Usage);
        Assert.Null(info.Frequency);
    }

    [Fact]
    public void CpuInfo_WithValues_RoundTrips()
    {
        var info = new CpuInfo
        {
            Name = "AMD Ryzen 9 7950X",
            PackageTemperature = 72.0f,
            Usage = 45.2f,
            Frequency = 5700f,
        };

        Assert.Equal("AMD Ryzen 9 7950X", info.Name);
        Assert.Equal(72.0f, info.PackageTemperature);
        Assert.Equal(45.2f, info.Usage);
        Assert.Equal(5700f, info.Frequency);
    }
}

using NVLite.Core.Monitoring;
using Xunit;

namespace NVLite.Core.Tests.Monitoring;

public class HardwareMonitorServiceTests
{
    [Fact]
    public void Service_ReturnsNull_WhenNotOpen()
    {
        using var service = new HardwareMonitorService();
        // Not opened — should return null, not throw
        Assert.Null(service.GetGpuInfo());
        Assert.Null(service.GetCpuInfo());
    }

    [Fact]
    public void Service_CanBeDisposedMultipleTimes()
    {
        var service = new HardwareMonitorService();
        service.Dispose();
        service.Dispose(); // Should not throw
    }

    [Fact]
    public void Service_CloseWithoutOpen_DoesNotThrow()
    {
        var service = new HardwareMonitorService();
        service.Close(); // Should not throw
    }
}

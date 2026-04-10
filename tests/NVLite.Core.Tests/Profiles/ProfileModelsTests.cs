using NVLite.Core.Profiles;
using Xunit;

namespace NVLite.Core.Tests.Profiles;

public class ProfileModelsTests
{
    [Fact]
    public void ProfileInfo_Defaults()
    {
        var p = new ProfileInfo();
        Assert.Equal("", p.Name);
        Assert.False(p.IsPredefined);
        Assert.Equal(0, p.SettingCount);
        Assert.Equal(0, p.AppCount);
    }

    [Fact]
    public void ProfileSettingInfo_Defaults()
    {
        var s = new ProfileSettingInfo();
        Assert.Equal("", s.Name);
        Assert.Equal("", s.ValueString);
        Assert.Equal(0u, s.RawValue);
        Assert.Equal(0u, s.Id);
        Assert.False(s.IsPredefined);
    }

    [Fact]
    public void ProfileService_HandlesNoNvapi()
    {
        // On systems without NVIDIA drivers, ProfileService should not crash
        var service = new ProfileService();
        var profiles = service.GetAllProfiles();
        Assert.NotNull(profiles);
    }
}

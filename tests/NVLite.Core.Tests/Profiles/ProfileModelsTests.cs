using NVLite.Core.Profiles;
using Xunit;

namespace NVLite.Core.Tests.Profiles;

public class ProfileModelsTests
{
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
        var service = new ProfileService();
        var (settings, _) = service.GetBaseProfileSettings();
        Assert.NotNull(settings);
    }
}

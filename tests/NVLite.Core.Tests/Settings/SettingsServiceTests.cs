using NVLite.Core.Settings;

namespace NVLite.Core.Tests.Settings;

public class SettingsServiceTests
{
    [Fact]
    public void Load_ReturnsDefaults_WhenNoFileExists()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var service = new SettingsService(tempDir);
            Assert.Equal("System", service.Settings.Theme);
            Assert.Equal(1, service.Settings.PollingIntervalSeconds);
            Assert.False(service.Settings.CheckDriverOnStartup);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Save_And_Reload_PersistsSettings()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var service = new SettingsService(tempDir);
            service.Settings.Theme = "Dark";
            service.Settings.PollingIntervalSeconds = 5;
            service.Settings.CheckDriverOnStartup = true;
            service.Save();

            var service2 = new SettingsService(tempDir);
            Assert.Equal("Dark", service2.Settings.Theme);
            Assert.Equal(5, service2.Settings.PollingIntervalSeconds);
            Assert.True(service2.Settings.CheckDriverOnStartup);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Load_ReturnsDefaults_WhenFileIsCorrupt()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "settings.json"), "NOT VALID JSON{{{");
            var service = new SettingsService(tempDir);
            Assert.Equal("System", service.Settings.Theme);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}

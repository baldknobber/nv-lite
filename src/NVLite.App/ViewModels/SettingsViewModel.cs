using CommunityToolkit.Mvvm.ComponentModel;
using NVLite.Core.Settings;

namespace NVLite.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;
    private readonly Action<string> _applyTheme;

    [ObservableProperty]
    public partial int SelectedThemeIndex { get; set; }

    [ObservableProperty]
    public partial int PollingIntervalSeconds { get; set; }

    [ObservableProperty]
    public partial bool CheckDriverOnStartup { get; set; }

    [ObservableProperty]
    public partial bool MinimizeToTray { get; set; }

    [ObservableProperty]
    public partial bool StartWithWindows { get; set; }

    [ObservableProperty]
    public partial bool EnableTempAlerts { get; set; }

    [ObservableProperty]
    public partial int GpuTempAlertThreshold { get; set; }

    [ObservableProperty]
    public partial int DriverCheckIntervalHours { get; set; }

    [ObservableProperty]
    public partial bool UseFahrenheit { get; set; }

    public SettingsViewModel(SettingsService settingsService, Action<string> applyTheme)
    {
        _settingsService = settingsService;
        _applyTheme = applyTheme;

        var settings = _settingsService.Settings;
        SelectedThemeIndex = settings.Theme switch
        {
            "Light" => 1,
            "Dark" => 2,
            _ => 0
        };
        PollingIntervalSeconds = settings.PollingIntervalSeconds;
        CheckDriverOnStartup = settings.CheckDriverOnStartup;
        MinimizeToTray = settings.MinimizeToTray;
        StartWithWindows = settings.StartWithWindows;
        EnableTempAlerts = settings.EnableTempAlerts;
        GpuTempAlertThreshold = settings.GpuTempAlertThreshold;
        DriverCheckIntervalHours = settings.DriverCheckIntervalHours;
        UseFahrenheit = settings.UseFahrenheit;
    }

    partial void OnSelectedThemeIndexChanged(int value)
    {
        var theme = value switch
        {
            1 => "Light",
            2 => "Dark",
            _ => "System"
        };
        _settingsService.Settings.Theme = theme;
        _settingsService.Save();
        _applyTheme(theme);
    }

    partial void OnPollingIntervalSecondsChanged(int value)
    {
        var clamped = Math.Clamp(value, 1, 10);
        _settingsService.Settings.PollingIntervalSeconds = clamped;
        _settingsService.Save();
    }

    partial void OnCheckDriverOnStartupChanged(bool value)
    {
        _settingsService.Settings.CheckDriverOnStartup = value;
        _settingsService.Save();
    }

    partial void OnMinimizeToTrayChanged(bool value)
    {
        _settingsService.Settings.MinimizeToTray = value;
        _settingsService.Save();
        App.MainWindow?.EnsureTrayIcon(value);
    }

    partial void OnStartWithWindowsChanged(bool value)
    {
        _settingsService.Settings.StartWithWindows = value;
        _settingsService.Save();
        StartupService.Set(value);
    }

    partial void OnEnableTempAlertsChanged(bool value)
    {
        _settingsService.Settings.EnableTempAlerts = value;
        _settingsService.Save();
    }

    partial void OnGpuTempAlertThresholdChanged(int value)
    {
        var clamped = Math.Clamp(value, 50, 110);
        _settingsService.Settings.GpuTempAlertThreshold = clamped;
        _settingsService.Save();
    }

    partial void OnDriverCheckIntervalHoursChanged(int value)
    {
        var clamped = Math.Clamp(value, 0, 168);
        _settingsService.Settings.DriverCheckIntervalHours = clamped;
        _settingsService.Save();
    }

    partial void OnUseFahrenheitChanged(bool value)
    {
        _settingsService.Settings.UseFahrenheit = value;
        _settingsService.Save();
    }
}

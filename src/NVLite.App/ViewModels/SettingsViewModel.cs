using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
}

using Microsoft.UI.Xaml;
using NVLite.Core.Settings;

namespace NVLite.App;

public partial class App : Application
{
    private static MainWindow? _window;

    public static SettingsService Settings { get; } = new();

    public static MainWindow? MainWindow => _window;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        ApplyTheme(Settings.Settings.Theme);
        _window.Activate();
    }

    public static void ApplyTheme(string theme)
    {
        if (_window?.Content is FrameworkElement root)
        {
            root.RequestedTheme = theme switch
            {
                "Light" => ElementTheme.Light,
                "Dark" => ElementTheme.Dark,
                _ => ElementTheme.Default
            };
        }
    }
}

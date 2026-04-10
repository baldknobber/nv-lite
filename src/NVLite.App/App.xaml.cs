using Microsoft.UI.Xaml;
using NVLite.Core.Drivers;
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

        // Start background driver check if configured
        _ = RunPeriodicDriverCheckAsync();
    }

    public static void ShowNotification(string title, string message)
    {
        _window?.ShowTrayNotification(title, message);
    }

    private static async Task RunPeriodicDriverCheckAsync()
    {
        var hours = Settings.Settings.DriverCheckIntervalHours;
        if (hours <= 0) return;

        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromHours(hours));
            while (await timer.WaitForNextTickAsync())
            {
                var checker = new NvidiaDriverChecker();
                var installed = checker.GetInstalledDriverVersion();
                var latest = await checker.GetLatestDriverInfoAsync();
                if (installed is not null && latest is not null
                    && string.Compare(latest.Version, installed, StringComparison.Ordinal) > 0)
                {
                    ShowNotification("Driver Update Available",
                        $"NVIDIA driver {latest.Version} is available (installed: {installed})");
                }
            }
        }
        catch { /* App shutting down or timer disposed */ }
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

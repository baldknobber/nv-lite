using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NVLite.App.Views;
using Windows.Graphics;

namespace NVLite.App;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.SetIcon("Assets/AppIcon.ico");

        RestoreWindowState();

        // Select Dashboard by default
        NavView.SelectedItem = NavView.MenuItems[0];

        Closed += MainWindow_Closed;
    }

    private void RestoreWindowState()
    {
        var s = App.Settings.Settings;
        var w = s.WindowWidth > 200 ? (int)s.WindowWidth : 1024;
        var h = s.WindowHeight > 200 ? (int)s.WindowHeight : 700;
        AppWindow.Resize(new SizeInt32(w, h));

        if (s.WindowX >= 0 && s.WindowY >= 0)
            AppWindow.Move(new PointInt32(s.WindowX, s.WindowY));
    }

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        var s = App.Settings.Settings;
        s.WindowWidth = AppWindow.Size.Width;
        s.WindowHeight = AppWindow.Size.Height;
        s.WindowX = AppWindow.Position.X;
        s.WindowY = AppWindow.Position.Y;
        App.Settings.Save();
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer is NavigationViewItem item)
        {
            var tag = item.Tag?.ToString();
            var pageType = tag switch
            {
                "Dashboard" => typeof(DashboardPage),
                "Drivers" => typeof(DriverPage),
                "Profiles" => typeof(ProfilesPage),
                "Settings" => typeof(SettingsPage),
                "About" => typeof(AboutPage),
                _ => typeof(DashboardPage)
            };
            ContentFrame.Navigate(pageType);
        }
    }
}

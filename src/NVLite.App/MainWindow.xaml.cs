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
        AppWindow.Resize(new SizeInt32(1024, 700));

        // Select Dashboard by default
        NavView.SelectedItem = NavView.MenuItems[0];
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

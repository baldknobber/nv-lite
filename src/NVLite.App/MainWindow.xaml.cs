using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NVLite.App.Views;
using Windows.Graphics;
using H.NotifyIcon;
using CommunityToolkit.Mvvm.Input;

namespace NVLite.App;

public sealed partial class MainWindow : Window
{
    private TaskbarIcon? _trayIcon;
    private bool _forceClose;

    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.SetIcon("Assets/AppIcon.ico");

        RestoreWindowState();

        if (App.Settings.Settings.MinimizeToTray)
            InitializeTrayIcon();

        // Select Dashboard by default
        NavView.SelectedItem = NavView.MenuItems[0];

        Closed += MainWindow_Closed;
    }

    public void InitializeTrayIcon()
    {
        if (_trayIcon is not null) return;

        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "NVLite",
            DoubleClickCommand = new RelayCommand(RestoreWindow),
        };

        var menu = new MenuFlyout();
        var showItem = new MenuFlyoutItem { Text = "Show NVLite" };
        showItem.Click += (_, _) => RestoreWindow();
        menu.Items.Add(showItem);
        menu.Items.Add(new MenuFlyoutSeparator());
        var exitItem = new MenuFlyoutItem { Text = "Exit" };
        exitItem.Click += (_, _) => ExitApplication();
        menu.Items.Add(exitItem);
        _trayIcon.ContextFlyout = menu;

        _trayIcon.ForceCreate();
    }

    public void EnsureTrayIcon(bool enabled)
    {
        if (enabled)
            InitializeTrayIcon();
        else
        {
            _trayIcon?.Dispose();
            _trayIcon = null;
        }
    }

    public void ShowTrayNotification(string title, string message)
    {
        _trayIcon?.ShowNotification(title: title, message: message);
    }

    private void RestoreWindow()
    {
        WindowExtensions.Show(this);
        Activate();
    }

    private void ExitApplication()
    {
        _forceClose = true;
        _trayIcon?.Dispose();
        _trayIcon = null;
        Close();
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
        if (!_forceClose && App.Settings.Settings.MinimizeToTray && _trayIcon is not null)
        {
            args.Handled = true;
            WindowExtensions.Hide(this);
            return;
        }

        var s = App.Settings.Settings;
        s.WindowWidth = AppWindow.Size.Width;
        s.WindowHeight = AppWindow.Size.Height;
        s.WindowX = AppWindow.Position.X;
        s.WindowY = AppWindow.Position.Y;
        App.Settings.Save();

        _trayIcon?.Dispose();
        _trayIcon = null;
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

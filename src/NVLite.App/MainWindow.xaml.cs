using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using NVLite.App.Views;
using Windows.Graphics;
using H.NotifyIcon;
using CommunityToolkit.Mvvm.Input;
using System.Runtime.InteropServices;

namespace NVLite.App;

public sealed partial class MainWindow : Window
{
    private TaskbarIcon? _trayIcon;
    private bool _forceClose;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint LoadImage(nint hInst, nint name, uint type, int cx, int cy, uint fuLoad);

    [DllImport("kernel32.dll")]
    private static extern nint GetModuleHandle(string? lpModuleName);

    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        SetWindowIcon();

        RestoreWindowState();

        if (App.Settings.Settings.MinimizeToTray)
            InitializeTrayIcon();

        // Select Dashboard by default
        NavView.SelectedItem = NavView.MenuItems[0];

        Closed += MainWindow_Closed;
    }

    private void SetWindowIcon()
    {
        // Load the icon embedded in the exe by ApplicationIcon (resource ID 32512 = IDI_APPLICATION, 
        // but .NET embeds ApplicationIcon as resource ID 32512 which is the main icon).
        // Try the loose file first (works in Debug), fall back to embedded exe resource.
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico");
        if (File.Exists(iconPath))
        {
            AppWindow.SetIcon(iconPath);
        }
        else
        {
            // Load icon from exe's embedded Win32 resources (set by ApplicationIcon in csproj)
            const uint IMAGE_ICON = 1;
            const uint LR_DEFAULTSIZE = 0x00000040;
            nint hIcon = LoadImage(GetModuleHandle(null), 32512, IMAGE_ICON, 0, 0, LR_DEFAULTSIZE);
            if (hIcon != 0)
            {
                var iconId = Microsoft.UI.Win32Interop.GetIconIdFromIcon(hIcon);
                AppWindow.SetIcon(iconId);
            }
        }
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

        // First launch or previously maximized — maximize the window
        if (s.WindowWidth <= 200 || s.IsMaximized)
        {
            if (AppWindow.Presenter is OverlappedPresenter presenter)
                presenter.Maximize();
            return;
        }

        var w = (int)s.WindowWidth;
        var h = (int)s.WindowHeight;
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
        s.IsMaximized = AppWindow.Presenter is OverlappedPresenter p
                        && p.State == OverlappedPresenterState.Maximized;
        if (!s.IsMaximized)
        {
            s.WindowWidth = AppWindow.Size.Width;
            s.WindowHeight = AppWindow.Size.Height;
            s.WindowX = AppWindow.Position.X;
            s.WindowY = AppWindow.Position.Y;
        }
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
                "Settings" => typeof(SettingsPage),
                "About" => typeof(AboutPage),
                _ => typeof(DashboardPage)
            };
            ContentFrame.Navigate(pageType);
        }
    }
}

using Microsoft.UI.Xaml.Controls;
using NVLite.App.ViewModels;

namespace NVLite.App.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        ViewModel = new SettingsViewModel(App.Settings, App.ApplyTheme);
        InitializeComponent();
    }
}

using Microsoft.UI.Xaml.Controls;
using NVLite.App.ViewModels;

namespace NVLite.App.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardViewModel ViewModel { get; }

    public DashboardPage()
    {
        ViewModel = new DashboardViewModel();
        InitializeComponent();
        Loaded += async (_, _) => await ViewModel.StartMonitoringAsync();
        Unloaded += (_, _) => ViewModel.StopMonitoring();
    }
}

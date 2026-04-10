using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NVLite.App.ViewModels;

namespace NVLite.App.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardViewModel ViewModel { get; }

    public DashboardPage()
    {
        ViewModel = new DashboardViewModel();
        ViewModel.NotificationRequested += (title, msg) => App.ShowNotification(title, msg);
        ViewModel.SuppressSelectionChanged = () => _suppressSelectionChanged = true;
        ViewModel.ResumeSelectionChanged = () => _suppressSelectionChanged = false;
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            ViewModel.LoadDisplayInfo();
            await ViewModel.LoadGpuSettingsAsync();
            _suppressSelectionChanged = false;
            await ViewModel.StartMonitoringAsync();
        };
        Unloaded += (_, _) => ViewModel.StopMonitoring();
    }

    private bool _suppressSelectionChanged = true;

    private void GpuSettingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressSelectionChanged) return;
        if (sender is not ComboBox combo) return;
        if (combo.DataContext is not GpuSettingItem setting) return;
        if (combo.SelectedIndex < 0 || combo.SelectedIndex >= setting.OptionKeys.Count) return;

        var newValue = setting.OptionKeys[combo.SelectedIndex];
        setting.CurrentValue = newValue;
        ViewModel.ApplyGpuSetting(setting.Id, newValue);
    }
}

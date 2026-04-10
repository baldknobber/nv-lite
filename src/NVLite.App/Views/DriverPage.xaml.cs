using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using NVLite.App.ViewModels;

namespace NVLite.App.Views;

public sealed partial class DriverPage : Page
{
    public DriverViewModel ViewModel { get; }

    public DriverPage()
    {
        ViewModel = new DriverViewModel();
        InitializeComponent();
        Loaded += DriverPage_Loaded;
    }

    private async void DriverPage_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= DriverPage_Loaded;
        await ViewModel.AutoCheckAsync();
    }

    private async void VersionDownload_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string url)
        {
            await ViewModel.DownloadSpecificVersionAsync(url);
        }
    }
}

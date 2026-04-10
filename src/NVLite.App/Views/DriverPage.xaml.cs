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
    }
}

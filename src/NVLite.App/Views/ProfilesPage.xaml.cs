using Microsoft.UI.Xaml.Controls;
using NVLite.App.ViewModels;

namespace NVLite.App.Views;

public sealed partial class ProfilesPage : Page
{
    public ProfilesViewModel ViewModel { get; }

    public ProfilesPage()
    {
        ViewModel = new ProfilesViewModel();
        InitializeComponent();
        Loaded += async (_, _) => await ViewModel.LoadProfilesAsync();
    }
}

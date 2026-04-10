using System.Globalization;
using Microsoft.UI.Xaml;
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

    private async void NewProfile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "New Profile",
            PrimaryButtonText = "Create",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot
        };

        var input = new TextBox { PlaceholderText = "Profile name" };
        dialog.Content = input;

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(input.Text))
        {
            await ViewModel.CreateProfileCommand.ExecuteAsync(input.Text.Trim());
        }
    }

    private async void DeleteProfile_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedProfile is null) return;
        if (ViewModel.SelectedProfile.IsPredefined)
        {
            ViewModel.StatusText = "Cannot delete built-in profiles";
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "Delete Profile",
            Content = $"Delete \"{ViewModel.SelectedProfile.Name}\"? This cannot be undone.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            await ViewModel.DeleteProfileCommand.ExecuteAsync(null);
        }
    }

    private void SettingValue_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox) return;
        if (textBox.Tag is not uint settingId) return;

        var text = textBox.Text.Trim();
        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
            && uint.TryParse(text[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hexValue))
        {
            ViewModel.StageSettingChange(settingId, hexValue);
        }
        else if (uint.TryParse(text, out var decValue))
        {
            ViewModel.StageSettingChange(settingId, decValue);
        }
    }
}

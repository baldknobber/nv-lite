using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using NVLite.Core.Profiles;

namespace NVLite.App.ViewModels;

public partial class ProfilesViewModel : ObservableObject
{
    private readonly ProfileService _profileService = new();
    private List<ProfileInfo> _allProfiles = [];

    [ObservableProperty] public partial ObservableCollection<ProfileInfo> FilteredProfiles { get; set; } = [];
    [ObservableProperty] public partial ProfileInfo? SelectedProfile { get; set; }
    [ObservableProperty] public partial string SelectedProfileName { get; set; } = "";
    [ObservableProperty] public partial ObservableCollection<ProfileSettingInfo> SelectedProfileSettings { get; set; } = [];

    partial void OnSelectedProfileChanged(ProfileInfo? value)
    {
        if (value is null)
        {
            SelectedProfileName = "";
            SelectedProfileSettings.Clear();
            return;
        }

        SelectedProfileName = value.Name;
        var settings = _profileService.GetProfileSettings(value.Name);
        SelectedProfileSettings = new ObservableCollection<ProfileSettingInfo>(settings);
    }

    public async Task LoadProfilesAsync()
    {
        try
        {
            _allProfiles = await Task.Run(() => _profileService.GetAllProfiles());
            FilteredProfiles = new ObservableCollection<ProfileInfo>(_allProfiles);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load profiles: {ex.Message}");
        }
    }

    public void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput) return;

        var query = sender.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(query))
        {
            FilteredProfiles = new ObservableCollection<ProfileInfo>(_allProfiles);
        }
        else
        {
            var filtered = _allProfiles
                .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
            FilteredProfiles = new ObservableCollection<ProfileInfo>(filtered);
        }
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        if (_allProfiles.Count == 0) return;
        try
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "nvlite-profiles.json");
            await Task.Run(() => _profileService.ExportProfilesToJson(path));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Export failed: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ImportAsync()
    {
        try
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "nvlite-profiles.json");
            if (!File.Exists(path)) return;
            await Task.Run(() => _profileService.ImportProfilesFromJson(path));
            await LoadProfilesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Import failed: {ex.Message}");
        }
    }
}

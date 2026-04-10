using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using NVLite.Core.Profiles;
using Windows.Storage.Pickers;

namespace NVLite.App.ViewModels;

public partial class ProfilesViewModel : ObservableObject
{
    private readonly ProfileService _profileService = new();
    private readonly CommunityProfileService _communityService = new();
    private List<ProfileInfo> _allProfiles = [];

    [ObservableProperty] public partial ObservableCollection<ProfileInfo> FilteredProfiles { get; set; } = [];
    [ObservableProperty] public partial ProfileInfo? SelectedProfile { get; set; }
    [ObservableProperty] public partial string SelectedProfileName { get; set; } = "";
    [ObservableProperty] public partial ObservableCollection<ProfileSettingInfo> SelectedProfileSettings { get; set; } = [];
    [ObservableProperty] public partial string StatusText { get; set; } = "";
    [ObservableProperty] public partial bool HasUnsavedChanges { get; set; }
    [ObservableProperty] public partial bool CanDeleteSelectedProfile { get; set; }
    [ObservableProperty] public partial ObservableCollection<CommunityProfileEntry> CommunityProfiles { get; set; } = [];
    [ObservableProperty] public partial bool HasCommunityProfiles { get; set; }

    private readonly Dictionary<uint, uint> _pendingChanges = [];

    partial void OnSelectedProfileChanged(ProfileInfo? value)
    {
        if (value is null)
        {
            SelectedProfileName = "";
            SelectedProfileSettings.Clear();
            _pendingChanges.Clear();
            HasUnsavedChanges = false;
            CanDeleteSelectedProfile = false;
            return;
        }

        SelectedProfileName = value.Name;
        CanDeleteSelectedProfile = !value.IsPredefined;
        _pendingChanges.Clear();
        HasUnsavedChanges = false;
        var settings = _profileService.GetProfileSettings(value.Name);
        SelectedProfileSettings = new ObservableCollection<ProfileSettingInfo>(settings);
    }

    public async Task LoadProfilesAsync()
    {
        try
        {
            StatusText = "Loading profiles...";
            _allProfiles = await Task.Run(() => _profileService.GetAllProfiles());

            // Sort: custom profiles first, then predefined, both alphabetical
            _allProfiles = [.. _allProfiles
                .OrderBy(p => p.IsPredefined)
                .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase)];

            FilteredProfiles = new ObservableCollection<ProfileInfo>(_allProfiles);
            StatusText = $"{_allProfiles.Count} profiles loaded";
        }
        catch (Exception ex)
        {
            StatusText = $"Failed to load profiles: {ex.Message}";
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

    public void StageSettingChange(uint settingId, uint newValue)
    {
        _pendingChanges[settingId] = newValue;
        HasUnsavedChanges = _pendingChanges.Count > 0;
    }

    [RelayCommand]
    private async Task SaveChangesAsync()
    {
        if (SelectedProfile is null || _pendingChanges.Count == 0) return;

        var profileName = SelectedProfile.Name;
        var changesToApply = new Dictionary<uint, uint>(_pendingChanges);
        var failed = 0;
        await Task.Run(() =>
        {
            foreach (var (settingId, value) in changesToApply)
            {
                if (!_profileService.SetSetting(profileName, settingId, value))
                    failed++;
            }
        });

        _pendingChanges.Clear();
        HasUnsavedChanges = false;

        if (failed > 0)
            StatusText = $"Saved with {failed} error(s)";
        else
            StatusText = "Changes saved";

        // Refresh settings display
        OnSelectedProfileChanged(SelectedProfile);
    }

    [RelayCommand]
    private async Task CreateProfileAsync(string? profileName)
    {
        if (string.IsNullOrWhiteSpace(profileName)) return;

        var success = await Task.Run(() => _profileService.CreateProfile(profileName));
        if (success)
        {
            StatusText = $"Created profile: {profileName}";
            await LoadProfilesAsync();
        }
        else
        {
            StatusText = "Failed to create profile (NVAPI unavailable or error)";
        }
    }

    [RelayCommand]
    private async Task DeleteProfileAsync()
    {
        if (SelectedProfile is null || SelectedProfile.IsPredefined) return;

        var name = SelectedProfile.Name;
        var success = await Task.Run(() => _profileService.DeleteProfile(name));
        if (success)
        {
            StatusText = $"Deleted profile: {name}";
            SelectedProfile = null;
            await LoadProfilesAsync();
        }
        else
        {
            StatusText = "Failed to delete profile";
        }
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        if (_allProfiles.Count == 0) return;
        try
        {
            var picker = new FileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            picker.SuggestedFileName = "nvlite-profiles";
            picker.FileTypeChoices.Add("JSON", [".json"]);

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSaveFileAsync();
            if (file is null) return;

            await Task.Run(() => _profileService.ExportProfilesToJson(file.Path));
            StatusText = $"Exported to {file.Name}";
        }
        catch (Exception ex)
        {
            StatusText = $"Export failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ImportAsync()
    {
        try
        {
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            picker.FileTypeFilter.Add(".json");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file is null) return;

            await Task.Run(() => _profileService.ImportProfilesFromJson(file.Path));
            StatusText = $"Imported from {file.Name}";
            await LoadProfilesAsync();
        }
        catch (Exception ex)
        {
            StatusText = $"Import failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task LoadCommunityProfilesAsync()
    {
        try
        {
            var entries = await _communityService.GetIndexAsync();
            CommunityProfiles = new ObservableCollection<CommunityProfileEntry>(entries);
            HasCommunityProfiles = CommunityProfiles.Count > 0;
        }
        catch
        {
            HasCommunityProfiles = false;
        }
    }

    public async Task ApplyCommunityProfileAsync(string fileName)
    {
        try
        {
            StatusText = "Downloading community profile...";
            var profile = await _communityService.GetProfileAsync(fileName);
            if (profile is null)
            {
                StatusText = "Failed to download profile";
                return;
            }

            var created = await Task.Run(() => _profileService.CreateProfile(profile.ProfileName));
            if (!created)
            {
                StatusText = "Failed to create profile (NVAPI unavailable)";
                return;
            }

            var failed = 0;
            await Task.Run(() =>
            {
                foreach (var (settingIdHex, value) in profile.Settings)
                {
                    if (uint.TryParse(settingIdHex.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber,
                        System.Globalization.CultureInfo.InvariantCulture, out var settingId))
                    {
                        if (!_profileService.SetSetting(profile.ProfileName, settingId, value))
                            failed++;
                    }
                }
            });

            StatusText = failed > 0
                ? $"Applied \"{profile.ProfileName}\" with {failed} error(s)"
                : $"Applied \"{profile.ProfileName}\" successfully";
            await LoadProfilesAsync();
        }
        catch (Exception ex)
        {
            StatusText = $"Failed to apply profile: {ex.Message}";
        }
    }
}

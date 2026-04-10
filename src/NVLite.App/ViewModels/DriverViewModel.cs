using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NVLite.Core.Drivers;

namespace NVLite.App.ViewModels;

public partial class DriverViewModel : ObservableObject
{
    private readonly NvidiaDriverChecker _checker = new();
    private readonly DriverDownloader _downloader = new();

    [ObservableProperty] public partial string InstalledVersion { get; set; } = "Checking...";
    [ObservableProperty] public partial string LatestVersion { get; set; } = "—";
    [ObservableProperty] public partial string UpdateStatus { get; set; } = "";
    [ObservableProperty] public partial double DownloadProgress { get; set; }
    [ObservableProperty] public partial bool IsDownloading { get; set; }
    [ObservableProperty] public partial bool CleanInstall { get; set; }
    [ObservableProperty] public partial bool ClearShaderCache { get; set; }
    [ObservableProperty] public partial string StatusText { get; set; } = "";

    private string? _downloadUrl;
    private string? _downloadedFilePath;

    [RelayCommand]
    private async Task CheckForUpdatesAsync()
    {
        try
        {
            StatusText = "Checking installed driver...";
            var installed = _checker.GetInstalledDriverVersion();
            InstalledVersion = installed ?? "Not found";

            StatusText = "Checking for latest driver...";
            var latest = await _checker.GetLatestDriverInfoAsync();
            if (latest is not null)
            {
                LatestVersion = latest.Version;
                _downloadUrl = latest.DownloadUrl;

                if (installed is not null && string.Compare(latest.Version, installed, StringComparison.Ordinal) > 0)
                    UpdateStatus = "Update available!";
                else
                    UpdateStatus = "You're up to date.";
            }
            else
            {
                LatestVersion = "Could not retrieve";
                UpdateStatus = "";
            }
            StatusText = "";
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DownloadDriverAsync()
    {
        if (_downloadUrl is null)
        {
            StatusText = "Check for updates first.";
            return;
        }

        try
        {
            IsDownloading = true;
            StatusText = "Downloading...";
            var progress = new Progress<double>(p => DownloadProgress = p * 100);
            _downloadedFilePath = await _downloader.DownloadAsync(_downloadUrl, progress);
            StatusText = $"Downloaded to {_downloadedFilePath}";
        }
        catch (Exception ex)
        {
            StatusText = $"Download failed: {ex.Message}";
        }
        finally
        {
            IsDownloading = false;
        }
    }

    [RelayCommand]
    private void LaunchInstaller()
    {
        if (_downloadedFilePath is null || !File.Exists(_downloadedFilePath))
        {
            StatusText = "Download a driver first.";
            return;
        }

        try
        {
            if (ClearShaderCache)
                DriverDownloader.ClearShaderCache();

            _downloader.LaunchInstaller(_downloadedFilePath, CleanInstall);
            StatusText = "Installer launched.";
        }
        catch (Exception ex)
        {
            StatusText = $"Failed to launch: {ex.Message}";
        }
    }
}

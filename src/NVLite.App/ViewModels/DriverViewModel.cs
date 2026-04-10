using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NVLite.Core.Drivers;

namespace NVLite.App.ViewModels;

public partial class DriverViewModel : ObservableObject
{
    private readonly NvidiaDriverChecker _checker = new();
    private readonly DriverDownloader _downloader = new();
    private readonly DriverHistoryService _historyService = new();

    [ObservableProperty] public partial string InstalledVersion { get; set; } = "Checking...";
    [ObservableProperty] public partial string LatestVersion { get; set; } = "—";
    [ObservableProperty] public partial string UpdateStatus { get; set; } = "";
    [ObservableProperty] public partial double DownloadProgress { get; set; }
    [ObservableProperty] public partial bool IsDownloading { get; set; }
    [ObservableProperty] public partial bool CleanInstall { get; set; }
    [ObservableProperty] public partial bool ClearShaderCache { get; set; }
    [ObservableProperty] public partial string StatusText { get; set; } = "";
    [ObservableProperty] public partial string DownloadSizeText { get; set; } = "";
    [ObservableProperty] public partial bool HasVersionHistory { get; set; }
    [ObservableProperty] public partial string? RollbackVersion { get; set; }
    [ObservableProperty] public partial bool HasRollback { get; set; }

    public ObservableCollection<DriverReleaseInfo> VersionHistory { get; } = [];

    private string? _downloadUrl;
    private string? _downloadedFilePath;
    private string? _rollbackUrl;
    private CancellationTokenSource? _downloadCts;

    public async Task AutoCheckAsync()
    {
        await CheckForUpdatesAsync();
    }

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

            StatusText = "Loading version history...";
            await LoadVersionHistoryAsync();
            StatusText = "";
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
    }

    private async Task LoadVersionHistoryAsync()
    {
        try
        {
            var releases = await _historyService.GetRecentReleasesAsync(10);
            VersionHistory.Clear();

            var installed = InstalledVersion;
            int installedIdx = -1;

            for (int i = 0; i < releases.Count; i++)
            {
                var release = releases[i];
                if (installed is not null && release.Version == installed)
                {
                    release.IsInstalled = true;
                    installedIdx = i;
                }
                VersionHistory.Add(release);
            }

            HasVersionHistory = VersionHistory.Count > 0;

            // Find rollback target (first version older than installed)
            if (installedIdx >= 0 && installedIdx + 1 < releases.Count)
            {
                var rollback = releases[installedIdx + 1];
                RollbackVersion = rollback.Version;
                _rollbackUrl = rollback.DownloadUrl;
                HasRollback = true;
            }
            else
            {
                HasRollback = false;
            }
        }
        catch
        {
            HasVersionHistory = false;
            HasRollback = false;
        }
    }

    [RelayCommand]
    private async Task DownloadDriverAsync()
    {
        await DownloadFromUrlAsync(_downloadUrl);
    }

    public async Task DownloadSpecificVersionAsync(string? url)
    {
        await DownloadFromUrlAsync(url);
    }

    private async Task DownloadFromUrlAsync(string? url)
    {
        if (url is null)
        {
            StatusText = "Check for updates first.";
            return;
        }

        try
        {
            _downloadCts = new CancellationTokenSource();
            IsDownloading = true;
            DownloadSizeText = "";
            StatusText = "Downloading...";
            var progress = new Progress<double>(p =>
            {
                DownloadProgress = p * 100;
                DownloadSizeText = $"{p * 100:F0}%";
            });
            _downloadedFilePath = await _downloader.DownloadAsync(url, progress, _downloadCts.Token);
            StatusText = $"Downloaded to {_downloadedFilePath}";
            DownloadSizeText = "Complete";
        }
        catch (OperationCanceledException)
        {
            StatusText = "Download cancelled.";
            DownloadSizeText = "";
        }
        catch (Exception ex)
        {
            StatusText = $"Download failed: {ex.Message}";
            DownloadSizeText = "";
        }
        finally
        {
            IsDownloading = false;
            _downloadCts?.Dispose();
            _downloadCts = null;
        }
    }

    [RelayCommand]
    private void CancelDownload()
    {
        _downloadCts?.Cancel();
    }

    [RelayCommand]
    private async Task RollbackDriverAsync()
    {
        if (_rollbackUrl is not null)
            await DownloadFromUrlAsync(_rollbackUrl);
    }

    [RelayCommand]
    private async Task LaunchInstallerAsync()
    {
        if (_downloadedFilePath is null || !File.Exists(_downloadedFilePath))
        {
            StatusText = "Download a driver first.";
            return;
        }

        try
        {
            if (ClearShaderCache)
            {
                StatusText = "Clearing shader cache...";
                await DriverDownloader.ClearShaderCacheAsync();
            }

            _downloader.LaunchInstaller(_downloadedFilePath, CleanInstall);
            StatusText = "Installer launched.";
        }
        catch (Exception ex)
        {
            StatusText = $"Failed to launch: {ex.Message}";
        }
    }
}

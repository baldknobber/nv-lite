using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using NVLite.Core.Monitoring;

namespace NVLite.App.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly HardwareMonitorService _monitor = new();
    private CancellationTokenSource? _cts;
    private SensorLogger? _sensorLogger;
    private DateTime _lastTempAlertTime;

    private static readonly SolidColorBrush GreenBrush = new(Colors.LimeGreen);
    private static readonly SolidColorBrush YellowBrush = new(Colors.Orange);
    private static readonly SolidColorBrush RedBrush = new(Colors.OrangeRed);

    [ObservableProperty] public partial string GpuName { get; set; } = "Detecting GPU...";
    [ObservableProperty] public partial string GpuTemp { get; set; } = "--";
    [ObservableProperty] public partial string GpuCoreClock { get; set; } = "--";
    [ObservableProperty] public partial string GpuMemClock { get; set; } = "--";
    [ObservableProperty] public partial string GpuPower { get; set; } = "--";
    [ObservableProperty] public partial double GpuUsage { get; set; }
    public string GpuUsageText => GpuUsage.ToString("F0");
    partial void OnGpuUsageChanged(double value) => OnPropertyChanged(nameof(GpuUsageText));
    [ObservableProperty] public partial double GpuMemUsed { get; set; }
    [ObservableProperty] public partial double GpuMemTotal { get; set; } = 1;
    [ObservableProperty] public partial string GpuFanSpeed { get; set; } = "--";
    [ObservableProperty] public partial SolidColorBrush GpuTempColor { get; set; } = GreenBrush;
    [ObservableProperty] public partial string GpuDriverVersion { get; set; } = "";

    public Microsoft.UI.Xaml.Visibility HasDriverVersion =>
        string.IsNullOrEmpty(GpuDriverVersion) ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;

    partial void OnGpuDriverVersionChanged(string value) => OnPropertyChanged(nameof(HasDriverVersion));

    [ObservableProperty] public partial string CpuName { get; set; } = "Detecting CPU...";
    [ObservableProperty] public partial string CpuTemp { get; set; } = "--";
    [ObservableProperty] public partial double CpuUsage { get; set; }
    public string CpuUsageText => CpuUsage.ToString("F0");
    partial void OnCpuUsageChanged(double value) => OnPropertyChanged(nameof(CpuUsageText));
    [ObservableProperty] public partial SolidColorBrush CpuTempColor { get; set; } = GreenBrush;

    [ObservableProperty] public partial string StatusText { get; set; } = "Starting...";
    [ObservableProperty] public partial string LastUpdated { get; set; } = "";

    public Microsoft.UI.Xaml.Visibility HasLastUpdated =>
        string.IsNullOrEmpty(LastUpdated) ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;

    partial void OnLastUpdatedChanged(string value) => OnPropertyChanged(nameof(HasLastUpdated));

    // Temperature alerts
    [ObservableProperty] public partial bool IsAlertActive { get; set; }
    [ObservableProperty] public partial string AlertMessage { get; set; } = "";

    public event Action<string, string>? NotificationRequested;

    // Sensor logging
    [ObservableProperty] public partial bool IsLogging { get; set; }
    [ObservableProperty] public partial string LoggingStatus { get; set; } = "";

    public string LoggingButtonText => IsLogging ? "Stop Logging" : "Start Logging";
    partial void OnIsLoggingChanged(bool value) => OnPropertyChanged(nameof(LoggingButtonText));

    private static SolidColorBrush GetTempBrush(float? temp)
    {
        if (temp is null) return GreenBrush;
        return temp switch
        {
            > 85 => RedBrush,
            > 70 => YellowBrush,
            _ => GreenBrush,
        };
    }

    public async Task StartMonitoringAsync()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        try
        {
            _monitor.Open();
            StatusText = "Monitoring active";
            GpuDriverVersion = _monitor.GetGpuDriverVersion() ?? "";

            var interval = TimeSpan.FromSeconds(
                Math.Max(1, App.Settings.Settings.PollingIntervalSeconds));
            using var timer = new PeriodicTimer(interval);
            while (await timer.WaitForNextTickAsync(_cts.Token))
            {
                var gpu = _monitor.GetGpuInfo();
                if (gpu is not null)
                {
                    GpuName = gpu.Name;
                    GpuTemp = gpu.Temperature?.ToString("F0") ?? "--";
                    GpuTempColor = GetTempBrush(gpu.Temperature);
                    GpuCoreClock = gpu.CoreClock?.ToString("F0") ?? "--";
                    GpuMemClock = gpu.MemoryClock?.ToString("F0") ?? "--";
                    GpuPower = gpu.PowerDraw?.ToString("F1") ?? "--";
                    GpuUsage = gpu.Usage ?? 0;
                    GpuMemUsed = gpu.MemoryUsed ?? 0;
                    GpuMemTotal = gpu.MemoryTotal > 0 ? gpu.MemoryTotal ?? 1 : 1;
                    GpuFanSpeed = gpu.FanSpeed?.ToString("F0") ?? "--";
                }

                var cpu = _monitor.GetCpuInfo();
                if (cpu is not null)
                {
                    CpuName = cpu.Name;
                    CpuTemp = cpu.PackageTemperature?.ToString("F0") ?? "--";
                    CpuTempColor = GetTempBrush(cpu.PackageTemperature);
                    CpuUsage = cpu.Usage ?? 0;
                }

                // Sensor logging
                _sensorLogger?.LogSample(gpu, cpu);

                // Temperature alert check
                CheckTempAlert(gpu?.Temperature);

                LastUpdated = DateTime.Now.ToString("h:mm:ss tt");
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
    }

    public void StopMonitoring()
    {
        _cts?.Cancel();
        _monitor.Close();
        _sensorLogger?.Stop();
        _sensorLogger?.Dispose();
        _sensorLogger = null;
        IsLogging = false;
    }

    [RelayCommand]
    private void ToggleLogging()
    {
        if (IsLogging)
        {
            var path = _sensorLogger?.FilePath;
            _sensorLogger?.Stop();
            _sensorLogger?.Dispose();
            _sensorLogger = null;
            IsLogging = false;
            LoggingStatus = $"Saved to {Path.GetFileName(path)}";
        }
        else
        {
            _sensorLogger = new SensorLogger();
            _sensorLogger.Start();
            IsLogging = true;
            LoggingStatus = $"Logging to {Path.GetFileName(_sensorLogger.FilePath)}";
        }
    }

    private void CheckTempAlert(float? gpuTemp)
    {
        var settings = App.Settings.Settings;
        if (!settings.EnableTempAlerts || gpuTemp is null)
        {
            IsAlertActive = false;
            return;
        }

        if (gpuTemp > settings.GpuTempAlertThreshold)
        {
            IsAlertActive = true;
            AlertMessage = $"GPU temperature is {gpuTemp:F0}°C (threshold: {settings.GpuTempAlertThreshold}°C)";

            // Cooldown — don't spam notifications more than once per 5 minutes
            if (DateTime.Now - _lastTempAlertTime > TimeSpan.FromMinutes(5))
            {
                _lastTempAlertTime = DateTime.Now;
                NotificationRequested?.Invoke("Temperature Alert", AlertMessage);
            }
        }
        else
        {
            IsAlertActive = false;
        }
    }
}

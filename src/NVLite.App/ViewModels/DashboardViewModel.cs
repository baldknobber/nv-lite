using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using NVLite.Core.Monitoring;

namespace NVLite.App.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly HardwareMonitorService _monitor = new();
    private CancellationTokenSource? _cts;

    private static readonly SolidColorBrush GreenBrush = new(Colors.LimeGreen);
    private static readonly SolidColorBrush YellowBrush = new(Colors.Orange);
    private static readonly SolidColorBrush RedBrush = new(Colors.OrangeRed);

    [ObservableProperty] public partial string GpuName { get; set; } = "Detecting GPU...";
    [ObservableProperty] public partial string GpuTemp { get; set; } = "--";
    [ObservableProperty] public partial string GpuCoreClock { get; set; } = "--";
    [ObservableProperty] public partial string GpuMemClock { get; set; } = "--";
    [ObservableProperty] public partial string GpuPower { get; set; } = "--";
    [ObservableProperty] public partial double GpuUsage { get; set; }
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
    [ObservableProperty] public partial SolidColorBrush CpuTempColor { get; set; } = GreenBrush;

    [ObservableProperty] public partial string StatusText { get; set; } = "Starting...";
    [ObservableProperty] public partial string LastUpdated { get; set; } = "";

    public Microsoft.UI.Xaml.Visibility HasLastUpdated =>
        string.IsNullOrEmpty(LastUpdated) ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;

    partial void OnLastUpdatedChanged(string value) => OnPropertyChanged(nameof(HasLastUpdated));

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
        _cts = new CancellationTokenSource();
        try
        {
            _monitor.Open();
            StatusText = "Monitoring active";
            GpuDriverVersion = _monitor.GetGpuDriverVersion() ?? "";

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
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
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using NVLite.Core.Monitoring;

namespace NVLite.App.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly HardwareMonitorService _monitor = new();
    private CancellationTokenSource? _cts;

    [ObservableProperty] public partial string GpuName { get; set; } = "Detecting GPU...";
    [ObservableProperty] public partial string GpuTemp { get; set; } = "--";
    [ObservableProperty] public partial string GpuCoreClock { get; set; } = "--";
    [ObservableProperty] public partial string GpuMemClock { get; set; } = "--";
    [ObservableProperty] public partial string GpuPower { get; set; } = "--";
    [ObservableProperty] public partial double GpuUsage { get; set; }
    [ObservableProperty] public partial double GpuMemUsed { get; set; }
    [ObservableProperty] public partial double GpuMemTotal { get; set; } = 1;
    [ObservableProperty] public partial string GpuFanSpeed { get; set; } = "--";

    [ObservableProperty] public partial string CpuName { get; set; } = "Detecting CPU...";
    [ObservableProperty] public partial string CpuTemp { get; set; } = "--";
    [ObservableProperty] public partial double CpuUsage { get; set; }

    [ObservableProperty] public partial string StatusText { get; set; } = "Starting...";

    public async Task StartMonitoringAsync()
    {
        _cts = new CancellationTokenSource();
        try
        {
            _monitor.Open();
            StatusText = "Monitoring active";

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            while (await timer.WaitForNextTickAsync(_cts.Token))
            {
                var gpu = _monitor.GetGpuInfo();
                if (gpu is not null)
                {
                    GpuName = gpu.Name;
                    GpuTemp = gpu.Temperature?.ToString("F0") ?? "--";
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
                    CpuUsage = cpu.Usage ?? 0;
                }
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

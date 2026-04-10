using LibreHardwareMonitor.Hardware;

namespace NVLite.Core.Monitoring;

public sealed class HardwareMonitorService : IDisposable
{
    private readonly Computer _computer = new()
    {
        IsCpuEnabled = true,
        IsGpuEnabled = true,
    };
    private bool _isOpen;

    public void Open()
    {
        if (_isOpen) return;
        _computer.Open();
        _isOpen = true;
    }

    public void Close()
    {
        if (!_isOpen) return;
        _computer.Close();
        _isOpen = false;
    }

    public GpuInfo? GetGpuInfo()
    {
        if (!_isOpen) return null;

        foreach (var hardware in _computer.Hardware)
        {
            if (hardware.HardwareType is not (HardwareType.GpuNvidia or HardwareType.GpuAmd or HardwareType.GpuIntel))
                continue;

            hardware.Update();

            float? temp = null, coreClock = null, memClock = null, usage = null;
            float? power = null, memUsed = null, memTotal = null, fanSpeed = null;

            foreach (var sensor in hardware.Sensors)
            {
                switch (sensor.SensorType)
                {
                    case SensorType.Temperature when sensor.Name.Contains("Core", StringComparison.OrdinalIgnoreCase):
                        temp ??= sensor.Value;
                        break;
                    case SensorType.Clock when sensor.Name.Contains("Core", StringComparison.OrdinalIgnoreCase):
                        coreClock ??= sensor.Value;
                        break;
                    case SensorType.Clock when sensor.Name.Contains("Memory", StringComparison.OrdinalIgnoreCase):
                        memClock ??= sensor.Value;
                        break;
                    case SensorType.Load when sensor.Name.Contains("Core", StringComparison.OrdinalIgnoreCase):
                        usage ??= sensor.Value;
                        break;
                    case SensorType.Power when sensor.Name.Contains("Package", StringComparison.OrdinalIgnoreCase)
                                            || sensor.Name.Contains("GPU", StringComparison.OrdinalIgnoreCase):
                        power ??= sensor.Value;
                        break;
                    case SensorType.SmallData when sensor.Name.Contains("Used", StringComparison.OrdinalIgnoreCase):
                        memUsed ??= sensor.Value;
                        break;
                    case SensorType.SmallData when sensor.Name.Contains("Total", StringComparison.OrdinalIgnoreCase):
                        memTotal ??= sensor.Value;
                        break;
                    case SensorType.Fan:
                        fanSpeed ??= sensor.Value;
                        break;
                }
            }

            return new GpuInfo
            {
                Name = hardware.Name,
                Temperature = temp,
                CoreClock = coreClock,
                MemoryClock = memClock,
                Usage = usage,
                PowerDraw = power,
                MemoryUsed = memUsed,
                MemoryTotal = memTotal,
                FanSpeed = fanSpeed,
            };
        }

        return null;
    }

    public CpuInfo? GetCpuInfo()
    {
        if (!_isOpen) return null;

        foreach (var hardware in _computer.Hardware)
        {
            if (hardware.HardwareType is not HardwareType.Cpu)
                continue;

            hardware.Update();

            float? packageTemp = null, usage = null, freq = null;

            foreach (var sensor in hardware.Sensors)
            {
                switch (sensor.SensorType)
                {
                    case SensorType.Temperature when sensor.Name.Contains("Package", StringComparison.OrdinalIgnoreCase)
                                                  || sensor.Name.Contains("Core (Tctl", StringComparison.OrdinalIgnoreCase):
                        packageTemp ??= sensor.Value;
                        break;
                    case SensorType.Temperature when packageTemp is null && sensor.Name.Contains("Core", StringComparison.OrdinalIgnoreCase):
                        packageTemp ??= sensor.Value;
                        break;
                    case SensorType.Load when sensor.Name.Contains("Total", StringComparison.OrdinalIgnoreCase):
                        usage ??= sensor.Value;
                        break;
                    case SensorType.Clock when freq is null:
                        freq ??= sensor.Value;
                        break;
                }
            }

            return new CpuInfo
            {
                Name = hardware.Name,
                PackageTemperature = packageTemp,
                Usage = usage,
                Frequency = freq,
            };
        }

        return null;
    }

    public string? GetGpuDriverVersion()
    {
        if (!_isOpen) return null;

        foreach (var hardware in _computer.Hardware)
        {
            if (hardware.HardwareType is HardwareType.GpuNvidia or HardwareType.GpuAmd or HardwareType.GpuIntel)
            {
                // Try to get driver version from registry (NVIDIA stores it here)
                try
                {
                    using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Device Manager");
                    // Fallback: use WMI-free approach via display adapter registry
                    foreach (var subKeyName in Microsoft.Win32.Registry.LocalMachine
                        .OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}")
                        ?.GetSubKeyNames() ?? [])
                    {
                        if (!int.TryParse(subKeyName, out _)) continue;
                        using var adapterKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                            $@"SYSTEM\CurrentControlSet\Control\Class\{{4d36e968-e325-11ce-bfc1-08002be10318}}\{subKeyName}");
                        var desc = adapterKey?.GetValue("DriverDesc") as string;
                        if (desc is null || !desc.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase)) continue;
                        var version = adapterKey?.GetValue("DriverVersion") as string;
                        if (version is not null)
                        {
                            // Convert Windows driver version (e.g. 32.0.15.7675) to NVIDIA format (576.75)
                            var parts = version.Split('.');
                            if (parts.Length >= 4)
                            {
                                var last5 = parts[2][^2..] + parts[3];
                                if (last5.Length == 5)
                                    return $"{last5[..3]}.{last5[3..]}";
                            }
                            return version;
                        }
                    }
                }
                catch { /* Registry access may fail, return null */ }
                break;
            }
        }

        return null;
    }

    public void Dispose() => Close();
}

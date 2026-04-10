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
            foreach (var sub in hardware.SubHardware)
                sub.Update();

            float? temp = null, hotSpot = null, memJunction = null;
            float? coreClock = null, memClock = null;
            float? usage = null, memCtrlLoad = null, videoLoad = null;
            float? power = null, powerLimit = null, voltage = null;
            float? memUsed = null, memTotal = null;
            float? fanSpeed = null, fanPercent = null;

            var allSensors = hardware.Sensors
                .Concat(hardware.SubHardware.SelectMany(s => s.Sensors));

            foreach (var sensor in allSensors)
            {
                var name = sensor.Name;
                switch (sensor.SensorType)
                {
                    // Temperatures
                    case SensorType.Temperature when name.Contains("Hot Spot", StringComparison.OrdinalIgnoreCase)
                                                  || name.Contains("Hotspot", StringComparison.OrdinalIgnoreCase):
                        hotSpot ??= sensor.Value;
                        break;
                    case SensorType.Temperature when name.Contains("Memory Junction", StringComparison.OrdinalIgnoreCase)
                                                  || name.Contains("Mem Junction", StringComparison.OrdinalIgnoreCase):
                        memJunction ??= sensor.Value;
                        break;
                    case SensorType.Temperature when name.Contains("Core", StringComparison.OrdinalIgnoreCase)
                                                  || name.Contains("GPU", StringComparison.OrdinalIgnoreCase):
                        temp ??= sensor.Value;
                        break;

                    // Clocks
                    case SensorType.Clock when name.Contains("Core", StringComparison.OrdinalIgnoreCase)
                                            || name.Contains("GPU", StringComparison.OrdinalIgnoreCase):
                        coreClock ??= sensor.Value;
                        break;
                    case SensorType.Clock when name.Contains("Memory", StringComparison.OrdinalIgnoreCase):
                        memClock ??= sensor.Value;
                        break;

                    // Load
                    case SensorType.Load when name.Contains("Core", StringComparison.OrdinalIgnoreCase)
                                           || name.Contains("GPU", StringComparison.OrdinalIgnoreCase)
                                           || name.Contains("D3D 3D", StringComparison.OrdinalIgnoreCase):
                        usage ??= sensor.Value;
                        break;
                    case SensorType.Load when name.Contains("Memory Controller", StringComparison.OrdinalIgnoreCase):
                        memCtrlLoad ??= sensor.Value;
                        break;
                    case SensorType.Load when name.Contains("Video", StringComparison.OrdinalIgnoreCase):
                        videoLoad ??= sensor.Value;
                        break;

                    // Power
                    case SensorType.Power when name.Contains("Limit", StringComparison.OrdinalIgnoreCase):
                        powerLimit ??= sensor.Value;
                        break;
                    case SensorType.Power:
                        power ??= sensor.Value;
                        break;

                    // Voltage
                    case SensorType.Voltage:
                        voltage ??= sensor.Value;
                        break;

                    // Memory
                    case SensorType.SmallData when name.Contains("Used", StringComparison.OrdinalIgnoreCase):
                        memUsed ??= sensor.Value;
                        break;
                    case SensorType.SmallData when name.Contains("Total", StringComparison.OrdinalIgnoreCase):
                        memTotal ??= sensor.Value;
                        break;

                    // Fan
                    case SensorType.Fan:
                        fanSpeed ??= sensor.Value;
                        break;
                    case SensorType.Control:
                        fanPercent ??= sensor.Value;
                        break;
                }
            }

            return new GpuInfo
            {
                Name = hardware.Name,
                Temperature = Positive(temp),
                HotSpotTemperature = Positive(hotSpot),
                MemoryJunctionTemperature = Positive(memJunction),
                CoreClock = Positive(coreClock),
                MemoryClock = Positive(memClock),
                Usage = usage,
                MemoryControllerLoad = memCtrlLoad,
                VideoEngineLoad = videoLoad,
                PowerDraw = Positive(power),
                PowerLimit = Positive(powerLimit),
                Voltage = Positive(voltage),
                MemoryUsed = memUsed,
                MemoryTotal = memTotal,
                FanSpeed = Positive(fanSpeed),
                FanPercent = fanPercent,
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
            foreach (var sub in hardware.SubHardware)
                sub.Update();

            float? packageTemp = null, usage = null, freq = null;
            float? voltage = null, power = null;

            // Per-core tracking
            var coreTemps = new Dictionary<int, float>();
            var coreClocks = new Dictionary<int, float>();
            var coreLoads = new Dictionary<int, float>();

            var allSensors = hardware.Sensors
                .Concat(hardware.SubHardware.SelectMany(s => s.Sensors));

            foreach (var sensor in allSensors)
            {
                var name = sensor.Name;
                switch (sensor.SensorType)
                {
                    // Package / Tctl / Tdie temperature
                    case SensorType.Temperature when name.Contains("Package", StringComparison.OrdinalIgnoreCase)
                                                  || name.Contains("Tctl", StringComparison.OrdinalIgnoreCase)
                                                  || name.Contains("Tdie", StringComparison.OrdinalIgnoreCase):
                        packageTemp ??= sensor.Value;
                        break;
                    case SensorType.Temperature when packageTemp is null
                                                  && name.Contains("CCD", StringComparison.OrdinalIgnoreCase):
                        packageTemp ??= sensor.Value;
                        break;
                    // Per-core temps: "Core #0", "Core #1", etc.
                    case SensorType.Temperature when TryParseCoreIndex(name, out var idx):
                        if (sensor.Value is float tv)
                            coreTemps.TryAdd(idx, tv);
                        packageTemp ??= sensor.Value; // fallback if no package sensor
                        break;

                    // Total CPU load
                    case SensorType.Load when name.Contains("Total", StringComparison.OrdinalIgnoreCase):
                        usage ??= sensor.Value;
                        break;
                    // Per-core loads
                    case SensorType.Load when TryParseCoreIndex(name, out var idx):
                        if (sensor.Value is float lv)
                            coreLoads.TryAdd(idx, lv);
                        break;

                    // Per-core clocks — pick highest as "frequency", also track per-core
                    case SensorType.Clock when TryParseCoreIndex(name, out var idx):
                        if (sensor.Value is float cv)
                        {
                            coreClocks.TryAdd(idx, cv);
                            if (freq is null || cv > freq)
                                freq = cv;
                        }
                        break;
                    case SensorType.Clock when name.Contains("Bus", StringComparison.OrdinalIgnoreCase):
                        break; // skip bus speed
                    case SensorType.Clock when freq is null:
                        freq ??= sensor.Value;
                        break;

                    // Voltage
                    case SensorType.Voltage when name.Contains("Core", StringComparison.OrdinalIgnoreCase)
                                              || name.Contains("VCore", StringComparison.OrdinalIgnoreCase):
                        voltage ??= sensor.Value;
                        break;
                    case SensorType.Voltage when voltage is null:
                        voltage ??= sensor.Value;
                        break;

                    // Power
                    case SensorType.Power when name.Contains("Package", StringComparison.OrdinalIgnoreCase)
                                            || name.Contains("CPU", StringComparison.OrdinalIgnoreCase):
                        power ??= sensor.Value;
                        break;
                    case SensorType.Power when power is null:
                        power ??= sensor.Value;
                        break;
                }
            }

            // Build per-core details
            var coreIndices = coreTemps.Keys
                .Union(coreClocks.Keys)
                .Union(coreLoads.Keys)
                .Order()
                .ToList();

            var cores = coreIndices.Select(i => new CoreDetail
            {
                Name = $"Core #{i}",
                Temperature = coreTemps.TryGetValue(i, out var ct) && ct > 0 ? ct : null,
                Clock = coreClocks.TryGetValue(i, out var cc) && cc > 0 ? cc : null,
                Load = coreLoads.TryGetValue(i, out var cl) ? cl : null,
            }).ToList();

            return new CpuInfo
            {
                Name = hardware.Name,
                PackageTemperature = Positive(packageTemp),
                Usage = usage,
                Frequency = Positive(freq),
                Voltage = Positive(voltage),
                PowerDraw = Positive(power),
                CoreCount = cores.Count > 0 ? cores.Count : Environment.ProcessorCount,
                ThreadCount = Environment.ProcessorCount,
                Cores = cores,
            };
        }

        return null;
    }

    private static bool TryParseCoreIndex(string sensorName, out int index)
    {
        // Matches "Core #0", "CPU Core #12", etc.
        index = 0;
        var hashIdx = sensorName.IndexOf('#');
        if (hashIdx < 0 || hashIdx + 1 >= sensorName.Length) return false;
        var numPart = sensorName.AsSpan(hashIdx + 1);
        // Trim trailing non-digit chars
        int len = 0;
        while (len < numPart.Length && char.IsDigit(numPart[len])) len++;
        return len > 0 && int.TryParse(numPart[..len], out index);
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
                            // Convert Windows driver version (e.g. 32.0.15.9579) to NVIDIA format (595.79)
                            var parts = version.Split('.');
                            if (parts.Length >= 4)
                            {
                                var combined = parts[2] + parts[3];
                                if (combined.Length >= 5)
                                {
                                    var last5 = combined[^5..];
                                    return $"{last5[..3]}.{last5[3..]}";
                                }
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

    /// <summary>Returns null if the value is null or &lt;= 0 (physically impossible for temps, clocks, voltage).</summary>
    private static float? Positive(float? v) => v is > 0 ? v : null;
}

using System.Globalization;
using System.Text;

namespace NVLite.Core.Monitoring;

public sealed class SensorLogger : IDisposable
{
    private StreamWriter? _writer;

    public string FilePath { get; }
    public bool IsLogging => _writer is not null;

    public SensorLogger(string? outputPath = null)
    {
        FilePath = outputPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads",
            $"nvlite-sensors-{DateTime.Now:yyyyMMdd-HHmmss}.csv");
    }

    public void Start()
    {
        if (_writer is not null) return;

        var dir = Path.GetDirectoryName(FilePath);
        if (dir is not null) Directory.CreateDirectory(dir);

        _writer = new StreamWriter(FilePath, append: false, Encoding.UTF8);
        _writer.WriteLine("Timestamp,GPU Name,GPU Temp (°C),GPU Core Clock (MHz),GPU Mem Clock (MHz),GPU Power (W),GPU Usage (%),GPU VRAM Used (MB),GPU VRAM Total (MB),GPU Fan (RPM),CPU Name,CPU Temp (°C),CPU Usage (%)");
        _writer.Flush();
    }

    public void LogSample(GpuInfo? gpu, CpuInfo? cpu)
    {
        if (_writer is null) return;

        var line = string.Join(",",
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            Escape(gpu?.Name ?? ""),
            Format(gpu?.Temperature),
            Format(gpu?.CoreClock),
            Format(gpu?.MemoryClock),
            Format(gpu?.PowerDraw),
            Format(gpu?.Usage),
            Format(gpu?.MemoryUsed),
            Format(gpu?.MemoryTotal),
            Format(gpu?.FanSpeed),
            Escape(cpu?.Name ?? ""),
            Format(cpu?.PackageTemperature),
            Format(cpu?.Usage));

        _writer.WriteLine(line);
        _writer.Flush();
    }

    public void Stop()
    {
        _writer?.Dispose();
        _writer = null;
    }

    public void Dispose() => Stop();

    private static string Format(float? value) =>
        value?.ToString("F1", CultureInfo.InvariantCulture) ?? "";

    private static string Format(double? value) =>
        value?.ToString("F1", CultureInfo.InvariantCulture) ?? "";

    private static string Escape(string value) =>
        value.Contains(',') || value.Contains('"')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
}

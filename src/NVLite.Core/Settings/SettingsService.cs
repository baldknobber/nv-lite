using System.Text.Json;

namespace NVLite.Core.Settings;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;
    private AppSettings _settings;

    public SettingsService(string? basePath = null)
    {
        var dir = basePath ?? AppContext.BaseDirectory;
        _filePath = Path.Combine(dir, "settings.json");
        _settings = Load();
    }

    public AppSettings Settings => _settings;

    public void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings, JsonOptions);
            File.WriteAllText(_filePath, json);
        }
        catch (IOException)
        {
            // Portable EXE may be in a read-only location — silently ignore
        }
    }

    public void Reload()
    {
        _settings = Load();
    }

    private AppSettings Load()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            // Corrupt or unreadable — fall back to defaults
        }

        return new AppSettings();
    }
}

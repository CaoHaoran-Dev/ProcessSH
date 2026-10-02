using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProcessSH.Models;

public sealed class AppSettings
{
    private static readonly string SettingsDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ProcessSH");

    private static readonly string SettingsPath =
        Path.Combine(SettingsDir, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private static AppSettings? _instance;
    private static readonly object _lock = new();

    public static AppSettings Current
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = Load();
                    Localization.Current = _instance.Language;
                    Localization.Initialize();
                }
                return _instance;
            }
        }
    }

    public string DefaultWorkingDirectory { get; set; } = string.Empty;
    public bool HideOnDeactivate { get; set; } = true;
    public string ToggleHotkey { get; set; } = "Ctrl+Win+R";
    public AppLanguage Language { get; set; } = AppLanguage.SimplifiedChinese;

    [JsonIgnore]
    public string ResolvedWorkingDirectory
    {
        get
        {
            if (string.IsNullOrWhiteSpace(DefaultWorkingDirectory))
                return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            try
            {
                var expanded = Environment.ExpandEnvironmentVariables(DefaultWorkingDirectory);
                if (Directory.Exists(expanded))
                    return expanded;
            }
            catch { }

            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }
    }

    [JsonIgnore]
    public bool IsWorkingDirectoryValid
    {
        get
        {
            if (string.IsNullOrWhiteSpace(DefaultWorkingDirectory))
                return true;
            try
            {
                var expanded = Environment.ExpandEnvironmentVariables(DefaultWorkingDirectory);
                return Directory.Exists(expanded);
            }
            catch { return false; }
        }
    }

    private static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var loaded = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                if (loaded != null) return loaded;
            }
        }
        catch { }

        return new AppSettings();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsDir);
            var json = JsonSerializer.Serialize(this, JsonOptions);
            File.WriteAllText(SettingsPath, json);
        }
        catch { }
    }

    public static void Reload()
    {
        lock (_lock)
        {
            _instance = Load();
            if (_instance != null)
            {
                Localization.Current = _instance.Language;
                Localization.Initialize();
            }
        }
    }
}
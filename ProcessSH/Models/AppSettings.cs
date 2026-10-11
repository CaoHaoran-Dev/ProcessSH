using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProcessSH.Models;

public sealed class AppSettings
{
    private static readonly string SettingsDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ProcessSH");

    private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
    };

    private static AppSettings? _instance;
    private static readonly object _lock = new();

    public static AppSettings Current
    {
        get
        {
            lock (_lock)
            {
                _instance ??= Load();
                return _instance;
            }
        }
    }

    // ─── 已有 ───
    public string DefaultWorkingDirectory { get; set; } = string.Empty;
    public bool HideOnDeactivate { get; set; } = true;
    public string ToggleHotkey { get; set; } = "Ctrl+Alt+R";
    public AppLanguage Language { get; set; } = AppLanguage.SimplifiedChinese;

    // ─── 对齐 RunProcess v3.4.0 ───

    /// <summary>默认以管理员身份执行。</summary>
    public bool DefaultSudo { get; set; } = false;

    /// <summary>开机自启动。</summary>
    public bool LaunchAtLogin { get; set; } = false;

    /// <summary>启动后隐藏窗口（只留托盘）。</summary>
    public bool HideWindowOnLaunch { get; set; } = false;

    /// <summary>输出显示 ANSI 颜色。</summary>
    public bool OutputColorsEnabled { get; set; } = true;

    /// <summary>输出颜色色板。</summary>
    public OutputColorScheme OutputColorScheme { get; set; } = OutputColorScheme.Auto;

    /// <summary>窗口背景材质。</summary>
    public WindowMaterial BackgroundMaterial { get; set; } = WindowMaterial.Mica;

    // ─── 解析辅助 ───

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
                if (Directory.Exists(expanded)) return expanded;
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
            if (string.IsNullOrWhiteSpace(DefaultWorkingDirectory)) return true;
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AppSettings] Load 失败: {ex.Message}");
        }
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AppSettings] Save 失败: {ex.Message}");
        }
    }

    public static void Reload()
    {
        lock (_lock) _instance = Load();
    }
}

public enum OutputColorScheme
{
    Auto,
    Dark,
    Light,
}

public enum WindowMaterial
{
    Mica,
    MicaAlt,
    Acrylic,
    None,
}
using System.Text.Json;

namespace ProcessSH.Models;

public enum AppLanguage
{
    English,
    SimplifiedChinese,
    TraditionalChinese,
}

/// <summary>
/// 运行时本地化。语言词典从 Assets/Localization/*.json 加载。
/// </summary>
public static class Localization
{
    public static event Action? LanguageChanged;

    private static AppLanguage _current = AppLanguage.SimplifiedChinese;
    private static Dictionary<string, string> _table = new();
    private static readonly object _lock = new();

    public static AppLanguage Current
    {
        get => _current;
        set
        {
            if (_current == value) return;
            _current = value;
            ReloadTable();
            LanguageChanged?.Invoke();
        }
    }

    public static string Get(string key)
    {
        lock (_lock)
        {
            if (_table.TryGetValue(key, out var value))
                return value;
        }
        return key;
    }

    /// <summary>首次使用时调一次，加载当前语言的词典</summary>
    public static void Initialize()
    {
        ReloadTable();
    }

    private static void ReloadTable()
    {
        var fileName = _current switch
        {
            AppLanguage.English => "en.json",
            AppLanguage.TraditionalChinese => "zh-Hant.json",
            _ => "zh-Hans.json",
        };

        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "Localization", fileName);

        try
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (dict != null)
                {
                    lock (_lock) { _table = dict; }
                    return;
                }
            }
        }
        catch { }

        // 加载失败时回退到英文
        lock (_lock) { _table = new Dictionary<string, string>(); }
    }
}
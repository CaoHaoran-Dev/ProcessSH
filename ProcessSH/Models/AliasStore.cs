using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProcessSH.Models;

public sealed class CommandAlias
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("expansion")]
    public string Expansion { get; set; } = string.Empty;
}

public sealed class AliasStore
{
    private static readonly string AppDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ProcessSH");

    private static readonly string FilePath = Path.Combine(AppDir, "aliases.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    private static AliasStore? _shared;
    private static readonly object _lock = new();

    public static AliasStore Shared
    {
        get
        {
            lock (_lock)
            {
                _shared ??= new AliasStore();
                return _shared;
            }
        }
    }

    private readonly List<CommandAlias> _aliases = new();
    private readonly object _listLock = new();

    private AliasStore()
    {
        Load();
        if (_aliases.Count == 0)
        {
            _aliases.AddRange(new[]
            {
                new CommandAlias { Name = "gs",    Expansion = "git status" },
                new CommandAlias { Name = "gp",    Expansion = "git pull --rebase" },
                new CommandAlias { Name = "ll",    Expansion = "Get-ChildItem -Force" },
                new CommandAlias { Name = "serve", Expansion = "python3 -m http.server 8000" },
                new CommandAlias { Name = "which", Expansion = "Get-Command" },
            });
            Save();
        }
    }

    public IReadOnlyList<CommandAlias> Aliases
    {
        get { lock (_listLock) return _aliases.ToList(); }
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return;
            var json = File.ReadAllText(FilePath);
            var loaded = JsonSerializer.Deserialize<List<CommandAlias>>(json, JsonOptions);
            if (loaded != null)
            {
                lock (_listLock)
                {
                    _aliases.Clear();
                    _aliases.AddRange(loaded);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AliasStore] Load 失败: {ex.Message}");
        }
    }

    public void Reload() => Load();

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(AppDir);
            List<CommandAlias> snapshot;
            lock (_listLock) snapshot = _aliases.ToList();
            var json = JsonSerializer.Serialize(snapshot, JsonOptions);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AliasStore] Save 失败: {ex.Message}");
        }
    }

    public void Add(CommandAlias alias)
    {
        if (string.IsNullOrWhiteSpace(alias.Name)) return;
        lock (_listLock)
        {
            var idx = _aliases.FindIndex(a => a.Name == alias.Name);
            if (idx >= 0) _aliases[idx] = alias;
            else _aliases.Add(alias);
        }
        Save();
    }

    public void Remove(string name)
    {
        lock (_listLock) _aliases.RemoveAll(a => a.Name == name);
        Save();
    }

    public void ReplaceAll(IEnumerable<CommandAlias> aliases)
    {
        lock (_listLock)
        {
            _aliases.Clear();
            _aliases.AddRange(aliases);
        }
        Save();
    }

    public List<CommandAlias> Match(string prefix)
    {
        if (string.IsNullOrEmpty(prefix)) return new();
        lock (_listLock)
            return _aliases.Where(a => a.Name.StartsWith(prefix, StringComparison.Ordinal)).ToList();
    }

    public string? Expansion(string name)
    {
        lock (_listLock)
            return _aliases.FirstOrDefault(a => a.Name == name)?.Expansion;
    }
}
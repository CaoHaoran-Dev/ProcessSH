using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProcessSH.Models;

public sealed class HistoryEntry
{
    [JsonPropertyName("command")]
    public string Command { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    public int Count { get; set; } = 1;

    [JsonPropertyName("lastUsed")]
    public DateTime LastUsed { get; set; } = DateTime.UtcNow;

    public HistoryEntry() { }

    public HistoryEntry(string command)
    {
        Command = command;
        Count = 1;
        LastUsed = DateTime.UtcNow;
    }

    public void RecordUsage()
    {
        Count++;
        LastUsed = DateTime.UtcNow;
    }

    [JsonIgnore]
    public double Frecency
    {
        get
        {
            var days = Math.Max(0, (DateTime.UtcNow - LastUsed).TotalDays);
            return Count / (1.0 + days);
        }
    }
}

public sealed class CommandHistory
{
    private const int MaxEntries = 500;

    private static readonly string AppDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ProcessSH");

    private static readonly string FilePath = Path.Combine(AppDir, "history.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static CommandHistory Shared { get; } = new();

    private readonly Dictionary<string, HistoryEntry> _entries = new(StringComparer.Ordinal);
    private readonly object _lock = new();
    private readonly object _saveLock = new();

    private int _saveScheduled;

    private CommandHistory() => Load();

    private void Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return;
            var json = File.ReadAllText(FilePath);
            var decoded = JsonSerializer.Deserialize<Dictionary<string, HistoryEntry>>(json, JsonOptions);
            if (decoded == null) return;

            lock (_lock)
            {
                _entries.Clear();
                foreach (var kv in decoded) _entries[kv.Key] = kv.Value;
            }
        }
        catch
        {
            lock (_lock) _entries.Clear();
        }
    }

    private void ScheduleSave()
    {
        if (Interlocked.Exchange(ref _saveScheduled, 1) == 1) return;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(500).ConfigureAwait(false);
                Save();
            }
            finally
            {
                Interlocked.Exchange(ref _saveScheduled, 0);
            }
        });
    }

    private void Save()
    {
        lock (_saveLock)
        {
            try
            {
                Directory.CreateDirectory(AppDir);
                Dictionary<string, HistoryEntry> snapshot;
                lock (_lock) snapshot = new(_entries, StringComparer.Ordinal);
                var json = JsonSerializer.Serialize(snapshot, JsonOptions);
                File.WriteAllText(FilePath, json);
            }
            catch { }
        }
    }

    public void Record(string command)
    {
        var trimmed = command.Trim();
        if (trimmed.Length == 0) return;

        lock (_lock)
        {
            if (_entries.TryGetValue(trimmed, out var existing))
            {
                existing.RecordUsage();
            }
            else
            {
                if (_entries.Count >= MaxEntries)
                {
                    var oldestKey = _entries.OrderBy(kv => kv.Value.LastUsed).FirstOrDefault().Key;
                    if (oldestKey != null) _entries.Remove(oldestKey);
                }
                _entries[trimmed] = new HistoryEntry(trimmed);
            }
        }

        ScheduleSave();
    }

    public List<HistoryEntry> Query(string prefix)
    {
        if (string.IsNullOrEmpty(prefix)) return new();

        List<HistoryEntry> snapshot;
        lock (_lock) snapshot = _entries.Values.ToList();

        return snapshot
            .Where(e => e.Command.StartsWith(prefix, StringComparison.Ordinal))
            .OrderByDescending(e => e.Count)
            .Take(20)
            .ToList();
    }

    /// <summary>按 frecency 排序（频次 × 时效）。</summary>
    public List<HistoryEntry> QueryByFrecency(string prefix)
    {
        if (string.IsNullOrEmpty(prefix)) return new();

        List<HistoryEntry> snapshot;
        lock (_lock) snapshot = _entries.Values.ToList();

        return snapshot
            .Where(e => e.Command.StartsWith(prefix, StringComparison.Ordinal))
            .OrderByDescending(e => e.Frecency)
            .Take(20)
            .ToList();
    }

    /// <summary>模糊搜索（字符顺序匹配）。</summary>
    public List<HistoryEntry> Search(string query)
    {
        List<HistoryEntry> snapshot;
        lock (_lock) snapshot = _entries.Values.ToList();

        var q = query?.ToLowerInvariant() ?? string.Empty;

        if (string.IsNullOrEmpty(q))
            return snapshot.OrderByDescending(e => e.Frecency).Take(50).ToList();

        return snapshot
            .Where(e => FuzzyMatch(q, e.Command.ToLowerInvariant()))
            .OrderByDescending(e => e.Frecency)
            .Take(50)
            .ToList();
    }

    private static bool FuzzyMatch(string query, string text)
    {
        int qi = 0;
        foreach (var ch in text)
        {
            if (qi < query.Length && ch == query[qi]) qi++;
        }
        return qi == query.Length;
    }

    public void ClearAll()
    {
        lock (_lock) _entries.Clear();
        Save();
    }

    public int Count()
    {
        lock (_lock) return _entries.Count;
    }

    public List<HistoryEntry> GetAll()
    {
        lock (_lock) return _entries.Values.ToList();
    }
}
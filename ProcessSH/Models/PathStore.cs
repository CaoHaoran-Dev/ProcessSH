using System.Text.Json;

namespace ProcessSH.Models;

/// <summary>
/// 用户自定义可执行文件搜索路径。
/// Windows 版：使用 `;` 分隔 PATH，识别 PATHEXT 里的可执行文件。
/// </summary>
public sealed class PathStore
{
    private static readonly string AppDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ProcessSH");

    private static readonly string FilePath = Path.Combine(AppDir, "paths.json");

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private sealed class PathFile
    {
        public List<string> Paths { get; set; } = new();
    }

    private static PathStore? _shared;
    private static readonly object _lock = new();

    public static PathStore Shared
    {
        get
        {
            lock (_lock)
            {
                _shared ??= new PathStore();
                return _shared;
            }
        }
    }

    private readonly List<string> _paths = new();
    private readonly object _listLock = new();

    private PathStore() => Load();

    public IReadOnlyList<string> Paths
    {
        get { lock (_listLock) return _paths.ToList(); }
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return;
            var json = File.ReadAllText(FilePath);
            var file = JsonSerializer.Deserialize<PathFile>(json, JsonOptions);
            if (file?.Paths != null)
            {
                lock (_listLock)
                {
                    _paths.Clear();
                    _paths.AddRange(file.Paths);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PathStore] Load 失败: {ex.Message}");
        }
    }

    public void Reload() => Load();

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(AppDir);
            List<string> snapshot;
            lock (_listLock) snapshot = _paths.ToList();
            var json = JsonSerializer.Serialize(new PathFile { Paths = snapshot }, JsonOptions);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PathStore] Save 失败: {ex.Message}");
        }
    }

    public void Add(string path)
    {
        var normalized = Normalize(path);
        if (string.IsNullOrEmpty(normalized)) return;
        lock (_listLock)
        {
            if (_paths.Contains(normalized, StringComparer.OrdinalIgnoreCase)) return;
            _paths.Add(normalized);
        }
        Save();
    }

    public void Remove(string path)
    {
        lock (_listLock) _paths.RemoveAll(p => p.Equals(path, StringComparison.OrdinalIgnoreCase));
        Save();
    }

    public void ReplaceAll(IEnumerable<string> paths)
    {
        lock (_listLock)
        {
            _paths.Clear();
            foreach (var p in paths)
            {
                var n = Normalize(p);
                if (!string.IsNullOrEmpty(n) &&
                    !_paths.Contains(n, StringComparer.OrdinalIgnoreCase))
                    _paths.Add(n);
            }
        }
        Save();
    }

    public void Move(int from, int to)
    {
        lock (_listLock)
        {
            if (to < 0 || to >= _paths.Count || from < 0 || from >= _paths.Count) return;
            (_paths[from], _paths[to]) = (_paths[to], _paths[from]);
        }
        Save();
    }

    /// <summary>返回用户自定义路径的 PATH 字符串（分号分隔）。</summary>
    public string CustomPathString()
    {
        List<string> snapshot;
        lock (_listLock) snapshot = _paths.ToList();
        return string.Join(";", snapshot.Select(Expand));
    }

    /// <summary>在自定义路径中查找可执行文件（按 PATHEXT 顺序）。</summary>
    public string? FindExecutable(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        var exts = GetPathExts();
        List<string> snapshot;
        lock (_listLock) snapshot = _paths.ToList();

        foreach (var dir in snapshot)
        {
            var expanded = Expand(dir);
            if (!Directory.Exists(expanded)) continue;

            // 已带扩展名
            if (Path.HasExtension(name))
            {
                var full = Path.Combine(expanded, name);
                if (File.Exists(full)) return full;
                continue;
            }

            // 依次试 PATHEXT
            foreach (var ext in exts)
            {
                var full = Path.Combine(expanded, name + ext);
                if (File.Exists(full)) return full;
            }
        }
        return null;
    }

    public bool IsValid(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        var expanded = Expand(path);
        return Directory.Exists(expanded);
    }

    private static string[] GetPathExts()
    {
        var raw = Environment.GetEnvironmentVariable("PATHEXT") ?? ".COM;.EXE;.BAT;.CMD;.PS1";
        return raw.Split(';', StringSplitOptions.RemoveEmptyEntries)
                  .Select(e => e.Trim().ToLowerInvariant())
                  .ToArray();
    }

    private static string Expand(string path)
    {
        if (string.IsNullOrEmpty(path)) return string.Empty;
        return Environment.ExpandEnvironmentVariables(path);
    }

    /// <summary>把绝对路径转成 %USERPROFILE% 形式。</summary>
    private static string Normalize(string path)
    {
        if (string.IsNullOrEmpty(path)) return string.Empty;

        var expanded = Expand(path);
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        if (!string.IsNullOrEmpty(home))
        {
            if (expanded.Equals(home, StringComparison.OrdinalIgnoreCase))
                return "%USERPROFILE%";

            if (expanded.StartsWith(home + "\\", StringComparison.OrdinalIgnoreCase))
                return "%USERPROFILE%" + expanded.Substring(home.Length);
        }
        return expanded;
    }
}
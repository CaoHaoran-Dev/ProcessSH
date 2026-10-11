using ProcessSH.Utils;

namespace ProcessSH.Models;

public sealed class CommandSuggester
{
    private readonly CommandHistory _history;
    private List<string> _cachedCommands = new();
    private DateTime _lastCacheUpdate = DateTime.MinValue;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);
    private static readonly object _cacheLock = new();

    private static readonly HashSet<string> PathOnlyCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "cd", "chdir", "dir", "ls", "type", "cat", "copy", "move", "del", "rm",
        "mkdir", "md", "rmdir", "rd", "start", "explorer", "Set-Location",
    };

    public CommandSuggester(CommandHistory history)
    {
        _history = history;
    }

    public List<Suggestion> Suggest(string input)
    {
        var lastWord = GetLastWord(input);
        if (string.IsNullOrEmpty(lastWord)) return new();

        if (IsPathLike(input, lastWord))
            return SuggestPaths(lastWord);

        return SuggestCommandsHistoryAliases(lastWord);
    }

    private static string GetLastWord(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        bool inQuote = false;
        int lastSpace = -1;
        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (c == '"' || c == '\'') inQuote = !inQuote;
            else if (c == ' ' && !inQuote) lastSpace = i;
        }

        var word = lastSpace >= 0 ? input.Substring(lastSpace + 1) : input;
        return word.Trim('"', '\'');
    }

    private static bool IsPathLike(string input, string lastWord)
    {
        if (string.IsNullOrEmpty(lastWord)) return false;

        // 绝对/相对路径
        if (lastWord.StartsWith("\\") || lastWord.StartsWith("/") ||
            lastWord.StartsWith(".") ||
            lastWord.StartsWith("~"))
            return true;

        // 驱动器号 C:\ 
        if (lastWord.Length >= 2 && char.IsLetter(lastWord[0]) && lastWord[1] == ':')
            return true;

        // 环境变量 %VAR%
        if (lastWord.StartsWith("%"))
            return true;

        // 首词是 path-only 命令
        var firstWord = GetFirstWord(input);
        if (!string.IsNullOrEmpty(firstWord) && PathOnlyCommands.Contains(firstWord))
            return true;

        // 含路径分隔符
        if (lastWord.Contains('\\') || lastWord.Contains('/'))
            return true;

        return false;
    }

    private static string GetFirstWord(string input)
    {
        var idx = input.IndexOf(' ');
        return idx < 0 ? input : input.Substring(0, idx);
    }

    private List<Suggestion> SuggestPaths(string input)
    {
        try
        {
            var expanded = ShellQuoting.ExpandEnv(input);

            var dir = Path.GetDirectoryName(expanded);
            var partial = Path.GetFileName(expanded);

            // 纯驱动器 "C:" → 补成 "C:\"
            if (string.IsNullOrEmpty(dir) && expanded.Length == 2 && expanded[1] == ':')
            {
                dir = expanded + "\\";
                partial = string.Empty;
            }

            if (string.IsNullOrEmpty(dir)) dir = Environment.CurrentDirectory;

            if (!Directory.Exists(dir))
            {
                // 尝试在自定义路径中查找
                var exe = PathStore.Shared.FindExecutable(partial);
                if (exe != null)
                    return new List<Suggestion> { new(exe, SuggestionType.Path) };
                return new();
            }

            var entries = Directory.GetFileSystemEntries(dir, partial + "*");
            var results = new List<Suggestion>();

            foreach (var path in entries.Take(20))
            {
                var display = path;
                if (Directory.Exists(path)) display += "\\";
                results.Add(new Suggestion(ShellQuoting.Quote(display), SuggestionType.Path));
            }

            return results;
        }
        catch
        {
            return new();
        }
    }

    private List<Suggestion> SuggestCommandsHistoryAliases(string prefix)
    {
        var suggestions = new List<Suggestion>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1. 别名优先
        foreach (var alias in AliasStore.Shared.Match(prefix))
        {
            var key = alias.Expansion;
            if (seen.Add(key))
            {
                suggestions.Add(new Suggestion(alias.Expansion, SuggestionType.Alias, alias.Name));
            }
        }

        // 2. 历史（frecency）
        foreach (var entry in _history.QueryByFrecency(prefix))
        {
            if (seen.Add(entry.Command))
                suggestions.Add(new Suggestion(entry.Command, SuggestionType.History, entry.Count));
        }

        // 3. 系统命令
        foreach (var cmd in FindSystemCommands(prefix))
        {
            if (seen.Add(cmd))
                suggestions.Add(new Suggestion(cmd, SuggestionType.Command));
        }

        return suggestions.OrderByDescending(s => s.Priority).ToList();
    }

    private List<string> FindSystemCommands(string prefix)
    {
        lock (_cacheLock)
        {
            if (DateTime.UtcNow - _lastCacheUpdate < CacheTtl && _cachedCommands.Count > 0)
            {
                return _cachedCommands
                    .Where(c => c.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .Take(20)
                    .ToList();
            }

            var pathString = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            var paths = pathString.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var exts = (Environment.GetEnvironmentVariable("PATHEXT")
                        ?? ".COM;.EXE;.BAT;.CMD;.PS1")
                        .Split(';', StringSplitOptions.RemoveEmptyEntries);

            var all = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var dir in paths)
            {
                try
                {
                    if (!Directory.Exists(dir)) continue;

                    foreach (var file in Directory.GetFiles(dir))
                    {
                        var ext = Path.GetExtension(file);
                        if (string.IsNullOrEmpty(ext)) continue;
                        if (!exts.Contains(ext, StringComparer.OrdinalIgnoreCase)) continue;

                        var name = Path.GetFileNameWithoutExtension(file);
                        if (string.IsNullOrEmpty(name) || name.StartsWith(".")) continue;
                        if (seen.Add(name)) all.Add(name);
                    }
                }
                catch { }
            }

            _cachedCommands = all.OrderBy(c => c).ToList();
            _lastCacheUpdate = DateTime.UtcNow;

            return _cachedCommands
                .Where(c => c.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Take(20)
                .ToList();
        }
    }
}
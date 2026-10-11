using ProcessSH.Models;

namespace ProcessSH.Utils;

/// <summary>
/// 命令预处理：可执行文件识别、别名展开、交互式检测。
/// Windows 版：以 .exe/.lnk/.bat/.cmd/.ps1 为主，不再处理 .app。
/// </summary>
public static class CommandPreprocessor
{
    private static readonly string[] LaunchExtensions =
    {
        ".exe", ".lnk", ".bat", ".cmd", ".msc", ".cpl"
    };

    /// <summary>
    /// 主预处理：识别可启动文件并加 Start-Process 前缀。
    /// </summary>
    public static string Process(string input)
    {
        var trimmed = input.Trim();
        if (string.IsNullOrEmpty(trimmed)) return trimmed;

        var lower = trimmed.ToLowerInvariant();

        // 已经是 Start-Process / start / explorer / Invoke-Item，不动
        if (lower.StartsWith("start-process ") ||
            lower.StartsWith("start ") ||
            lower.StartsWith("explorer ") ||
            lower.StartsWith("invoke-item "))
            return trimmed;

        var unwrapped = UnwrapQuotes(trimmed);

        // .lnk / .exe / .bat / .cmd / .msc / .cpl 直接启动
        foreach (var ext in LaunchExtensions)
        {
            if (unwrapped.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
            {
                return $"Start-Process {ShellQuoting.QuotePowerShell(unwrapped)}";
            }
        }

        // .ps1 用 pwsh -File 执行（当前进程已经是 pwsh，直接调用即可）
        if (unwrapped.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
        {
            return $"& {ShellQuoting.QuotePowerShell(unwrapped)}";
        }

        return trimmed;
    }

    /// <summary>
    /// 在自定义路径中查找可执行文件。返回处理后的命令或原命令。
    /// </summary>
    public static string ResolveCustomPath(string command)
    {
        var parts = command.Split(' ', 2, StringSplitOptions.None);
        if (parts.Length == 0) return command;
        var name = parts[0];
        if (string.IsNullOrEmpty(name)) return command;

        // 已带路径分隔符或驱动器号，跳过
        if (name.Contains('\\') || name.Contains('/') || name.Contains(':'))
            return command;

        var exe = PathStore.Shared.FindExecutable(name);
        if (exe == null) return command;

        var quoted = ShellQuoting.QuotePowerShell(exe);
        if (parts.Length == 1) return $"& {quoted}";
        return $"& {quoted} {parts[1]}";
    }

    /// <summary>展开别名（首词匹配）。</summary>
    public static string ExpandAlias(string command)
    {
        var parts = command.Split(' ', 2, StringSplitOptions.None);
        if (parts.Length == 0) return command;
        var name = parts[0];
        if (string.IsNullOrEmpty(name)) return command;

        var expansion = AliasStore.Shared.Expansion(name);
        if (expansion == null) return command;

        return parts.Length == 1 ? expansion : expansion + " " + parts[1];
    }

    /// <summary>交互式命令检测。</summary>
    public static bool IsInteractive(string command)
    {
        var unquoted = StripQuotes(command);
        var hasPipe = unquoted.Contains('|');
        var hasRedirect = unquoted.Contains('>') || unquoted.Contains('<');

        var firstPart = command.Split('|')[0].Trim();

        var interactive = new[]
        {
            // Shells
            "cmd", "powershell", "pwsh", "wsl", "bash", "sh", "zsh", "fish",
            // Editors
            "vim", "vi", "nano", "emacs", "notepad", "code",
            // Pagers
            "more", "less",
            // System
            "top", "htop", "taskmgr", "regedit", "services.msc", "mmc",
            // Remote
            "ssh", "telnet", "ftp", "sftp", "mstsc",
            // REPLs
            "python", "python3", "ipython", "node", "irb",
            // DBs
            "mysql", "psql", "sqlite3", "sqlcmd",
            // Debug
            "gdb", "lldb", "windbg", "cdb",
            // Others
            "bc", "dc",
        };

        foreach (var cmd in interactive)
        {
            if (firstPart.Equals(cmd, StringComparison.OrdinalIgnoreCase) ||
                firstPart.StartsWith(cmd + " ", StringComparison.OrdinalIgnoreCase))
            {
                if (hasPipe || hasRedirect) return false;
                return true;
            }
        }

        if (unquoted.Contains(" -i ") || unquoted.Contains(" --interactive "))
            return true;

        return false;
    }

    private static string UnwrapQuotes(string s)
    {
        if (s.Length < 2) return s;
        var first = s[0];
        var last = s[^1];
        if ((first == '"' && last == '"') || (first == '\'' && last == '\''))
        {
            var inner = s.Substring(1, s.Length - 2);
            if (!inner.Contains(first)) return inner;
        }
        return s;
    }

    private static string StripQuotes(string s)
    {
        var result = new System.Text.StringBuilder();
        bool inSingle = false, inDouble = false, escaped = false;

        foreach (var ch in s)
        {
            if (escaped) { escaped = false; continue; }
            if (ch == '\\') { escaped = true; continue; }
            if (ch == '\'' && !inDouble) { inSingle = !inSingle; continue; }
            if (ch == '"' && !inSingle) { inDouble = !inDouble; continue; }
            if (!inSingle && !inDouble) result.Append(ch);
        }
        return result.ToString();
    }
}
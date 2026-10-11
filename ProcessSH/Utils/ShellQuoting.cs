namespace ProcessSH.Utils;

/// <summary>
/// Windows / PowerShell 路径转义工具。
/// pwsh 单引号内除 '' 外全部字面量，最安全。
/// </summary>
public static class ShellQuoting
{
    /// <summary>PowerShell 单引号包裹，内部 ' 用 '' 转义。</summary>
    public static string QuotePowerShell(string path)
    {
        if (string.IsNullOrEmpty(path)) return "''";
        return "'" + path.Replace("'", "''") + "'";
    }

    /// <summary>CMD 双引号包裹。</summary>
    public static string QuoteCmd(string path)
    {
        if (string.IsNullOrEmpty(path)) return "\"\"";
        return "\"" + path.Replace("\"", "\\\"") + "\"";
    }

    /// <summary>
    /// 通用引用：只在含特殊字符时加单引号。
    /// 用于补全弹出时插入路径。
    /// </summary>
    public static string Quote(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;

        // 只有字母数字和 _-./\~@+ 时不需要引号
        bool needsQuote = false;
        foreach (var ch in path)
        {
            if (!char.IsLetterOrDigit(ch) && !"_-./\\~@+".Contains(ch))
            {
                needsQuote = true;
                break;
            }
        }

        return needsQuote ? QuotePowerShell(path) : path;
    }

    /// <summary>批量引用，空格分隔。</summary>
    public static string QuoteAll(IEnumerable<string> paths)
        => string.Join(" ", paths.Select(Quote));

    /// <summary>展开 Windows 环境变量和 %USERPROFILE%。</summary>
    public static string ExpandEnv(string path)
    {
        if (string.IsNullOrEmpty(path)) return string.Empty;
        return Environment.ExpandEnvironmentVariables(path);
    }

    /// <summary>把绝对路径缩写为 %USERPROFILE% 形式（用于显示）。</summary>
    public static string AbbreviateHome(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrEmpty(home)) return path;

        if (path.Equals(home, StringComparison.OrdinalIgnoreCase))
            return "%USERPROFILE%";

        if (path.StartsWith(home + "\\", StringComparison.OrdinalIgnoreCase))
            return "%USERPROFILE%" + path.Substring(home.Length);

        return path;
    }
}
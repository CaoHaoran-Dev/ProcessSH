using Microsoft.Win32;

namespace ProcessSH.Services;

/// <summary>
/// 开机自启动。Windows 用 HKCU\...\Run 注册表项。
/// </summary>
public static class LaunchAtLogin
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "ProcessSH";

    public static bool IsSupported => true; // Windows 全支持

    public static bool IsEnabled
    {
        get
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: false);
                var value = key?.GetValue(AppName) as string;
                return !string.IsNullOrEmpty(value);
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>设置开机自启动。返回是否成功。</summary>
    public static bool SetEnabled(bool enabled)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true)
                            ?? Registry.CurrentUser.CreateSubKey(RunKey);
            if (key == null) return false;

            if (enabled)
            {
                var exePath = GetExecutablePath();
                if (string.IsNullOrEmpty(exePath)) return false;
                key.SetValue(AppName, $"\"{exePath}\"");
            }
            else
            {
                key.DeleteValue(AppName, throwOnMissingValue: false);
            }
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LaunchAtLogin] 失败: {ex.Message}");
            return false;
        }
    }

    private static string GetExecutablePath()
    {
        // 单文件发布用 ProcessPath；否则用 Assembly location
        var path = Environment.ProcessPath;
        if (!string.IsNullOrEmpty(path) &&
            path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            return path;

        return System.Reflection.Assembly.GetExecutingAssembly().Location;
    }
}
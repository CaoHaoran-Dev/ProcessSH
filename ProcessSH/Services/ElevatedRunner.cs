using System.Diagnostics;

namespace ProcessSH.Services;

/// <summary>
/// 按需提权执行器。通过 runas 动词启动独立的提权进程执行单条命令。
/// </summary>
public static class ElevatedRunner
{
    public static async Task<CommandResult> ExecuteAsync(
        string command,
        string workingDirectory,
        double timeoutSeconds = 10.0)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "pwsh.exe",
            Arguments = $"-NoProfile -NonInteractive -Command \"{command.Replace("\"", "\\\"")}\"",
            WorkingDirectory = workingDirectory,
            Verb = "runas",
            UseShellExecute = true,
            CreateNoWindow = true,
        };

        try
        {
            using var process = Process.Start(psi);
            if (process == null)
                return CommandResult.Failure("提权进程启动失败");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            try
            {
                await process.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                try { process.Kill(entireProcessTree: true); } catch { }
                return CommandResult.Failure(
                    $"命令执行超时（超过 {timeoutSeconds:F0} 秒）",
                    isTimeout: true);
            }

            return process.ExitCode == 0
                ? CommandResult.Success("命令执行成功（提权模式无输出回传）")
                : CommandResult.Failure($"退出码: {process.ExitCode}", process.ExitCode);
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return CommandResult.Failure("用户取消了提权请求");
        }
        catch (Exception ex)
        {
            return CommandResult.Failure(ex.Message);
        }
    }
}
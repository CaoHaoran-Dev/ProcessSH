using System.Diagnostics;
using System.Text;
using ProcessSH.Models;

namespace ProcessSH.Services;

public static class ElevatedRunner
{
    public static async Task<CommandResult> ExecuteAsync(
        string command,
        string workingDirectory,
        double timeoutSeconds = 10.0)
    {
        if (string.IsNullOrWhiteSpace(workingDirectory) || !Directory.Exists(workingDirectory))
            workingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        var psi = new ProcessStartInfo
        {
            FileName = "pwsh.exe",
            Arguments = BuildArguments(command),
            WorkingDirectory = workingDirectory,
            Verb = "runas",
            UseShellExecute = true,
        };

        try
        {
            using var process = Process.Start(psi);
            if (process == null)
                return CommandResult.Failure("提权进程启动失败");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            try
            {
                await process.WaitForExitAsync(cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                try { process.Kill(entireProcessTree: true); } catch { }
                return CommandResult.Failure(
                    Localization.Get("main.timeout", timeoutSeconds), isTimeout: true);
            }

            return process.ExitCode == 0
                ? CommandResult.Success(Localization.Get("output.success.sudo"))
                : CommandResult.Failure(
                    Localization.Get("main.exitcode", process.ExitCode), process.ExitCode);
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return CommandResult.Failure(Localization.Get("error.sudo.cancelled"));
        }
        catch (Exception ex)
        {
            return CommandResult.Failure(ex.Message);
        }
    }

    private static string BuildArguments(string command)
    {
        var script = string.Join("; ", new[]
        {
            "[Console]::OutputEncoding = [System.Text.Encoding]::UTF8",
            "$OutputEncoding = [System.Text.Encoding]::UTF8",
            command,
        });
        var bytes = Encoding.Unicode.GetBytes(script);
        var encoded = Convert.ToBase64String(bytes);
        return $"-NoProfile -NonInteractive -EncodedCommand {encoded}";
    }
}
using System.Diagnostics;

namespace LittyLogs.Tool;

/// <summary>
/// spawns a dotnet subprocess and rewrites its output line-by-line in real time.
/// shared between all subcommands — test, build, and whatever we cook up next 🏃
/// </summary>
public static class DotnetProcessRunner
{
    /// <summary>
    /// runs a dotnet command as a subprocess, rewrites stdout line-by-line, passes stderr through.
    /// returns the subprocess exit code so CI/CD still works no cap
    /// </summary>
    public static async Task<int> RunAsync(
        IReadOnlyList<string> arguments,
        Func<string, string?> rewriter)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        foreach (var arg in arguments)
            psi.ArgumentList.Add(arg);

        using var process = Process.Start(psi);
        if (process is null)
        {
            Console.Error.WriteLine("couldnt start dotnet process, thats not bussin 💀");
            return 1;
        }

        // read stdout line-by-line and rewrite in real time
        var stdoutTask = Task.Run(async () =>
        {
            while (await process.StandardOutput.ReadLineAsync() is { } line)
            {
                var rewritten = rewriter(line);
                Console.WriteLine(rewritten ?? line);
            }
        });

        // stderr passthrough — error output stays real, no rewriting
        var stderrTask = Task.Run(async () =>
        {
            while (await process.StandardError.ReadLineAsync() is { } line)
            {
                Console.Error.WriteLine(line);
            }
        });

        await Task.WhenAll(stdoutTask, stderrTask);
        await process.WaitForExitAsync();

        return process.ExitCode;
    }
}

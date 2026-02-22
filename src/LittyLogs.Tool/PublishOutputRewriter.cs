using static LittyLogs.LittyLogsFormatHelper;

namespace LittyLogs.Tool;

/// <summary>
/// rewrites boring dotnet publish output into gen alpha slang.
/// composes all build transforms (since dotnet publish builds first) plus publish-specific ones 📤🔥
/// </summary>
public static class PublishOutputRewriter
{
    private record OutputTransform(
        Func<string, bool> Matches,
        Func<string, string> Rewrite);

    private static readonly OutputTransform[] PublishTransforms =
    [
        // "Publish succeeded." or "Publish succeeded in 2.1s"
        new(line => line.TrimStart().StartsWith("Publish succeeded", StringComparison.Ordinal),
            line =>
            {
                var timing = line.Contains("in ") ? line[line.IndexOf("in ")..].TrimEnd('.') : "";
                return $"\n{Green}published and ready to deploy bestie, absolute W{Reset}" +
                       (timing.Length > 0 ? $" {timing}" : "") + " 📤🏆";
            }),

        // "Publish FAILED."
        new(line => line.TrimStart().StartsWith("Publish FAILED", StringComparison.Ordinal),
            _ => $"\n{BrightRed}publish took a massive L, aint shipping nothing today{Reset} 💀📤"),

        // publish artifact path — "ProjectName -> /path/to/publish/"
        // different from build output because publish paths often end with a directory
        new(line => line.Contains(" -> ") && !line.TrimEnd().EndsWith(".dll") && !line.Contains(" succeeded "),
            line =>
            {
                var trimmed = line.TrimStart();
                var arrowIdx = trimmed.IndexOf(" -> ");
                if (arrowIdx < 0) return line;
                var projectName = trimmed[..arrowIdx];
                var publishPath = trimmed[(arrowIdx + " -> ".Length)..];
                return $"  {Green}{projectName} packed and ready to ship{Reset} → {Dim}{publishPath}{Reset} 📤";
            }),

        // self-contained related messages
        new(line => line.TrimStart().Contains("self-contained", StringComparison.OrdinalIgnoreCase),
            _ => $"  {Cyan}going self-contained mode — no runtime needed on the server bestie{Reset} 🏠"),

        // trimming messages — "Optimizing assemblies for size" etc
        new(line => line.TrimStart().Contains("Trimming", StringComparison.OrdinalIgnoreCase) ||
                    line.TrimStart().Contains("Optimizing assemblies", StringComparison.OrdinalIgnoreCase),
            _ => $"  {Cyan}trimming the fat — making the output skinny legend{Reset} ✂️"),

        // compressing publish artifacts
        new(line => line.TrimStart().Contains("Compressing", StringComparison.OrdinalIgnoreCase),
            _ => $"  {Cyan}squishing everything down real compact{Reset} 🗜️"),

        // generating native code / ReadyToRun
        new(line => line.TrimStart().Contains("Generating native code", StringComparison.OrdinalIgnoreCase) ||
                    line.TrimStart().Contains("ReadyToRun", StringComparison.OrdinalIgnoreCase),
            _ => $"  {Cyan}pre-compiling native code for maximum speed bestie{Reset} ⚡"),
    ];

    /// <summary>
    /// attempts to rewrite a dotnet publish output line into gen alpha slang.
    /// checks publish-specific transforms first, then falls back to build transforms.
    /// returns null if the line aint recognized (passthrough)
    /// </summary>
    public static string? TryRewrite(string line)
    {
        if (string.IsNullOrEmpty(line))
            return null;

        // publish-specific transforms first (more specific matches win)
        foreach (var transform in PublishTransforms)
        {
            if (transform.Matches(line))
                return transform.Rewrite(line);
        }

        // fall back to build transforms — restore, compile, warnings, errors, timing etc
        return BuildOutputRewriter.TryRewrite(line);
    }
}

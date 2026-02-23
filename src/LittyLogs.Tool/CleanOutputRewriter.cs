using static LittyLogs.LittyLogsFormatHelper;

namespace LittyLogs.Tool;

/// <summary>
/// rewrites boring dotnet clean output into gen alpha slang.
/// composes all build transforms plus clean-specific yeet energy 🗑️🔥
/// </summary>
public static class CleanOutputRewriter
{
    private record OutputTransform(
        Func<string, bool> Matches,
        Func<string, string> Rewrite);

    private static readonly OutputTransform[] CleanTransforms =
    [
        // "Build succeeded." — override BuildOutputRewriter with clean-flavored message
        new(line => line.TrimStart().StartsWith("Build succeeded", StringComparison.Ordinal),
            line =>
            {
                var timing = line.Contains("in ") ? line[line.IndexOf("in ")..].TrimEnd('.') : "";
                return $"\n{Green}all artifacts yeeted into the void bestie{Reset}" +
                       (timing.Length > 0 ? $" {timing}" : "") + " 🗑️🔥";
            }),

        // "         Deleting file "/path/to/file.dll"."
        new(line => line.TrimStart().StartsWith("Deleting file", StringComparison.Ordinal),
            line =>
            {
                var trimmed = line.TrimStart();
                var start = trimmed.IndexOf('"') + 1;
                var end = trimmed.LastIndexOf('"');
                var fileName = (start > 0 && end > start)
                    ? Path.GetFileName(trimmed[start..end])
                    : "unknown artifact";
                return $"  {Dim}yeeted: {fileName}{Reset} 🗑️";
            }),

        // "     2>CoreClean:" — MSBuild noise, suppress it
        new(line => line.TrimStart().StartsWith("CoreClean:", StringComparison.Ordinal)
                    || (line.Contains('>') && line.Contains("CoreClean:")),
            _ => ""),
    ];

    /// <summary>
    /// attempts to rewrite a dotnet clean output line into gen alpha slang.
    /// checks clean-specific transforms first, then falls back to build transforms.
    /// returns null if the line aint recognized (passthrough)
    /// </summary>
    public static string? TryRewrite(string line)
    {
        if (string.IsNullOrEmpty(line))
            return null;

        // clean-specific transforms first (more specific matches win)
        foreach (var transform in CleanTransforms)
        {
            if (transform.Matches(line))
                return transform.Rewrite(line);
        }

        // fall back to build transforms — warnings, errors, timing etc
        return BuildOutputRewriter.TryRewrite(line);
    }
}

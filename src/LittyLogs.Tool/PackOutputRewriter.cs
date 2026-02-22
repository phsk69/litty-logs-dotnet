using static LittyLogs.LittyLogsFormatHelper;

namespace LittyLogs.Tool;

/// <summary>
/// rewrites boring dotnet pack output into gen alpha slang.
/// composes all build transforms (since dotnet pack builds first) plus pack-specific nupkg energy 📦🔥
/// </summary>
public static class PackOutputRewriter
{
    private record OutputTransform(
        Func<string, bool> Matches,
        Func<string, string> Rewrite);

    private static readonly OutputTransform[] PackTransforms =
    [
        // "Successfully created package '/path/to/Foo.1.0.0.nupkg'."
        // also handles .snupkg (symbols packages) because Path.GetFileName dont discriminate 💅
        new(line => line.TrimStart().StartsWith("Successfully created package", StringComparison.Ordinal),
            line =>
            {
                // extract the path between single quotes
                var start = line.IndexOf('\'') + 1;
                var end = line.LastIndexOf('\'');
                var fileName = (start > 0 && end > start)
                    ? Path.GetFileName(line[start..end])
                    : "package";
                return $"  {Green}{fileName} cooked and ready to yeet to NuGet bestie{Reset} 📦🔥";
            }),

        // "Pack succeeded." or "Pack succeeded in 1.2s"
        new(line => line.TrimStart().StartsWith("Pack succeeded", StringComparison.Ordinal),
            line =>
            {
                var timing = line.Contains("in ") ? line[line.IndexOf("in ")..].TrimEnd('.') : "";
                return $"\n{Green}nupkgs absolutely bussin, packed and ready to ship bestie{Reset}" +
                       (timing.Length > 0 ? $" {timing}" : "") + " 📦🏆";
            }),

        // "Pack FAILED."
        new(line => line.TrimStart().StartsWith("Pack FAILED", StringComparison.Ordinal),
            _ => $"\n{BrightRed}pack took a massive L, no nupkgs today bestie{Reset} 💀📦"),
    ];

    /// <summary>
    /// attempts to rewrite a dotnet pack output line into gen alpha slang.
    /// checks pack-specific transforms first, then falls back to build transforms.
    /// returns null if the line aint recognized (passthrough)
    /// </summary>
    public static string? TryRewrite(string line)
    {
        if (string.IsNullOrEmpty(line))
            return null;

        // pack-specific transforms first (more specific matches win)
        foreach (var transform in PackTransforms)
        {
            if (transform.Matches(line))
                return transform.Rewrite(line);
        }

        // fall back to build transforms — restore, compile, warnings, errors, timing etc
        return BuildOutputRewriter.TryRewrite(line);
    }
}

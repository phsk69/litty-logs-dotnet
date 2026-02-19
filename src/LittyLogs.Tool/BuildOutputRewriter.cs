using static LittyLogs.LittyLogsFormatHelper;

namespace LittyLogs.Tool;

/// <summary>
/// rewrites boring dotnet build console output into gen alpha slang.
/// same vibes as LittyMessageRewriter but for MSBuild output instead of ILogger messages 🏗️
/// </summary>
public static class BuildOutputRewriter
{
    private record OutputTransform(
        Func<string, bool> Matches,
        Func<string, string> Rewrite);

    private static readonly OutputTransform[] Transforms =
    [
        // "Determining projects to restore..."
        new(line => line.TrimStart().StartsWith("Determining projects to restore", StringComparison.Ordinal),
            _ => $"  {Cyan}checking what deps we need bestie...{Reset} 📦"),

        // "All projects are up-to-date for restore."
        new(line => line.TrimStart().StartsWith("All projects are up-to-date for restore", StringComparison.Ordinal),
            _ => $"  {Green}deps already locked in, we good{Reset} ✅"),

        // "Restore complete (0.8s)" (shows in terminal UI mode)
        new(line => line.TrimStart().StartsWith("Restore complete", StringComparison.Ordinal),
            line => $"  {Cyan}dependencies locked in{Reset} {ExtractParenthetical(line)}📦"),

        // "  Restored /path/to/project.csproj (in 200 ms)."
        new(line => line.TrimStart().StartsWith("Restored ", StringComparison.Ordinal),
            line =>
            {
                var trimmed = line.TrimStart();
                var path = trimmed["Restored ".Length..];
                var timing = ExtractParenthetical(path);
                var projectPath = path;
                var parenIdx = projectPath.LastIndexOf('(');
                if (parenIdx > 0) projectPath = projectPath[..parenIdx].TrimEnd();
                var projectName = Path.GetFileNameWithoutExtension(projectPath);
                return $"  {Green}{projectName} deps secured{Reset} {timing}📦";
            }),

        // "  ProjectName -> /path/to/dll" (actual build output uses ASCII ->)
        new(line => line.Contains(" -> ") && line.TrimEnd().EndsWith(".dll"),
            line =>
            {
                var trimmed = line.TrimStart();
                var arrowIdx = trimmed.IndexOf(" -> ");
                var projectName = trimmed[..arrowIdx];
                var dllPath = trimmed[(arrowIdx + " -> ".Length)..];
                return $"  {Green}{projectName} built different{Reset} → {Dim}{dllPath}{Reset} ✅";
            }),

        // "  ProjectName net10.0 succeeded (0.1s) → path.dll" (terminal UI mode with Unicode →)
        new(line => line.Contains(" succeeded ") && line.Contains('→'),
            line =>
            {
                var trimmed = line.TrimStart();
                var projectName = trimmed.Split(' ')[0];
                var timing = ExtractParenthetical(line);
                var arrowIdx = line.IndexOf('→');
                var path = arrowIdx >= 0 ? line[(arrowIdx + 1)..].Trim() : "";
                return $"  {Green}{projectName} built different{Reset} {timing}→ {Dim}{path}{Reset} ✅";
            }),

        // "Build succeeded." or "Build succeeded in 2.1s"
        new(line => line.TrimStart().StartsWith("Build succeeded", StringComparison.Ordinal),
            line =>
            {
                var timing = line.Contains("in ") ? line[line.IndexOf("in ")..].TrimEnd('.') : "";
                return $"\n{Green}W in the chat bestie, build absolutely slayed{Reset}" +
                       (timing.Length > 0 ? $" {timing}" : "") + " 🏆";
            }),

        // "Build FAILED."
        new(line => line.TrimStart().StartsWith("Build FAILED", StringComparison.Ordinal),
            _ => $"\n{BrightRed}build took a massive L, not bussin at all{Reset} 💀"),

        // "Time Elapsed 00:00:01.30"
        new(line => line.TrimStart().StartsWith("Time Elapsed ", StringComparison.Ordinal),
            line =>
            {
                var time = line.TrimStart()["Time Elapsed ".Length..];
                return $"  {Dim}cooked in {time}{Reset} ⏱️";
            }),

        // "    0 Warning(s)" or "    3 Warning(s)"
        new(line => line.TrimEnd().EndsWith("Warning(s)"),
            line =>
            {
                var trimmed = line.Trim();
                var count = trimmed.Split(' ')[0];
                return count == "0"
                    ? $"  {Green}zero warnings, clean af{Reset} 💅"
                    : $"  {Yellow}{count} warnings, kinda sus{Reset} 😤";
            }),

        // "    0 Error(s)" or "    1 Error(s)"
        new(line => line.TrimEnd().EndsWith("Error(s)"),
            line =>
            {
                var trimmed = line.Trim();
                var count = trimmed.Split(' ')[0];
                return count == "0"
                    ? $"  {Green}zero errors, absolutely ate{Reset} 🔥"
                    : $"  {Red}{count} errors, thats not it{Reset} 💀";
            }),

        // warning lines: "  /path/File.cs(10,5): warning CS0168: ..."
        new(line => line.Contains(": warning "),
            line =>
            {
                var warnIdx = line.IndexOf(": warning ");
                var location = line[..warnIdx].TrimStart();
                var warnMsg = line[(warnIdx + ": warning ".Length)..];
                return $"  {Yellow}😤 heads up bestie:{Reset} {Dim}{location}{Reset} — {Yellow}{warnMsg}{Reset}";
            }),

        // error lines: "  /path/File.cs(10,5): error CS1234: ..."
        new(line => line.Contains(": error ") && !line.TrimStart().StartsWith("Build", StringComparison.Ordinal),
            line =>
            {
                var errIdx = line.IndexOf(": error ");
                var location = line[..errIdx].TrimStart();
                var errMsg = line[(errIdx + ": error ".Length)..];
                return $"  {Red}💀 big L:{Reset} {Dim}{location}{Reset} — {Red}{errMsg}{Reset}";
            }),
    ];

    /// <summary>
    /// attempts to rewrite a dotnet build output line into gen alpha slang.
    /// returns null if the line aint recognized (passthrough)
    /// </summary>
    public static string? TryRewrite(string line)
    {
        if (string.IsNullOrEmpty(line))
            return null;

        foreach (var transform in Transforms)
        {
            if (transform.Matches(line))
                return transform.Rewrite(line);
        }

        return null;
    }

    /// <summary>
    /// extracts the (timing) portion from a line like "Restore complete (0.8s)"
    /// </summary>
    internal static string ExtractParenthetical(string line)
    {
        var openParen = line.LastIndexOf('(');
        var closeParen = line.LastIndexOf(')');
        if (openParen >= 0 && closeParen > openParen)
            return line[openParen..(closeParen + 1)] + " ";
        return "";
    }
}

using static LittyLogs.LittyLogsFormatHelper;

namespace LittyLogs.Tool;

/// <summary>
/// rewrites boring dotnet test runner console output into gen alpha slang.
/// composes all build transforms (since dotnet test builds first) plus test-specific ones 🧪
/// </summary>
public static class TestOutputRewriter
{
    private record OutputTransform(
        Func<string, bool> Matches,
        Func<string, string> Rewrite);

    private static readonly OutputTransform[] TestTransforms =
    [
        // "[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v3.1.5+..."
        new(line => line.StartsWith("[xUnit.net ", StringComparison.Ordinal) && line.Contains("VSTest Adapter"),
            line =>
            {
                var adapterInfo = ExtractAfterKeyword(line, "Adapter ");
                return $"  {Green}[xUnit.net]{Reset} adapter locked in and ready to slay {Dim}{adapterInfo}{Reset}🔌";
            }),

        // "[xUnit.net 00:00:00.04]   Discovering: LittyLogs.Tests"
        new(line => line.StartsWith("[xUnit.net ", StringComparison.Ordinal) && line.Contains("Discovering:"),
            line =>
            {
                var project = ExtractAfterKeyword(line, "Discovering:").Trim();
                return $"  {Green}[xUnit.net]{Reset} {Yellow}scouting for tests in {project}{Reset} 🔍";
            }),

        // "[xUnit.net 00:00:00.09]   Discovered:  LittyLogs.Tests"
        new(line => line.StartsWith("[xUnit.net ", StringComparison.Ordinal) && line.Contains("Discovered:"),
            line =>
            {
                var project = ExtractAfterKeyword(line, "Discovered:").Trim();
                return $"  {Green}[xUnit.net]{Reset} {Green}found the squad in {project}{Reset} 📋";
            }),

        // "[xUnit.net 00:00:00.11]   Starting:    LittyLogs.Tests"
        new(line => line.StartsWith("[xUnit.net ", StringComparison.Ordinal) && line.Contains("Starting:"),
            line =>
            {
                var project = ExtractAfterKeyword(line, "Starting:").Trim();
                return $"  {Green}[xUnit.net]{Reset} {Cyan}lets gooo {project} is cooking{Reset} 🔥";
            }),

        // "[xUnit.net 00:00:00.22]   Finished:    LittyLogs.Tests"
        new(line => line.StartsWith("[xUnit.net ", StringComparison.Ordinal) && line.Contains("Finished:"),
            line =>
            {
                var project = ExtractAfterKeyword(line, "Finished:").Trim();
                return $"  {Green}[xUnit.net]{Reset} {Green}{project} absolutely ate no crumbs{Reset} 💅";
            }),

        // "Test run for /path/to.dll (.NETCoreApp,Version=v10.0)"
        new(line => line.StartsWith("Test run for ", StringComparison.Ordinal),
            line =>
            {
                var path = line["Test run for ".Length..];
                var dllName = Path.GetFileNameWithoutExtension(path.Split(' ')[0]);
                return $"  {Cyan}loading up {dllName} for the vibes check{Reset} 🧪";
            }),

        // "VSTest version 18.0.1 (x64)"
        new(line => line.StartsWith("VSTest version ", StringComparison.Ordinal),
            line =>
            {
                var version = line["VSTest version ".Length..];
                return $"  {Dim}test engine {version} locked in{Reset} 🔧";
            }),

        // "Starting test execution, please wait..."
        new(line => line.StartsWith("Starting test execution", StringComparison.Ordinal),
            _ => $"  {Yellow}tests entering their cooking era...{Reset} 🍳"),

        // "A total of 1 test files matched the specified pattern."
        new(line => line.StartsWith("A total of ", StringComparison.Ordinal) && line.Contains("test files"),
            line =>
            {
                var count = line.Split(' ')[3]; // "A total of N"
                return $"  {Green}found {count} test assembly to run{Reset} 📂";
            }),

        // "  Passed TestName [26 ms]" (detailed logger format)
        new(line => line.TrimStart().StartsWith("Passed ", StringComparison.Ordinal) &&
                    !line.Contains("Passed:") && !line.Contains("Passed!"),
            line =>
            {
                var trimmed = line.TrimStart();
                var testName = trimmed["Passed ".Length..];
                return $"  {Green}✅ {testName}{Reset}";
            }),

        // "  Failed TestName [26 ms]" (detailed logger format)
        new(line => line.TrimStart().StartsWith("Failed ", StringComparison.Ordinal) &&
                    !line.Contains("Failed:") && !line.Contains("Failed!"),
            line =>
            {
                var trimmed = line.TrimStart();
                var testName = trimmed["Failed ".Length..];
                return $"  {Red}💀 {testName}{Reset}";
            }),

        // "  Skipped TestName" (detailed logger format)
        new(line => line.TrimStart().StartsWith("Skipped ", StringComparison.Ordinal) &&
                    !line.Contains("Skipped:"),
            line =>
            {
                var trimmed = line.TrimStart();
                var testName = trimmed["Skipped ".Length..];
                return $"  {Yellow}⏭️ {testName}{Reset}";
            }),

        // "Passed!  - Failed:     0, Passed:     4, Skipped:     0, Total:     4, Duration: 36 ms - ProjectName.dll (net10.0)"
        new(line => line.TrimStart().StartsWith("Passed!", StringComparison.Ordinal),
            line => RewriteCompactSummary(line, passed: true)),

        // "Failed!  - Failed:     1, Passed:     3, ..."
        new(line => line.TrimStart().StartsWith("Failed!", StringComparison.Ordinal),
            line => RewriteCompactSummary(line, passed: false)),

        // "Test Run Successful." (detailed logger format)
        new(line => line.TrimStart().StartsWith("Test Run Successful", StringComparison.Ordinal),
            _ => $"\n  {Green}all tests ate and left no crumbs{Reset} 🏆"),

        // "Test Run Failed." (detailed logger format)
        new(line => line.TrimStart().StartsWith("Test Run Failed", StringComparison.Ordinal),
            _ => $"\n  {BrightRed}some tests flopped, thats not it bestie{Reset} 💀"),

        // "Total tests: 62" (detailed logger format)
        new(line => line.StartsWith("Total tests:", StringComparison.Ordinal),
            line =>
            {
                var count = line["Total tests:".Length..].Trim();
                return $"  {Cyan}total vibes checked: {count}{Reset}";
            }),

        // "     Passed: 62" (detailed logger format summary)
        new(line => line.TrimStart().StartsWith("Passed:", StringComparison.Ordinal),
            line =>
            {
                var count = line.TrimStart()["Passed:".Length..].Trim();
                return $"  {Green}ate: {count}{Reset} ✅";
            }),

        // " Total time: 0.5130 Seconds" (detailed logger format)
        new(line => line.TrimStart().StartsWith("Total time:", StringComparison.Ordinal),
            line =>
            {
                var time = line.TrimStart()["Total time:".Length..].Trim();
                return $"  {Dim}cooked in {time}{Reset} ⏱️";
            }),

        // "  Standard Output Messages:" header
        new(line => line.TrimStart().StartsWith("Standard Output Messages:", StringComparison.Ordinal),
            _ => $"  {Dim}--- test output ---{Reset}"),

        // test project succeeded in compact mode: "  LittyLogs.Tests test net10.0 succeeded (0.7s)"
        new(line => line.Contains(" test ") && line.Contains("succeeded") && !line.Contains(" -> "),
            line =>
            {
                var trimmed = line.TrimStart();
                var project = trimmed.Split(' ')[0];
                var timing = BuildOutputRewriter.ExtractParenthetical(line);
                return $"  {Green}{project} tests all slapped{Reset} {timing}🧪";
            }),

        // test project failed in compact mode
        new(line => line.Contains(" test ") && line.Contains("failed") && !line.Contains(" -> "),
            line =>
            {
                var trimmed = line.TrimStart();
                var project = trimmed.Split(' ')[0];
                var timing = BuildOutputRewriter.ExtractParenthetical(line);
                return $"  {Red}{project} tests flopped hard{Reset} {timing}💀";
            }),

        // "Test summary: total: 66, failed: 0, succeeded: 66, skipped: 0, duration: 0.6s"
        // (terminal UI compact format)
        new(line => line.StartsWith("Test summary:", StringComparison.Ordinal),
            RewriteTestSummary),
    ];

    /// <summary>
    /// attempts to rewrite a dotnet test output line into gen alpha slang.
    /// checks test-specific transforms first, then falls back to build transforms.
    /// returns null if the line aint recognized (passthrough)
    /// </summary>
    public static string? TryRewrite(string line)
    {
        if (string.IsNullOrEmpty(line))
            return null;

        // test-specific transforms first (more specific matches)
        foreach (var transform in TestTransforms)
        {
            if (transform.Matches(line))
                return transform.Rewrite(line);
        }

        // fall back to build transforms (restore, compile, build succeeded, etc)
        return BuildOutputRewriter.TryRewrite(line);
    }

    /// <summary>
    /// rewrites "Passed!  - Failed: 0, Passed: 4, Skipped: 0, Total: 4, Duration: 36 ms - Project.dll (net10.0)"
    /// </summary>
    private static string RewriteCompactSummary(string line, bool passed)
    {
        var failedCount = ExtractCompactStat(line, "Failed:");
        var passedCount = ExtractCompactStat(line, "Passed:");
        var skippedCount = ExtractCompactStat(line, "Skipped:");
        var totalCount = ExtractCompactStat(line, "Total:");
        var duration = ExtractCompactStat(line, "Duration:");

        // extract project name from end: "- ProjectName.dll (framework)"
        var lastDash = line.LastIndexOf(" - ");
        var projectName = lastDash >= 0 ? line[(lastDash + 3)..].Trim() : "";

        var color = passed ? Green : BrightRed;
        var emoji = passed ? "✅" : "💀";
        var vibe = passed ? "all ate" : "some flopped";

        return $"  {color}{emoji} {projectName}:{Reset} " +
               $"{Green}{passedCount} ate{Reset}, " +
               $"{(failedCount != "0" ? Red : Dim)}{failedCount} flopped{Reset}, " +
               $"{Dim}{skippedCount} ghosted{Reset} " +
               $"({totalCount} total, {duration}) — {color}{vibe}{Reset}";
    }

    /// <summary>
    /// litty-fies the test summary line with all the stats (terminal UI format)
    /// </summary>
    private static string RewriteTestSummary(string line)
    {
        var total = ExtractStat(line, "total:");
        var failed = ExtractStat(line, "failed:");
        var succeeded = ExtractStat(line, "succeeded:");
        var skipped = ExtractStat(line, "skipped:");
        var duration = ExtractStat(line, "duration:");

        var hasFails = failed != "0";
        var color = hasFails ? BrightRed : Green;
        var emoji = hasFails ? "💀" : "🏆";
        var vibe = hasFails ? "some tests took an L" : "all tests ate no cap";

        return $"\n{color}vibes check:{Reset} {total} ran, " +
               $"{Green}{succeeded} ate{Reset}, " +
               $"{(hasFails ? Red : Dim)}{failed} flopped{Reset}, " +
               $"{Dim}{skipped} ghosted{Reset} " +
               $"in {duration} — {color}{vibe}{Reset} {emoji}";
    }

    private static string ExtractStat(string line, string key)
    {
        var idx = line.IndexOf(key, StringComparison.Ordinal);
        if (idx < 0) return "?";
        var start = idx + key.Length;
        var rest = line[start..].TrimStart();
        var end = rest.IndexOfAny([',', '\n', '\r']);
        return end >= 0 ? rest[..end].Trim() : rest.Trim();
    }

    private static string ExtractCompactStat(string line, string key)
    {
        var idx = line.IndexOf(key, StringComparison.Ordinal);
        if (idx < 0) return "?";
        var start = idx + key.Length;
        var rest = line[start..].TrimStart();
        var end = rest.IndexOfAny([',', '-']);
        return end >= 0 ? rest[..end].Trim() : rest.Trim();
    }

    private static string ExtractAfterKeyword(string line, string keyword)
    {
        var idx = line.IndexOf(keyword, StringComparison.Ordinal);
        if (idx < 0) return "";
        return line[(idx + keyword.Length)..];
    }
}

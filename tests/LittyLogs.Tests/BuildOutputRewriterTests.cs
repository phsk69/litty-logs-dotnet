using LittyLogs.Tool;
using LittyLogs.Xunit;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace LittyLogs.Tests;

/// <summary>
/// making sure the build output rewriter is absolutely slaying every MSBuild line fr fr 🏗️
/// </summary>
public class BuildOutputRewriterTests
{
    private readonly ILogger<BuildOutputRewriterTests> _logger;

    public BuildOutputRewriterTests(ITestOutputHelper output)
    {
        _logger = output.CreateLittyLogger<BuildOutputRewriterTests>();
    }

    [Fact]
    public void TryRewrite_DeterminingProjects_GetsLittyfied()
    {
        var result = BuildOutputRewriter.TryRewrite("Determining projects to restore...");

        Assert.NotNull(result);
        Assert.Contains("deps", result);
    }

    [Fact]
    public void TryRewrite_AllProjectsUpToDate_GetsLittyfied()
    {
        var result = BuildOutputRewriter.TryRewrite("  All projects are up-to-date for restore.");

        Assert.NotNull(result);
        Assert.Contains("locked in", result);
    }

    [Fact]
    public void TryRewrite_RestoreComplete_GetsLittyfied()
    {
        // terminal UI format
        var result = BuildOutputRewriter.TryRewrite("Restore complete (0.8s)");

        Assert.NotNull(result);
        Assert.Contains("locked in", result);
        Assert.Contains("(0.8s)", result);
    }

    [Fact]
    public void TryRewrite_ProjectBuildSucceeded_PreservesProjectName()
    {
        var result = BuildOutputRewriter.TryRewrite(
            "  LittyLogs -> /home/psk/git/public/litty-logs/src/LittyLogs/bin/Debug/net10.0/LittyLogs.dll");

        Assert.NotNull(result);
        Assert.Contains("LittyLogs", result);
        Assert.Contains("built different", result);
    }

    [Fact]
    public void TryRewrite_ProjectBuildSucceeded_TerminalUIFormat()
    {
        // terminal UI format uses Unicode → and "succeeded"
        var result = BuildOutputRewriter.TryRewrite(
            "  LittyLogs net10.0 succeeded (0.1s) → src/LittyLogs/bin/Debug/net10.0/LittyLogs.dll");

        Assert.NotNull(result);
        Assert.Contains("LittyLogs", result);
        Assert.Contains("built different", result);
        Assert.Contains("(0.1s)", result);
    }

    [Fact]
    public void TryRewrite_BuildSucceeded_GetsLittyfied()
    {
        var result = BuildOutputRewriter.TryRewrite("Build succeeded.");

        Assert.NotNull(result);
        Assert.Contains("slayed", result);
    }

    [Fact]
    public void TryRewrite_BuildSucceededWithTiming_PreservesTiming()
    {
        // terminal UI format
        var result = BuildOutputRewriter.TryRewrite("Build succeeded in 2.1s");

        Assert.NotNull(result);
        Assert.Contains("slayed", result);
        Assert.Contains("in 2.1s", result);
    }

    [Fact]
    public void TryRewrite_BuildFailed_GetsLittyfied()
    {
        var result = BuildOutputRewriter.TryRewrite("Build FAILED.");

        Assert.NotNull(result);
        Assert.Contains("massive L", result);
    }

    [Fact]
    public void TryRewrite_TimeElapsed_GetsLittyfied()
    {
        var result = BuildOutputRewriter.TryRewrite("Time Elapsed 00:00:01.30");

        Assert.NotNull(result);
        Assert.Contains("cooked in", result);
        Assert.Contains("00:00:01.30", result);
    }

    [Fact]
    public void TryRewrite_ZeroWarnings_GetsLittyfied()
    {
        var result = BuildOutputRewriter.TryRewrite("    0 Warning(s)");

        Assert.NotNull(result);
        Assert.Contains("zero warnings", result);
    }

    [Fact]
    public void TryRewrite_SomeWarnings_GetsLittyfied()
    {
        var result = BuildOutputRewriter.TryRewrite("    3 Warning(s)");

        Assert.NotNull(result);
        Assert.Contains("3", result);
        Assert.Contains("sus", result);
    }

    [Fact]
    public void TryRewrite_ZeroErrors_GetsLittyfied()
    {
        var result = BuildOutputRewriter.TryRewrite("    0 Error(s)");

        Assert.NotNull(result);
        Assert.Contains("zero errors", result);
    }

    [Fact]
    public void TryRewrite_WarningLine_PreservesDetails()
    {
        var result = BuildOutputRewriter.TryRewrite(
            "  /path/File.cs(10,5): warning CS0168: The variable 'x' is declared but never used");

        Assert.NotNull(result);
        Assert.Contains("heads up", result);
        Assert.Contains("CS0168", result);
    }

    [Fact]
    public void TryRewrite_ErrorLine_PreservesDetails()
    {
        var result = BuildOutputRewriter.TryRewrite(
            "  /path/File.cs(10,5): error CS1234: some compiler error");

        Assert.NotNull(result);
        Assert.Contains("big L", result);
        Assert.Contains("CS1234", result);
    }

    [Fact]
    public void TryRewrite_UnknownLine_ReturnsNull()
    {
        var result = BuildOutputRewriter.TryRewrite("some random output nobody expected");

        Assert.Null(result);
    }

    [Fact]
    public void TryRewrite_EmptyString_ReturnsNull()
    {
        var result = BuildOutputRewriter.TryRewrite("");

        Assert.Null(result);
    }
}

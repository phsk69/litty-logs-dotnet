using LittyLogs.Tool;
using LittyLogs.Xunit;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace LittyLogs.Tests;

/// <summary>
/// making sure dotnet litty clean is yeeting artifacts with style fr fr 🗑️
/// </summary>
public class CleanOutputRewriterTests
{
    private readonly ILogger<CleanOutputRewriterTests> _logger;

    public CleanOutputRewriterTests(ITestOutputHelper output)
    {
        _logger = output.CreateLittyLogger<CleanOutputRewriterTests>();
    }

    [Fact]
    public void TryRewrite_DeletingFile_GetsLittyfied()
    {
        var result = CleanOutputRewriter.TryRewrite(
            "         Deleting file \"/home/psk/git/litty-logs-dotnet/tests/LittyLogs.Tests/bin/Debug/net10.0/LittyLogs.Tests.dll\".");

        Assert.NotNull(result);
        Assert.Contains("yeeted", result);
        Assert.Contains("LittyLogs.Tests.dll", result);
        _logger.LogInformation("deleting file line getting the yeet treatment 🗑️🔥");
    }

    [Fact]
    public void TryRewrite_DeletingFile_ExtractsJustFilename()
    {
        // should show just the filename, not the full path — keep it clean bestie
        var result = CleanOutputRewriter.TryRewrite(
            "         Deleting file \"/very/long/path/to/xunit.abstractions.dll\".");

        Assert.NotNull(result);
        Assert.Contains("xunit.abstractions.dll", result);
        Assert.DoesNotContain("/very/long/path", result);
    }

    [Fact]
    public void TryRewrite_CoreClean_GetsSuppressed()
    {
        var result = CleanOutputRewriter.TryRewrite("     2>CoreClean:");

        Assert.NotNull(result);
        Assert.Equal("", result);
        _logger.LogInformation("CoreClean MSBuild noise suppressed 🤫");
    }

    [Fact]
    public void TryRewrite_CoreCleanNoPrefix_GetsSuppressed()
    {
        var result = CleanOutputRewriter.TryRewrite("CoreClean:");

        Assert.NotNull(result);
        Assert.Equal("", result);
    }

    [Fact]
    public void TryRewrite_BuildSucceeded_GetsCleanFlavoredMessage()
    {
        var result = CleanOutputRewriter.TryRewrite("Build succeeded.");

        Assert.NotNull(result);
        Assert.Contains("yeeted", result);
        Assert.Contains("void", result);
        _logger.LogInformation("build succeeded overridden with clean vibes 🗑️🔥");
    }

    [Fact]
    public void TryRewrite_BuildSucceededWithTiming_PreservesTiming()
    {
        var result = CleanOutputRewriter.TryRewrite("Build succeeded in 1.1s");

        Assert.NotNull(result);
        Assert.Contains("yeeted", result);
        Assert.Contains("in 1.1s", result);
    }

    [Fact]
    public void TryRewrite_BuildOutputFallback_StillWorks()
    {
        // clean shares some output with build — fallback should still match
        var result = CleanOutputRewriter.TryRewrite("    0 Warning(s)");

        Assert.NotNull(result);
        Assert.Contains("zero warnings", result);
        _logger.LogInformation("build fallback still works in clean mode bestie 💅");
    }

    [Fact]
    public void TryRewrite_TimeElapsed_FallsBackToBuildRewriter()
    {
        var result = CleanOutputRewriter.TryRewrite("Time Elapsed 00:00:01.06");

        Assert.NotNull(result);
        Assert.Contains("cooked in", result);
    }

    [Fact]
    public void TryRewrite_UnknownLine_ReturnsNull()
    {
        var result = CleanOutputRewriter.TryRewrite("some random output nobody expected");

        Assert.Null(result);
    }

    [Fact]
    public void TryRewrite_EmptyString_ReturnsNull()
    {
        var result = CleanOutputRewriter.TryRewrite("");

        Assert.Null(result);
    }

    [Fact]
    public void TryRewrite_NullInput_ReturnsNull()
    {
        var result = CleanOutputRewriter.TryRewrite(null!);

        Assert.Null(result);
    }
}

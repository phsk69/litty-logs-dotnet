using LittyLogs.Tool;
using LittyLogs.Xunit;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace LittyLogs.Tests;

/// <summary>
/// making sure the test output rewriter slays every xUnit runner line AND inherits build transforms 🧪
/// </summary>
public class TestOutputRewriterTests
{
    private readonly ILogger<TestOutputRewriterTests> _logger;

    public TestOutputRewriterTests(ITestOutputHelper output)
    {
        _logger = output.CreateLittyLogger<TestOutputRewriterTests>();
    }

    [Theory]
    [InlineData("Discovering:", "scouting")]
    [InlineData("Discovered:", "found")]
    [InlineData("Starting:", "cooking")]
    [InlineData("Finished:", "ate")]
    public void TryRewrite_XunitLifecycle_AllGetLittyfied(string keyword, string expectedContains)
    {
        var line = $"[xUnit.net 00:00:00.04]   {keyword}  LittyLogs.Tests";
        var result = TestOutputRewriter.TryRewrite(line);

        Assert.NotNull(result);
        Assert.Contains(expectedContains, result);
        Assert.Contains("LittyLogs.Tests", result);
    }

    [Fact]
    public void TryRewrite_VstestAdapter_GetsLittyfied()
    {
        var result = TestOutputRewriter.TryRewrite(
            "[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v3.1.5+abc (64-bit .NET 10.0.3)");

        Assert.NotNull(result);
        Assert.Contains("adapter", result);
        Assert.Contains("locked in", result);
    }

    [Fact]
    public void TryRewrite_TestRunFor_PreservesProjectName()
    {
        var result = TestOutputRewriter.TryRewrite(
            "Test run for /path/to/LittyLogs.Tests.dll (.NETCoreApp,Version=v10.0)");

        Assert.NotNull(result);
        Assert.Contains("LittyLogs.Tests", result);
        Assert.Contains("vibes check", result);
    }

    [Fact]
    public void TryRewrite_VstestVersion_GetsLittyfied()
    {
        var result = TestOutputRewriter.TryRewrite("VSTest version 18.0.1 (x64)");

        Assert.NotNull(result);
        Assert.Contains("test engine", result);
        Assert.Contains("18.0.1", result);
    }

    [Fact]
    public void TryRewrite_StartingExecution_GetsLittyfied()
    {
        var result = TestOutputRewriter.TryRewrite("Starting test execution, please wait...");

        Assert.NotNull(result);
        Assert.Contains("cooking era", result);
    }

    [Fact]
    public void TryRewrite_TotalTestFiles_PreservesCount()
    {
        var result = TestOutputRewriter.TryRewrite(
            "A total of 1 test files matched the specified pattern.");

        Assert.NotNull(result);
        Assert.Contains("1", result);
    }

    [Fact]
    public void TryRewrite_PassedTest_ShowsCheckmark()
    {
        var result = TestOutputRewriter.TryRewrite(
            "  Passed LittyLogs.Tests.MyTest.SomeTest [26 ms]");

        Assert.NotNull(result);
        Assert.Contains("✅", result);
        Assert.Contains("LittyLogs.Tests.MyTest.SomeTest", result);
    }

    [Fact]
    public void TryRewrite_FailedTest_ShowsSkull()
    {
        var result = TestOutputRewriter.TryRewrite(
            "  Failed LittyLogs.Tests.MyTest.BrokenTest [5 ms]");

        Assert.NotNull(result);
        Assert.Contains("💀", result);
        Assert.Contains("BrokenTest", result);
    }

    [Fact]
    public void TryRewrite_SkippedTest_ShowsSkip()
    {
        var result = TestOutputRewriter.TryRewrite(
            "  Skipped LittyLogs.Tests.MyTest.TodoTest");

        Assert.NotNull(result);
        Assert.Contains("⏭️", result);
    }

    [Fact]
    public void TryRewrite_CompactPassedSummary_GetsLittyfied()
    {
        var result = TestOutputRewriter.TryRewrite(
            "Passed!  - Failed:     0, Passed:     4, Skipped:     0, Total:     4, Duration: 36 ms - LittyLogs.Example.Xunit.dll (net10.0)");

        Assert.NotNull(result);
        Assert.Contains("4", result);
        Assert.Contains("ate", result);
        Assert.Contains("✅", result);
    }

    [Fact]
    public void TryRewrite_CompactFailedSummary_GetsLittyfied()
    {
        var result = TestOutputRewriter.TryRewrite(
            "Failed!  - Failed:     2, Passed:     4, Skipped:     0, Total:     6, Duration: 50 ms - MyTests.dll (net10.0)");

        Assert.NotNull(result);
        Assert.Contains("2", result);
        Assert.Contains("flopped", result);
        Assert.Contains("💀", result);
    }

    [Fact]
    public void TryRewrite_TestRunSuccessful_GetsLittyfied()
    {
        var result = TestOutputRewriter.TryRewrite("Test Run Successful.");

        Assert.NotNull(result);
        Assert.Contains("ate", result);
        Assert.Contains("no crumbs", result);
    }

    [Fact]
    public void TryRewrite_TestRunFailed_GetsLittyfied()
    {
        var result = TestOutputRewriter.TryRewrite("Test Run Failed.");

        Assert.NotNull(result);
        Assert.Contains("flopped", result);
    }

    [Fact]
    public void TryRewrite_TestSummaryTerminalUI_GetsLittyfied()
    {
        var result = TestOutputRewriter.TryRewrite(
            "Test summary: total: 66, failed: 0, succeeded: 66, skipped: 0, duration: 0.6s");

        Assert.NotNull(result);
        Assert.Contains("vibes check", result);
        Assert.Contains("66", result);
    }

    [Fact]
    public void TryRewrite_FallsBackToBuildTransforms()
    {
        // build output lines should still work through TestOutputRewriter
        var result = TestOutputRewriter.TryRewrite(
            "  LittyLogs -> /path/to/LittyLogs.dll");

        Assert.NotNull(result);
        Assert.Contains("built different", result);
    }

    [Fact]
    public void TryRewrite_StandardOutputMessages_GetsRewritten()
    {
        var result = TestOutputRewriter.TryRewrite("  Standard Output Messages:");

        Assert.NotNull(result);
        Assert.Contains("test output", result);
    }

    [Fact]
    public void TryRewrite_UnknownLine_ReturnsNull()
    {
        var result = TestOutputRewriter.TryRewrite("some random output nobody expected");

        Assert.Null(result);
    }

    [Fact]
    public void TryRewrite_EmptyString_ReturnsNull()
    {
        var result = TestOutputRewriter.TryRewrite("");

        Assert.Null(result);
    }

    [Fact]
    public void TryRewrite_TestOutputLines_PassThrough()
    {
        // actual ITestOutputHelper content should pass through untouched
        var littyLog = " [🔥 INFO] [2026-02-18T21:45:00.420Z] [MyTests] my test log message";
        var result = TestOutputRewriter.TryRewrite(littyLog);

        Assert.Null(result);
    }
}

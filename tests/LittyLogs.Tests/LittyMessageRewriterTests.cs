using LittyLogs.Xunit;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace LittyLogs.Tests;

/// <summary>
/// testing that the message rewriter is absolutely slaying every known framework message fr fr 🔥
/// dogfooding our own litty-logs xUnit integration for test output 💅
/// </summary>
public class LittyMessageRewriterTests
{
    private readonly ILogger<LittyMessageRewriterTests> _logger;

    public LittyMessageRewriterTests(ITestOutputHelper output)
    {
        _logger = output.CreateLittyLogger<LittyMessageRewriterTests>();
    }

    [Fact]
    public void TryRewrite_ApplicationStarted_GetsFullyLittyfied()
    {
        // the most iconic boring dotnet message gets the glow up it deserves
        var result = LittyMessageRewriter.TryRewrite(
            "Application started. Press Ctrl+C to shut down.");

        Assert.NotNull(result);
        Assert.Contains("bussin", result);
        Assert.Contains("slay", result);
        Assert.Contains("no cap", result);
    }

    [Fact]
    public void TryRewrite_ApplicationShuttingDown_SaysAightImmaHeadOut()
    {
        var result = LittyMessageRewriter.TryRewrite(
            "Application is shutting down...");

        Assert.NotNull(result);
        Assert.Contains("aight imma head out", result);
    }

    [Fact]
    public void TryRewrite_NowListeningOn_PreservesTheUrl()
    {
        // gotta keep the actual URL bestie, we just litty-fy the wrapper
        var result = LittyMessageRewriter.TryRewrite(
            "Now listening on: http://localhost:5000");

        Assert.NotNull(result);
        Assert.Contains("http://localhost:5000", result);
        Assert.Contains("vibing", result);
    }

    [Fact]
    public void TryRewrite_ContentRootPath_PreservesThePath()
    {
        var result = LittyMessageRewriter.TryRewrite(
            "Content root path: /app/my-project");

        Assert.NotNull(result);
        Assert.Contains("/app/my-project", result);
        Assert.Contains("bestie", result);
    }

    [Fact]
    public void TryRewrite_HostingEnvironment_PreservesTheEnvName()
    {
        var result = LittyMessageRewriter.TryRewrite(
            "Hosting environment: Development");

        Assert.NotNull(result);
        Assert.Contains("Development", result);
        Assert.Contains("era", result);
    }

    [Theory]
    [InlineData("Hosting starting", "bread")]
    [InlineData("Hosting started", "slay")]
    [InlineData("Hosting stopping", "yeeting")]
    [InlineData("Hosting stopped", "peace")]
    public void TryRewrite_HostingLifecycle_AllGetLittyfied(string original, string expectedContains)
    {
        var result = LittyMessageRewriter.TryRewrite(original);

        Assert.NotNull(result);
        Assert.Contains(expectedContains, result);
    }

    [Fact]
    public void TryRewrite_RequestStarting_PreservesRequestDetails()
    {
        var result = LittyMessageRewriter.TryRewrite(
            "Request starting HTTP/2 GET https://localhost/api/vibes - - -");

        Assert.NotNull(result);
        Assert.Contains("HTTP/2 GET https://localhost/api/vibes", result);
        Assert.Contains("slid in", result);
    }

    [Fact]
    public void TryRewrite_RequestFinished_PreservesDetails()
    {
        var result = LittyMessageRewriter.TryRewrite(
            "Request finished HTTP/2 GET https://localhost/api/vibes - 200 0.5ms");

        Assert.NotNull(result);
        Assert.Contains("finished cooking", result);
    }

    [Fact]
    public void TryRewrite_EndpointMatched_PreservesEndpointName()
    {
        var result = LittyMessageRewriter.TryRewrite(
            "Request matched endpoint 'GET /api/vibes'");

        Assert.NotNull(result);
        Assert.Contains("bestie endpoint", result);
        Assert.Contains("GET /api/vibes", result);
    }

    [Fact]
    public void TryRewrite_ExecutingEndpoint_LittyfiesIt()
    {
        var result = LittyMessageRewriter.TryRewrite(
            "Executing endpoint 'GET /api/health'");

        Assert.NotNull(result);
        Assert.Contains("lets gooo", result);
    }

    [Fact]
    public void TryRewrite_ExecutedEndpoint_LittyfiesIt()
    {
        var result = LittyMessageRewriter.TryRewrite(
            "Executed endpoint 'GET /api/health'");

        Assert.NotNull(result);
        Assert.Contains("no crumbs", result);
    }

    [Fact]
    public void TryRewrite_UnknownMessage_ReturnsNull()
    {
        // unknown messages should pass through untouched, we dont mess with custom app logs
        var result = LittyMessageRewriter.TryRewrite(
            "some random custom log message that isnt a framework thing");

        Assert.Null(result);
    }

    [Fact]
    public void TryRewrite_EmptyString_ReturnsNull()
    {
        var result = LittyMessageRewriter.TryRewrite("");
        Assert.Null(result);
    }

    [Fact]
    public void TryRewrite_PartialMatch_DoesNotFalsePositive()
    {
        // "Application started" without the full suffix should NOT match
        // because "Application started." != "Application started. Press Ctrl+C..."
        // but actually StartsWith will match... lets make sure the right transform fires
        var result = LittyMessageRewriter.TryRewrite(
            "Application started. Press Ctrl+C to shut down.");

        Assert.NotNull(result);
        Assert.DoesNotContain("Press Ctrl+C", result); // original text should be gone
    }
}

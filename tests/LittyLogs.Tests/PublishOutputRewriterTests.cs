using LittyLogs.Tool;
using LittyLogs.Xunit;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace LittyLogs.Tests;

/// <summary>
/// making sure dotnet litty publish is slaying every MSBuild publish line fr fr 📤
/// </summary>
public class PublishOutputRewriterTests
{
    private readonly ILogger<PublishOutputRewriterTests> _logger;

    public PublishOutputRewriterTests(ITestOutputHelper output)
    {
        _logger = output.CreateLittyLogger<PublishOutputRewriterTests>();
    }

    [Fact]
    public void TryRewrite_PublishSucceeded_GetsLittyfied()
    {
        var result = PublishOutputRewriter.TryRewrite("Publish succeeded.");

        Assert.NotNull(result);
        Assert.Contains("published", result);
        Assert.Contains("deploy", result);
        _logger.LogInformation("publish succeeded got litty-fied no cap 🔥");
    }

    [Fact]
    public void TryRewrite_PublishSucceededWithTiming_PreservesTiming()
    {
        var result = PublishOutputRewriter.TryRewrite("Publish succeeded in 2.1s");

        Assert.NotNull(result);
        Assert.Contains("published", result);
        Assert.Contains("in 2.1s", result);
    }

    [Fact]
    public void TryRewrite_PublishFailed_GetsLittyfied()
    {
        var result = PublishOutputRewriter.TryRewrite("Publish FAILED.");

        Assert.NotNull(result);
        Assert.Contains("massive L", result);
    }

    [Fact]
    public void TryRewrite_SelfContained_GetsLittyfied()
    {
        var result = PublishOutputRewriter.TryRewrite("  Publishing self-contained app");

        Assert.NotNull(result);
        Assert.Contains("self-contained", result);
    }

    [Fact]
    public void TryRewrite_Trimming_GetsLittyfied()
    {
        var result = PublishOutputRewriter.TryRewrite("  Trimming unused assemblies");

        Assert.NotNull(result);
        Assert.Contains("trimming", result);
    }

    [Fact]
    public void TryRewrite_Compressing_GetsLittyfied()
    {
        var result = PublishOutputRewriter.TryRewrite("  Compressing publish artifacts");

        Assert.NotNull(result);
        Assert.Contains("squishing", result);
    }

    [Fact]
    public void TryRewrite_BuildOutputFallback_StillWorks()
    {
        // dotnet publish runs dotnet build internally — build transforms should still match
        var result = PublishOutputRewriter.TryRewrite("Determining projects to restore...");

        Assert.NotNull(result);
        Assert.Contains("deps", result);
        _logger.LogInformation("build fallback still works in publish mode bestie 💅");
    }

    [Fact]
    public void TryRewrite_BuildSucceededFallback_StillWorks()
    {
        var result = PublishOutputRewriter.TryRewrite("Build succeeded.");

        Assert.NotNull(result);
        Assert.Contains("slayed", result);
    }

    [Fact]
    public void TryRewrite_UnknownLine_ReturnsNull()
    {
        var result = PublishOutputRewriter.TryRewrite("some random output nobody expected");

        Assert.Null(result);
    }

    [Fact]
    public void TryRewrite_EmptyString_ReturnsNull()
    {
        var result = PublishOutputRewriter.TryRewrite("");

        Assert.Null(result);
    }

    [Fact]
    public void TryRewrite_ReadyToRun_GetsLittyfied()
    {
        var result = PublishOutputRewriter.TryRewrite("  Generating ReadyToRun images");

        Assert.NotNull(result);
        Assert.Contains("native code", result);
        Assert.Contains("speed", result);
        _logger.LogInformation("ReadyToRun pre-compilation slaying 🔥⚡");
    }

    [Fact]
    public void TryRewrite_NativeCodeGeneration_GetsLittyfied()
    {
        var result = PublishOutputRewriter.TryRewrite("  Generating native code");

        Assert.NotNull(result);
        Assert.Contains("native code", result);
    }

    [Fact]
    public void TryRewrite_PublishArtifactPath_GetsLittyfied()
    {
        // publish paths end with a directory, not .dll
        var result = PublishOutputRewriter.TryRewrite("  MyApp -> /app/publish/");

        Assert.NotNull(result);
        Assert.Contains("MyApp", result);
        Assert.Contains("ship", result);
    }

    [Fact]
    public void TryRewrite_BuildArtifactPath_FallsBackToBuildRewriter()
    {
        // build paths end with .dll — should fall through to build rewriter
        var result = PublishOutputRewriter.TryRewrite("  MyApp -> /bin/Debug/net10.0/MyApp.dll");

        Assert.NotNull(result);
        // build rewriter handles .dll paths
        Assert.Contains("MyApp", result);
    }

    [Fact]
    public void TryRewrite_OptimizingAssemblies_GetsLittyfied()
    {
        var result = PublishOutputRewriter.TryRewrite("  Optimizing assemblies for size");

        Assert.NotNull(result);
        Assert.Contains("trimming", result);
        Assert.Contains("skinny", result);
    }

    [Fact]
    public void TryRewrite_NullInput_ReturnsNull()
    {
        var result = PublishOutputRewriter.TryRewrite(null!);

        Assert.Null(result);
    }
}

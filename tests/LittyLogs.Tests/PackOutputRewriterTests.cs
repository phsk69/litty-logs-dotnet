using LittyLogs.Tool;
using LittyLogs.Xunit;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace LittyLogs.Tests;

/// <summary>
/// making sure dotnet litty pack is slaying every nupkg line fr fr 📦
/// </summary>
public class PackOutputRewriterTests
{
    private readonly ILogger<PackOutputRewriterTests> _logger;

    public PackOutputRewriterTests(ITestOutputHelper output)
    {
        _logger = output.CreateLittyLogger<PackOutputRewriterTests>();
    }

    [Fact]
    public void TryRewrite_SuccessfullyCreatedPackage_GetsLittyfied()
    {
        var result = PackOutputRewriter.TryRewrite(
            "  Successfully created package '/home/psk/git/public/litty-logs/src/LittyLogs/bin/Release/LittyLogs.0.1.4.nupkg'.");

        Assert.NotNull(result);
        Assert.Contains("LittyLogs.0.1.4.nupkg", result);
        Assert.Contains("NuGet", result);
        _logger.LogInformation("nupkg creation line slaying no cap 📦🔥");
    }

    [Fact]
    public void TryRewrite_SymbolsPackage_AlsoGetsLittyfied()
    {
        // .snupkg (symbols) uses the same handler — Path.GetFileName dont discriminate bestie
        var result = PackOutputRewriter.TryRewrite(
            "  Successfully created package '/path/to/LittyLogs.0.1.4.snupkg'.");

        Assert.NotNull(result);
        Assert.Contains("LittyLogs.0.1.4.snupkg", result);
        Assert.Contains("NuGet", result);
        _logger.LogInformation("symbols package also getting the treatment 💅");
    }

    [Fact]
    public void TryRewrite_PackSucceeded_GetsLittyfied()
    {
        var result = PackOutputRewriter.TryRewrite("Pack succeeded.");

        Assert.NotNull(result);
        Assert.Contains("bussin", result);
        Assert.Contains("ship", result);
        _logger.LogInformation("pack succeeded got litty-fied no cap 🔥");
    }

    [Fact]
    public void TryRewrite_PackSucceededWithTiming_PreservesTiming()
    {
        var result = PackOutputRewriter.TryRewrite("Pack succeeded in 1.2s");

        Assert.NotNull(result);
        Assert.Contains("bussin", result);
        Assert.Contains("in 1.2s", result);
    }

    [Fact]
    public void TryRewrite_PackFailed_GetsLittyfied()
    {
        var result = PackOutputRewriter.TryRewrite("Pack FAILED.");

        Assert.NotNull(result);
        Assert.Contains("massive L", result);
    }

    [Fact]
    public void TryRewrite_BuildOutputFallback_StillWorks()
    {
        // dotnet pack runs dotnet build internally — build transforms should still match
        var result = PackOutputRewriter.TryRewrite("Determining projects to restore...");

        Assert.NotNull(result);
        Assert.Contains("deps", result);
        _logger.LogInformation("build fallback still works in pack mode bestie 💅");
    }

    [Fact]
    public void TryRewrite_BuildArtifactPath_FallsBackToBuildRewriter()
    {
        // build paths end with .dll — should fall through to build rewriter
        var result = PackOutputRewriter.TryRewrite("  LittyLogs -> /bin/Release/net10.0/LittyLogs.dll");

        Assert.NotNull(result);
        Assert.Contains("built different", result);
    }

    [Fact]
    public void TryRewrite_UnknownLine_ReturnsNull()
    {
        var result = PackOutputRewriter.TryRewrite("some random output nobody expected");

        Assert.Null(result);
    }

    [Fact]
    public void TryRewrite_EmptyString_ReturnsNull()
    {
        var result = PackOutputRewriter.TryRewrite("");

        Assert.Null(result);
    }

    [Fact]
    public void TryRewrite_NullInput_ReturnsNull()
    {
        var result = PackOutputRewriter.TryRewrite(null!);

        Assert.Null(result);
    }
}

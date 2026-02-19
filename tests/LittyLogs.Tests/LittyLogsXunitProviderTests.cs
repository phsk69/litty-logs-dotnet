using LittyLogs.Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace LittyLogs.Tests;

/// <summary>
/// testing that the xUnit provider is absolutely slaying bestie 🔥
/// we mock ITestOutputHelper to capture what gets written and assert on it
/// </summary>
public class LittyLogsXunitProviderTests
{
    private readonly List<string> _capturedOutput = [];
    private readonly Mock<ITestOutputHelper> _mockOutput;

    public LittyLogsXunitProviderTests()
    {
        _mockOutput = new Mock<ITestOutputHelper>();
        _mockOutput
            .Setup(x => x.WriteLine(It.IsAny<string>()))
            .Callback<string>(msg => _capturedOutput.Add(msg));
    }

    [Fact]
    public void Provider_CreatesWorkingLoggers()
    {
        // the provider gotta create loggers that actually do something
        using var provider = new LittyLogsXunitProvider(_mockOutput.Object);
        var logger = provider.CreateLogger("TestCategory");

        logger.LogInformation("yo this works");

        Assert.Single(_capturedOutput);
        Assert.Contains("yo this works", _capturedOutput[0]);
    }

    [Theory]
    [InlineData(LogLevel.Trace, "👀")]
    [InlineData(LogLevel.Debug, "🔍")]
    [InlineData(LogLevel.Information, "🔥")]
    [InlineData(LogLevel.Warning, "😤")]
    [InlineData(LogLevel.Error, "💀")]
    [InlineData(LogLevel.Critical, "☠️")]
    public void Logger_EmitsCorrectEmojiForLogLevel(LogLevel level, string expectedEmoji)
    {
        // emoji game gotta be on point in test output too no cap
        using var provider = new LittyLogsXunitProvider(_mockOutput.Object);
        var logger = provider.CreateLogger("TestCategory");

        logger.Log(level, "test message");

        Assert.Single(_capturedOutput);
        Assert.Contains(expectedEmoji, _capturedOutput[0]);
    }

    [Theory]
    [InlineData(LogLevel.Trace, "trace")]
    [InlineData(LogLevel.Debug, "debug")]
    [InlineData(LogLevel.Information, "info")]
    [InlineData(LogLevel.Warning, "warning")]
    [InlineData(LogLevel.Error, "err")]
    [InlineData(LogLevel.Critical, "crit")]
    public void Logger_EmitsCorrectLevelLabel(LogLevel level, string expectedLabel)
    {
        using var provider = new LittyLogsXunitProvider(_mockOutput.Object);
        var logger = provider.CreateLogger("TestCategory");

        logger.Log(level, "test message");

        Assert.Single(_capturedOutput);
        Assert.Contains(expectedLabel, _capturedOutput[0]);
    }

    [Fact]
    public void Logger_ShortensCategoryByDefault()
    {
        // yeet the namespace bloat in test output too fr
        using var provider = new LittyLogsXunitProvider(_mockOutput.Object);
        var logger = provider.CreateLogger("Microsoft.Hosting.Lifetime");

        logger.LogInformation("some message");

        Assert.Contains("[Lifetime]", _capturedOutput[0]);
        Assert.DoesNotContain("Microsoft.Hosting", _capturedOutput[0]);
    }

    [Fact]
    public void Logger_RewritesFrameworkMessages()
    {
        // the secret sauce works through the xUnit provider too
        using var provider = new LittyLogsXunitProvider(_mockOutput.Object);
        var logger = provider.CreateLogger("Microsoft.Hosting.Lifetime");

        logger.LogInformation("Application started. Press Ctrl+C to shut down.");

        Assert.Contains("bussin", _capturedOutput[0]);
        Assert.DoesNotContain("Press Ctrl+C", _capturedOutput[0]);
    }

    [Fact]
    public void Logger_PassesThroughCustomMessages()
    {
        // custom messages stay untouched bestie
        using var provider = new LittyLogsXunitProvider(_mockOutput.Object);
        var logger = provider.CreateLogger("MyApp.Services");

        logger.LogInformation("my custom message about something specific");

        Assert.Contains("my custom message about something specific", _capturedOutput[0]);
    }

    [Fact]
    public void Logger_ColorsOffByDefault()
    {
        // test output usually cant render ANSI so colors off by default
        using var provider = new LittyLogsXunitProvider(_mockOutput.Object);
        var logger = provider.CreateLogger("TestCategory");

        logger.LogError("error message");

        Assert.False(_capturedOutput[0].Contains('\x1b'),
            "xUnit output should not contain ANSI escape chars by default bestie");
    }

    [Fact]
    public void Logger_IncludesTimestamp()
    {
        using var provider = new LittyLogsXunitProvider(_mockOutput.Object);
        var logger = provider.CreateLogger("TestCategory");

        logger.LogInformation("test message");

        // ISO 8601 with milliseconds
        Assert.Matches(@"\[\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}", _capturedOutput[0]);
    }

    [Fact]
    public void Logger_IncludesExceptionDetails()
    {
        using var provider = new LittyLogsXunitProvider(_mockOutput.Object);
        var logger = provider.CreateLogger("TestCategory");

        var ex = new InvalidOperationException("something flopped hard");
        logger.LogError(ex, "caught this L");

        var output = _capturedOutput[0];
        Assert.Contains("caught this L", output);
        Assert.Contains("something flopped hard", output);
    }

    [Fact]
    public void CreateLittyLogger_ConvenienceExtension_Works()
    {
        // the one-liner convenience method gotta work too
        var logger = _mockOutput.Object.CreateLittyLogger<LittyLogsXunitProviderTests>();

        logger.LogInformation("convenience method is bussin");

        Assert.Single(_capturedOutput);
        Assert.Contains("convenience method is bussin", _capturedOutput[0]);
    }

    [Fact]
    public void AddLittyLogs_WithCustomOptions_RespectsOptions()
    {
        // custom options gotta be respected no cap
        var factory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddLittyLogs(_mockOutput.Object, opts =>
            {
                opts.RewriteMessages = false;
            });
        });

        var logger = factory.CreateLogger("Microsoft.Hosting.Lifetime");
        logger.LogInformation("Application started. Press Ctrl+C to shut down.");

        // rewriting is off so the boring message should pass through
        Assert.Contains("Press Ctrl+C", _capturedOutput[0]);
    }
}

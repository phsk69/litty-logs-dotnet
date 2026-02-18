using LittyLogs.Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace LittyLogs.Tests;

/// <summary>
/// testing that the formatter absolutely slaps in every way no cap 🔥
/// dogfooding our own litty-logs xUnit integration for test output 💅
/// </summary>
public class LittyLogsFormatterTests
{
    private readonly ILogger<LittyLogsFormatterTests> _logger;

    public LittyLogsFormatterTests(ITestOutputHelper output)
    {
        _logger = output.CreateLittyLogger<LittyLogsFormatterTests>();
    }

    private static LittyLogsFormatter CreateFormatter(LittyLogsOptions? opts = null)
    {
        var monitor = new Mock<IOptionsMonitor<LittyLogsOptions>>();
        monitor.Setup(m => m.CurrentValue).Returns(opts ?? new LittyLogsOptions());
        return new LittyLogsFormatter(monitor.Object);
    }

    private static string RenderLog(
        LittyLogsFormatter formatter,
        LogLevel level,
        string category,
        string message,
        Exception? exception = null)
    {
        var writer = new StringWriter();
        var entry = new LogEntry<string>(
            level,
            category,
            new EventId(0),
            message,
            exception,
            (state, _) => state);

        formatter.Write(in entry, null, writer);
        return writer.ToString();
    }

    [Theory]
    [InlineData(LogLevel.Trace, "👀")]
    [InlineData(LogLevel.Debug, "🔍")]
    [InlineData(LogLevel.Information, "🔥")]
    [InlineData(LogLevel.Warning, "😤")]
    [InlineData(LogLevel.Error, "💀")]
    [InlineData(LogLevel.Critical, "☠️")]
    public void Write_EmitsCorrectEmojiForLogLevel(LogLevel level, string expectedEmoji)
    {
        // emoji game gotta be on point no cap
        var formatter = CreateFormatter();
        var output = RenderLog(formatter, level, "TestCategory", "test message");
        Assert.Contains(expectedEmoji, output);
    }

    [Theory]
    [InlineData(LogLevel.Trace, "TRACE")]
    [InlineData(LogLevel.Debug, "DBG")]
    [InlineData(LogLevel.Information, "INFO")]
    [InlineData(LogLevel.Warning, "WARN")]
    [InlineData(LogLevel.Error, "ERR")]
    [InlineData(LogLevel.Critical, "CRIT")]
    public void Write_EmitsCorrectLevelLabel(LogLevel level, string expectedLabel)
    {
        // level labels gotta be concise bestie
        var formatter = CreateFormatter();
        var output = RenderLog(formatter, level, "TestCategory", "test message");
        Assert.Contains(expectedLabel, output);
    }

    [Fact]
    public void Write_ShortensCategory_ToLastSegment()
    {
        // yeet the namespace bloat fr
        var formatter = CreateFormatter();
        var output = RenderLog(
            formatter, LogLevel.Information,
            "Microsoft.Hosting.Lifetime",
            "some message");

        Assert.Contains("[Lifetime]", output);
        Assert.DoesNotContain("Microsoft.Hosting", output);
    }

    [Fact]
    public void Write_KeepsCategoryWhenNoNamespace()
    {
        // no dots = keep the whole thing bestie
        var formatter = CreateFormatter();
        var output = RenderLog(
            formatter, LogLevel.Information,
            "Program",
            "starting up");

        Assert.Contains("[Program]", output);
    }

    [Fact]
    public void Write_DoesNotShortenCategory_WhenDisabled()
    {
        var opts = new LittyLogsOptions { ShortenCategories = false };
        var formatter = CreateFormatter(opts);
        var output = RenderLog(
            formatter, LogLevel.Information,
            "Microsoft.Hosting.Lifetime",
            "some message");

        Assert.Contains("[Microsoft.Hosting.Lifetime]", output);
    }

    [Fact]
    public void Write_IncludesIso8601Timestamp()
    {
        // ISO 8601 for that international rizz 🌍
        var formatter = CreateFormatter();
        var output = RenderLog(
            formatter, LogLevel.Information,
            "TestCategory",
            "test message");

        // ISO 8601 with milliseconds: yyyy-MM-ddTHH:mm:ss.fffZ (or +00:00)
        Assert.Matches(@"\[\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}", output);
    }

    [Fact]
    public void Write_IncludesAnsiColorCodes()
    {
        // colors gotta hit different in the terminal 🎨
        var formatter = CreateFormatter();
        var output = RenderLog(
            formatter, LogLevel.Error,
            "TestCategory",
            "error message");

        Assert.Contains("\x1b[", output);
        Assert.Contains("\x1b[0m", output); // reset code
    }

    [Fact]
    public void Write_OmitsAnsiCodes_WhenColorsDisabled()
    {
        var opts = new LittyLogsOptions { UseColors = false };
        var formatter = CreateFormatter(opts);
        var output = RenderLog(
            formatter, LogLevel.Error,
            "TestCategory",
            "error message");

        // gotta use ordinal comparison because CurrentCulture treats ESC as ignorable 💀
        Assert.False(output.Contains('\x1b'),
            "output should not contain ANSI escape chars when colors are disabled bestie");
    }

    [Fact]
    public void Write_RewritesKnownFrameworkMessage()
    {
        // the secret sauce — boring messages get litty-fied
        var formatter = CreateFormatter();
        var output = RenderLog(
            formatter, LogLevel.Information,
            "Microsoft.Hosting.Lifetime",
            "Application started. Press Ctrl+C to shut down.");

        Assert.Contains("bussin", output);
        Assert.DoesNotContain("Press Ctrl+C to shut down", output);
    }

    [Fact]
    public void Write_PassesThroughCustomMessages()
    {
        // custom app messages should stay untouched bestie
        var formatter = CreateFormatter();
        var customMessage = "my custom log message about something specific";
        var output = RenderLog(
            formatter, LogLevel.Information,
            "MyApp.Services.CoolService",
            customMessage);

        Assert.Contains(customMessage, output);
    }

    [Fact]
    public void Write_DoesNotRewrite_WhenRewriteDisabled()
    {
        var opts = new LittyLogsOptions { RewriteMessages = false };
        var formatter = CreateFormatter(opts);
        var output = RenderLog(
            formatter, LogLevel.Information,
            "Microsoft.Hosting.Lifetime",
            "Application started. Press Ctrl+C to shut down.");

        // original boring message should be preserved when rewriting is off
        Assert.Contains("Press Ctrl+C to shut down", output);
    }

    [Fact]
    public void Write_IncludesExceptionDetails()
    {
        // exceptions gotta show up so we can debug fr
        var formatter = CreateFormatter();
        var ex = new InvalidOperationException("something flopped hard bestie");
        var output = RenderLog(
            formatter, LogLevel.Error,
            "TestCategory",
            "log message",
            ex);

        Assert.Contains("something flopped hard bestie", output);
    }

    [Fact]
    public void Write_PreservesExistingEmojisInMessage()
    {
        // custom app messages might already have emojis, we dont mess with those
        var formatter = CreateFormatter();
        var output = RenderLog(
            formatter, LogLevel.Information,
            "TestCategory",
            "🔥 ICS feed generated, absolutely ate");

        Assert.Contains("🔥 INFO", output);  // formatter prefix
        Assert.Contains("🔥 ICS feed generated", output);  // message emoji preserved
    }

    [Fact]
    public void Write_SkipsNullMessage_WhenNoException()
    {
        // no message + no exception = no output, we dont do empty lines
        var formatter = CreateFormatter();
        var writer = new StringWriter();
        var entry = new LogEntry<string>(
            LogLevel.Information,
            "TestCategory",
            new EventId(0),
            "ignored",
            null,
            (_, _) => null!);

        formatter.Write(in entry, null, writer);
        Assert.Empty(writer.ToString());
    }
}

using LittyLogs.Xunit;
using Microsoft.Extensions.Logging;
using Xunit;

namespace LittyLogs.Example.Xunit;

/// <summary>
/// example tests showing how litty-logs makes your test output bussin 🔥
/// run with `dotnet test --verbosity normal` to see the full litty output
/// </summary>
public class ExampleTests
{
    private readonly ILogger<ExampleTests> _logger;
    private readonly ITestOutputHelper _output;

    public ExampleTests(ITestOutputHelper output)
    {
        _output = output;
        // one line to litty-fy your test output bestie 💅
        _logger = output.CreateLittyLogger<ExampleTests>();
    }

    [Fact]
    public void FullEmojiSpectrum_InTestOutput()
    {
        // every log level with its own emoji, right in your test results
        _logger.LogTrace("trace level — for when you lowkey wanna see everything 👀");
        _logger.LogDebug("debug level — investigating whats going on 🔍");
        _logger.LogInformation("info level — everything bussin fr fr 🔥");
        _logger.LogWarning("warning level — something kinda sus 😤");
        _logger.LogError("error level — took a fat L 💀");
        _logger.LogCritical("critical level — absolute catastrophe ☠️");
    }

    [Fact]
    public void FrameworkMessages_GetLittyfied()
    {
        // even if framework code logs boring messages, they get rewritten
        var frameworkLogger = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddLittyLogs(
                new TestOutputHelperProxy(
                    msg => _logger.LogInformation(msg)));
        }).CreateLogger("Microsoft.Hosting.Lifetime");

        // this would normally say "Application started. Press Ctrl+C to shut down."
        // but litty-logs rewrites it into gen alpha slang no cap
        _logger.LogInformation("Application started. Press Ctrl+C to shut down.");
    }

    [Fact]
    public void CustomMessages_PassThroughUntouched()
    {
        // your own messages keep their original text, just get the litty formatting
        _logger.LogInformation("my custom business logic message stays exactly like this");
        _logger.LogWarning("something specific to my app that shouldnt be rewritten");
    }

    [Fact]
    public void TimestampFirst_ShowsObservabilityOrdering()
    {
        // timestamp-first mode puts the timestamp before the level label
        // for when observability tools are sorting by timestamp bestie 📊
        var tsFirstLogger = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddLittyLogs(_output, opts => opts.TimestampFirst = true);
        }).CreateLogger("TimestampFirstDemo");

        tsFirstLogger.LogInformation("timestamp leads in observability mode 📊");
        tsFirstLogger.LogWarning("same vibes different ordering 😤");
        tsFirstLogger.LogError("even Ls look organized with timestamp-first 💀");
    }

    [Fact]
    public void ExceptionLogging_ShowsDetails()
    {
        try
        {
            throw new InvalidOperationException("something flopped hard bestie");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "caught this L, heres the deets");
        }
    }
}

/// <summary>
/// lil proxy wrapper just for the framework message demo above
/// </summary>
internal class TestOutputHelperProxy(Action<string> writeLine) : ITestOutputHelper
{
    public string Output => string.Empty;
    public void Write(string message) => writeLine(message);
    public void Write(string format, params object[] args) => writeLine(string.Format(format, args));
    public void WriteLine(string message) => writeLine(message);
    public void WriteLine(string format, params object[] args) => writeLine(string.Format(format, args));
}

using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace LittyLogs.Xunit;

/// <summary>
/// ILogger that formats with litty-logs and writes to xUnit's ITestOutputHelper.
/// same formatting brain as the console formatter, just different delivery bestie 💅
/// </summary>
public sealed class LittyLogsXunitLogger(
    ITestOutputHelper output,
    string categoryName,
    LittyLogsOptions options) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        if (message is null && exception is null)
            return;

        // rewrite boring framework messages into gen alpha slang
        message = LittyLogsFormatHelper.RewriteIfNeeded(message, options.RewriteMessages);

        // format using the shared brain that all providers eat from
        var formatted = LittyLogsFormatHelper.FormatLogLine(
            logLevel,
            categoryName,
            message,
            exception,
            options);

        try
        {
            output.WriteLine(formatted);
        }
        catch (InvalidOperationException)
        {
            // xUnit throws if you write to ITestOutputHelper after the test is done
            // we just swallow it because thats just how it be sometimes 🤷
        }
    }
}

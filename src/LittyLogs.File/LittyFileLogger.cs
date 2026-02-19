using Microsoft.Extensions.Logging;

namespace LittyLogs.File;

/// <summary>
/// ILogger that formats with litty-logs and enqueues to the async file writer.
/// same formatting brain as console and xunit, just different delivery bestie 📁🔥
/// </summary>
internal sealed class LittyFileLogger(
    string categoryName,
    LittyFileWriter writer,
    LittyFileLogsOptions fileOptions,
    LittyLogsOptions littyOptions) : ILogger
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
        message = LittyLogsFormatHelper.RewriteIfNeeded(message, littyOptions.RewriteMessages);

        // format using the shared brain — text or JSON depending on the vibe
        var formatted = fileOptions.OutputFormat switch
        {
            LittyFileOutputFormat.Json => LittyLogsFormatHelper.FormatJsonLine(
                logLevel, categoryName, message, exception, eventId, littyOptions),
            _ => LittyLogsFormatHelper.FormatLogLine(
                logLevel, categoryName, message, exception, littyOptions)
        };

        // yeet it into the channel — returns immediately, non-blocking king 👑
        writer.Enqueue(formatted);
    }
}

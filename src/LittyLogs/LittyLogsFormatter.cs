using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace LittyLogs;

/// <summary>
/// the console formatter that makes ALL logs bussin with emojis, ANSI colors,
/// AND rewrites framework messages into gen alpha slang fr fr 🔥
/// uses LittyLogsFormatHelper under the hood so the formatting logic is shared
/// with every other provider (xunit, file sink, json, whatever comes next)
/// </summary>
public sealed class LittyLogsFormatter(IOptionsMonitor<LittyLogsOptions> options)
    : ConsoleFormatter("litty-logs")
{
    public override void Write<TState>(
        in LogEntry<TState> logEntry,
        IExternalScopeProvider? scopeProvider,
        TextWriter textWriter)
    {
        var message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);
        if (message is null && logEntry.Exception is null)
            return;

        var opts = options.CurrentValue;

        // rewrite boring framework messages into gen alpha slang
        message = LittyLogsFormatHelper.RewriteIfNeeded(message, opts.RewriteMessages);

        // format the whole line using the shared brain
        var formatted = LittyLogsFormatHelper.FormatLogLine(
            logEntry.LogLevel,
            logEntry.Category,
            message,
            logEntry.Exception,
            opts);

        textWriter.WriteLine(formatted);
    }
}

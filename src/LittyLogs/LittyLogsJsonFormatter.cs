using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace LittyLogs;

/// <summary>
/// console formatter that outputs fire JSON instead of plain text.
/// same litty rewrites and emojis, just structured for log aggregators to eat 🍽️
/// emojis in JSON? absolutely bussin — JSON is UTF-8 native bestie 🔥
/// </summary>
public sealed class LittyLogsJsonFormatter(IOptionsMonitor<LittyLogsOptions> options)
    : ConsoleFormatter("litty-json")
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

        // format the whole line as fire JSON using the shared brain
        var formatted = LittyLogsFormatHelper.FormatJsonLine(
            logEntry.LogLevel,
            logEntry.Category,
            message,
            logEntry.Exception,
            logEntry.EventId,
            opts);

        textWriter.WriteLine(formatted);
    }
}

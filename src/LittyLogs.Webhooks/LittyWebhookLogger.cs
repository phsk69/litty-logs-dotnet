using Microsoft.Extensions.Logging;

namespace LittyLogs.Webhooks;

/// <summary>
/// ILogger that formats with litty-logs and enqueues to the async webhook writer.
/// same formatting brain as console and file, just yeeting to chat instead bestie 🪝🔥
/// filters by MinimumLevel so we dont spam the group chat with trace logs 🔒
/// </summary>
internal sealed class LittyWebhookLogger(
    string categoryName,
    LittyWebhookWriter writer,
    LittyWebhookOptions webhookOptions,
    LittyLogsOptions littyOptions) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) =>
        logLevel != LogLevel.None && logLevel >= webhookOptions.MinimumLevel;

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

        // format using the shared brain — no colors because webhooks dont do ANSI
        var formatted = LittyLogsFormatHelper.FormatLogLine(
            logLevel, categoryName, message, null, littyOptions);

        // Matrix turns our exact fence into escaped pre/code HTML; Slack removes only the fence 🔥
        // and keeps the multiline exception as safe plain text. one stack trace, zero dupes 🔒🔥
        if (exception is not null)
        {
            formatted += $"\n```\n{exception}\n```";
        }

        // yeet it into the channel — returns immediately, non-blocking king 👑
        writer.Enqueue(new WebhookMessage(formatted));
    }
}

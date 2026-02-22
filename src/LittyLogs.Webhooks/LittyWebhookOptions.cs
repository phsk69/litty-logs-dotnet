using Microsoft.Extensions.Logging;

namespace LittyLogs.Webhooks;

/// <summary>
/// options for the litty webhook sink. configure where logs go, how they batch,
/// and what level starts yeeting to chat bestie 🪝🔥
/// </summary>
public class LittyWebhookOptions
{
    /// <summary>webhook URL to POST to. for Matrix hookshot this IS the auth, no headers needed 🔗</summary>
    public string WebhookUrl { get; set; } = "";

    /// <summary>which platform we targeting bestie 🎯</summary>
    public WebhookPlatform Platform { get; set; } = WebhookPlatform.Matrix;

    /// <summary>
    /// minimum log level to send to chat. defaults to Warning because
    /// nobody wants their chat spammed with trace logs thats not it 💀
    /// </summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Warning;

    /// <summary>bot display name in chat. "LittyLogs" by default because thats fire 🤖</summary>
    public string Username { get; set; } = "LittyLogs";

    /// <summary>max messages per batch before we flush. 10 by default no cap 📦</summary>
    public int BatchSize { get; set; } = 10;

    /// <summary>max time to wait before flushing a partial batch. 2s by default ⏱️</summary>
    public TimeSpan BatchInterval { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// whether to rewrite framework messages into gen alpha slang.
    /// true by default because thats literally the whole point bestie 💅
    /// </summary>
    public bool RewriteMessages { get; set; } = true;

    /// <summary>
    /// whether to shorten category names (yeet the namespace bloat).
    /// Microsoft.Hosting.Lifetime becomes just Lifetime fr fr
    /// </summary>
    public bool ShortenCategories { get; set; } = true;

    /// <summary>
    /// put timestamp before level label in text output.
    /// false = RFC 5424 style: [emoji level] [timestamp] [category] message (default)
    /// true = observability style: [timestamp] [emoji level] [category] message
    /// </summary>
    public bool TimestampFirst { get; set; } = false;

    /// <summary>whether to use UTC timestamps. true by default for that international rizz 🌍</summary>
    public bool UseUtcTimestamp { get; set; } = true;

    /// <summary>timestamp format string. ISO 8601 with milliseconds by default 📅</summary>
    public string TimestampFormat { get; set; } = "yyyy-MM-ddTHH:mm:ss.fffK";

    /// <summary>
    /// creates a LittyLogsOptions from these webhook options so we can reuse the shared brain.
    /// UseColors is ALWAYS false because ANSI in webhooks is even more cursed than files 💀
    /// </summary>
    internal LittyLogsOptions ToLittyLogsOptions() => new()
    {
        RewriteMessages = RewriteMessages,
        UseColors = false, // ANSI codes in webhooks is NOT it bestie 💀
        ShortenCategories = ShortenCategories,
        UseUtcTimestamp = UseUtcTimestamp,
        TimestampFormat = TimestampFormat,
        TimestampFirst = TimestampFirst
    };
}

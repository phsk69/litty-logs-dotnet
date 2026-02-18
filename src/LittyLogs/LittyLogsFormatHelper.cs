using System.Text;
using Microsoft.Extensions.Logging;

namespace LittyLogs;

/// <summary>
/// the shared brain of litty-logs — all providers (console, xunit, file sink, json, whatever)
/// eat from this one source of truth for formatting. no duplicating vibes ever 🧠
/// </summary>
public static class LittyLogsFormatHelper
{
    // ANSI escape codes that hit different 🎨
    public const string Reset = "\x1b[0m";
    public const string Dim = "\x1b[2m";
    public const string Cyan = "\x1b[36m";
    public const string Blue = "\x1b[34m";
    public const string Green = "\x1b[32m";
    public const string Yellow = "\x1b[33m";
    public const string Red = "\x1b[31m";
    public const string BrightRed = "\x1b[91m";

    /// <summary>
    /// gets the emoji, level label, and ANSI color for a log level.
    /// this is the litty level info that makes every log line bussin 🔥
    /// </summary>
    public static (string Emoji, string Label, string Color) GetLevelInfo(LogLevel level) => level switch
    {
        LogLevel.Trace       => ("👀", "TRACE", Cyan),
        LogLevel.Debug       => ("🔍", "DBG",   Blue),
        LogLevel.Information => ("🔥", "INFO",  Green),
        LogLevel.Warning     => ("😤", "WARN",  Yellow),
        LogLevel.Error       => ("💀", "ERR",   Red),
        LogLevel.Critical    => ("☠️",  "CRIT",  BrightRed),
        _                    => ("❓", "???",   Reset)
    };

    /// <summary>
    /// yeets the namespace bloat from category names.
    /// Microsoft.Hosting.Lifetime becomes just Lifetime fr fr
    /// </summary>
    public static string ShortenCategory(string category) =>
        category.Contains('.')
            ? category[(category.LastIndexOf('.') + 1)..]
            : category;

    /// <summary>
    /// rewrites framework messages if enabled, otherwise passes through untouched
    /// </summary>
    public static string? RewriteIfNeeded(string? message, bool rewriteEnabled)
    {
        if (message is null || !rewriteEnabled)
            return message;

        return LittyMessageRewriter.TryRewrite(message) ?? message;
    }

    /// <summary>
    /// formats a complete log line with all the litty goodness.
    /// this is the core method every provider calls — console, xunit, file sink, whatever.
    /// returns the fully formatted string ready to write 🔥
    /// </summary>
    public static string FormatLogLine(
        LogLevel level,
        string category,
        string? message,
        Exception? exception,
        LittyLogsOptions options)
    {
        var (emoji, levelLabel, color) = GetLevelInfo(level);

        var displayCategory = options.ShortenCategories
            ? ShortenCategory(category)
            : category;

        var now = options.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now;
        var timestampFmt = options.TimestampFormat ?? "yyyy-MM-ddTHH:mm:ss.fffK";
        var timestamp = now.ToString(timestampFmt);

        var sb = new StringBuilder();

        if (options.UseColors)
        {
            sb.Append($"{color}[{emoji} {levelLabel}]{Reset} ");
            sb.Append($"{Dim}[{timestamp}] [{displayCategory}]{Reset} ");
        }
        else
        {
            sb.Append($"[{emoji} {levelLabel}] ");
            sb.Append($"[{timestamp}] [{displayCategory}] ");
        }

        sb.Append(message);

        if (exception is not null)
        {
            sb.AppendLine();
            if (options.UseColors)
                sb.Append($"{Red}  {exception}{Reset}");
            else
                sb.Append($"  {exception}");
        }

        return sb.ToString();
    }
}

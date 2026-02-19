using System.Buffers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
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
        LogLevel.Trace       => ("👀", "trace",   Cyan),
        LogLevel.Debug       => ("🔍", "debug",   Blue),
        LogLevel.Information => ("🔥", "info",    Green),
        LogLevel.Warning     => ("😤", "warning", Yellow),
        LogLevel.Error       => ("💀", "err",     Red),
        LogLevel.Critical    => ("☠️",  "crit",    BrightRed),
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

        // build the two bracket segments then order based on config
        var levelBracket = options.UseColors
            ? $"{color}[{emoji} {levelLabel}]{Reset} "
            : $"[{emoji} {levelLabel}] ";

        var timestampBracket = options.UseColors
            ? $"{Dim}[{timestamp}] [{displayCategory}]{Reset} "
            : $"[{timestamp}] [{displayCategory}] ";

        if (options.TimestampFirst)
        {
            sb.Append(timestampBracket);
            sb.Append(levelBracket);
        }
        else
        {
            sb.Append(levelBracket);
            sb.Append(timestampBracket);
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

    /// <summary>
    /// formats a single JSON log line with all the litty fields.
    /// uses Utf8JsonWriter for that zero-alloc king energy 👑
    /// emojis serialize perfectly because JSON is UTF-8 native bestie 🔥
    /// </summary>
    public static string FormatJsonLine(
        LogLevel level,
        string category,
        string? message,
        Exception? exception,
        EventId eventId,
        LittyLogsOptions options)
    {
        var (emoji, levelLabel, _) = GetLevelInfo(level);

        var displayCategory = options.ShortenCategories
            ? ShortenCategory(category)
            : category;

        var now = options.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now;
        var timestampFmt = options.TimestampFormat ?? "yyyy-MM-ddTHH:mm:ss.fffK";
        var timestamp = now.ToString(timestampFmt);

        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions
        {
            // no indentation — one JSON object per line, compact and bussin
            // UnsafeRelaxedJsonEscaping keeps BMP chars literal (emojis, em dashes, plus signs)
            // so Loki/Grafana can actually search by emoji in raw JSON text 🔍
            // supplementary plane emojis (above U+FFFF) still get surrogate pair escaped
            // by Utf8JsonWriter regardless — UnescapeSurrogatePairs() handles those in post 💅
            Indented = false,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        writer.WriteStartObject();
        writer.WriteString("timestamp", timestamp);
        writer.WriteString("level", levelLabel);
        writer.WriteString("emoji", emoji);
        writer.WriteString("category", displayCategory);

        if (message is not null)
            writer.WriteString("message", message);

        if (eventId.Id != 0 || eventId.Name is not null)
        {
            writer.WriteStartObject("eventId");
            writer.WriteNumber("id", eventId.Id);
            if (eventId.Name is not null)
                writer.WriteString("name", eventId.Name);
            writer.WriteEndObject();
        }

        if (exception is not null)
        {
            writer.WriteStartObject("exception");
            writer.WriteString("type", exception.GetType().FullName);
            writer.WriteString("message", exception.Message);
            writer.WriteString("stackTrace", exception.StackTrace);
            if (exception.InnerException is not null)
                writer.WriteString("innerException", exception.InnerException.ToString());
            writer.WriteEndObject();
        }

        writer.WriteEndObject();
        writer.Flush();

        var json = Encoding.UTF8.GetString(buffer.WrittenSpan);

        // UnsafeRelaxedJsonEscaping handles BMP chars but Utf8JsonWriter still escapes
        // supplementary plane emojis (above U+FFFF) as surrogate pairs like \uD83D\uDD25
        // we convert those back to literal UTF-8 so Loki searches actually hit on emojis 🔥
        return UnescapeSurrogatePairs(json);
    }

    /// <summary>
    /// converts JSON surrogate pair escapes (\uD83D\uDD25) back to literal UTF-8 characters (🔥).
    /// even with UnsafeRelaxedJsonEscaping, Utf8JsonWriter still escapes supplementary plane chars
    /// (above U+FFFF) as surrogate pairs. we need literal emojis so Loki can search em bestie 💅
    /// </summary>
    private static readonly Regex SurrogatePairRegex = new(
        @"\\u([dD][89aAbB][0-9a-fA-F]{2})\\u([dD][cCdDeEfF][0-9a-fA-F]{2})",
        RegexOptions.Compiled);

    private static string UnescapeSurrogatePairs(string json)
    {
        return SurrogatePairRegex.Replace(json, match =>
        {
            var hi = Convert.ToInt32(match.Groups[1].Value, 16);
            var lo = Convert.ToInt32(match.Groups[2].Value, 16);
            return char.ConvertFromUtf32(char.ConvertToUtf32((char)hi, (char)lo));
        });
    }
}

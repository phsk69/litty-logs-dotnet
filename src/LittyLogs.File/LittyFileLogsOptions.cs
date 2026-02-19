namespace LittyLogs.File;

/// <summary>
/// output format for the file sink — text for humans, JSON for machines 🍽️
/// </summary>
public enum LittyFileOutputFormat
{
    /// <summary>plain text with emojis and litty formatting — same vibe as console output 🔥</summary>
    Text,

    /// <summary>structured JSON with emojis — log aggregators gonna eat GOOD 🍽️</summary>
    Json
}

/// <summary>
/// when to rotate the log file — daily is the default bestie 📅
/// </summary>
public enum LittyRollingInterval
{
    /// <summary>never rotate based on time — only size rotation if configured</summary>
    None,

    /// <summary>rotate daily — app-20260219.log 📅</summary>
    Daily,

    /// <summary>rotate hourly — app-20260219-14.log ⏰</summary>
    Hourly
}

/// <summary>
/// compression for rotated log files — active file stays uncompressed always 🗜️
/// </summary>
public enum LittyCompressionMode
{
    /// <summary>no compression — rotated files stay as-is</summary>
    None,

    /// <summary>gzip compression — rotated files become .gz, uses BCL GZipStream (zero deps) 🗜️</summary>
    Gzip
}

/// <summary>
/// options for the litty file sink. configure where logs go, how they rotate,
/// and whether machines or humans are eating the output bestie 📁🔥
/// </summary>
public class LittyFileLogsOptions
{
    /// <summary>
    /// path to the log file. directories get auto-created if they dont exist.
    /// default: "logs/app.log" because thats a vibe 📁
    /// </summary>
    public string FilePath { get; set; } = "logs/app.log";

    /// <summary>
    /// output format — Text for human vibes, Json for machine consumption 🍽️
    /// </summary>
    public LittyFileOutputFormat OutputFormat { get; set; } = LittyFileOutputFormat.Text;

    /// <summary>
    /// max file size in bytes before rotation kicks in. 0 = no size rotation.
    /// default: 0 (no limit, rotate on interval only)
    /// </summary>
    public long MaxFileSizeBytes { get; set; }

    /// <summary>
    /// time-based rolling interval for log rotation 📅
    /// </summary>
    public LittyRollingInterval RollingInterval { get; set; } = LittyRollingInterval.None;

    /// <summary>
    /// compression mode for OLD rotated files. active file stays uncompressed always.
    /// rotated files become .gz when Gzip is selected 🗜️
    /// </summary>
    public LittyCompressionMode CompressionMode { get; set; } = LittyCompressionMode.None;

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
    /// only affects Text format — JSON always outputs timestamp first as a key 📊
    /// </summary>
    public bool TimestampFirst { get; set; } = false;

    /// <summary>
    /// whether to use UTC timestamps. true by default for that international rizz 🌍
    /// </summary>
    public bool UseUtcTimestamp { get; set; } = true;

    /// <summary>
    /// timestamp format string. ISO 8601 with milliseconds by default 📅
    /// </summary>
    public string TimestampFormat { get; set; } = "yyyy-MM-ddTHH:mm:ss.fffK";

    /// <summary>
    /// creates a LittyLogsOptions from these file options so we can reuse the shared brain.
    /// UseColors is ALWAYS false because ANSI in files is NOT it 💀
    /// </summary>
    internal LittyLogsOptions ToLittyLogsOptions() => new()
    {
        RewriteMessages = RewriteMessages,
        UseColors = false, // ANSI codes in files is cursed af 💀
        ShortenCategories = ShortenCategories,
        UseUtcTimestamp = UseUtcTimestamp,
        TimestampFormat = TimestampFormat,
        TimestampFirst = TimestampFirst
    };
}

using Microsoft.Extensions.Logging.Console;

namespace LittyLogs;

/// <summary>
/// options for the litty logs formatter bestie, inherits ConsoleFormatterOptions no cap
/// </summary>
public class LittyLogsOptions : ConsoleFormatterOptions
{
    /// <summary>
    /// whether to rewrite framework messages into gen alpha slang.
    /// true by default because thats literally the whole point bestie 💅
    /// </summary>
    public bool RewriteMessages { get; set; } = true;

    /// <summary>
    /// whether to use ANSI color codes in output.
    /// disable if your terminal cant handle the drip 🎨
    /// </summary>
    public bool UseColors { get; set; } = true;

    /// <summary>
    /// whether to shorten category names (yeet the namespace bloat).
    /// Microsoft.Hosting.Lifetime becomes just Lifetime fr fr
    /// </summary>
    public bool ShortenCategories { get; set; } = true;

    public LittyLogsOptions()
    {
        // ISO 8601 timestamps for that international rizz 🌍
        TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffK";
        UseUtcTimestamp = true;
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LittyLogs;

/// <summary>
/// extension methods to add litty logs to your app bestie.
/// one line is all you need no cap 💅
/// </summary>
public static class LittyLoggingExtensions
{
    /// <summary>
    /// adds the litty logs formatter. replaces default console formatting with
    /// gen alpha slang, emojis, and ANSI colors. framework messages get fully rewritten.
    /// this is the one liner that changes everything fr fr 🔥
    /// </summary>
    public static ILoggingBuilder AddLittyLogs(this ILoggingBuilder builder)
    {
        builder.AddConsole(options => options.FormatterName = "litty-logs");
        builder.AddConsoleFormatter<LittyLogsFormatter, LittyLogsOptions>();
        return builder;
    }

    /// <summary>
    /// adds litty logs with custom options for when you need to configure the vibe ✨
    /// </summary>
    public static ILoggingBuilder AddLittyLogs(
        this ILoggingBuilder builder,
        Action<LittyLogsOptions> configure)
    {
        builder.AddLittyLogs();
        builder.Services.Configure(configure);
        return builder;
    }

    /// <summary>
    /// adds litty-fied JSON console logging. same rewrites and emojis,
    /// but structured as fire JSON for log aggregators to eat 🍽️
    /// one liner setup: builder.Logging.AddLittyJsonLogs() and youre eating no cap
    /// </summary>
    public static ILoggingBuilder AddLittyJsonLogs(this ILoggingBuilder builder)
    {
        builder.AddConsole(options => options.FormatterName = "litty-json");
        builder.AddConsoleFormatter<LittyLogsJsonFormatter, LittyLogsOptions>();
        return builder;
    }

    /// <summary>
    /// adds litty JSON logs with custom options for when you need to configure the vibe ✨
    /// </summary>
    public static ILoggingBuilder AddLittyJsonLogs(
        this ILoggingBuilder builder,
        Action<LittyLogsOptions> configure)
    {
        builder.AddLittyJsonLogs();
        builder.Services.Configure(configure);
        return builder;
    }
}

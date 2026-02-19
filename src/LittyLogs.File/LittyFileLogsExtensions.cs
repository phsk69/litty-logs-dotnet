using Microsoft.Extensions.Logging;

namespace LittyLogs.File;

/// <summary>
/// extension methods for litty-fying your log files bestie.
/// one liner and your logs are hitting disk with emojis and gen alpha energy 📁🔥
/// </summary>
public static class LittyFileLogsExtensions
{
    /// <summary>
    /// adds litty-fied file logging with default options.
    /// logs go to "logs/app.log" in text format by default bestie 📁
    /// </summary>
    public static ILoggingBuilder AddLittyFileLogs(this ILoggingBuilder builder)
    {
        builder.AddProvider(new LittyFileLogsProvider());
        return builder;
    }

    /// <summary>
    /// adds litty-fied file logging to a specific path.
    /// one liner: builder.Logging.AddLittyFileLogs("logs/myapp.log") 🔥
    /// </summary>
    public static ILoggingBuilder AddLittyFileLogs(
        this ILoggingBuilder builder,
        string filePath)
    {
        var options = new LittyFileLogsOptions { FilePath = filePath };
        builder.AddProvider(new LittyFileLogsProvider(options));
        return builder;
    }

    /// <summary>
    /// adds litty-fied file logging with full options for when you need to configure the vibe ✨
    /// </summary>
    public static ILoggingBuilder AddLittyFileLogs(
        this ILoggingBuilder builder,
        Action<LittyFileLogsOptions> configure)
    {
        var options = new LittyFileLogsOptions();
        configure(options);
        builder.AddProvider(new LittyFileLogsProvider(options));
        return builder;
    }
}

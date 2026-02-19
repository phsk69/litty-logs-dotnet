using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace LittyLogs.Xunit;

/// <summary>
/// extension methods for litty-fying your xUnit test output bestie.
/// one line and your tests are bussin no cap 💅
/// </summary>
public static class LittyLogsXunitExtensions
{
    /// <summary>
    /// adds litty-logs formatting to xUnit test output through ITestOutputHelper.
    /// colors are off by default since most test runners dont render ANSI 🎨
    /// </summary>
    public static ILoggingBuilder AddLittyLogs(
        this ILoggingBuilder builder,
        ITestOutputHelper output)
    {
        var options = new LittyLogsOptions { UseColors = false };
        builder.AddProvider(new LittyLogsXunitProvider(output, options));
        return builder;
    }

    /// <summary>
    /// adds litty-logs to xUnit with custom options for when you need to configure the vibe ✨
    /// </summary>
    public static ILoggingBuilder AddLittyLogs(
        this ILoggingBuilder builder,
        ITestOutputHelper output,
        Action<LittyLogsOptions> configure)
    {
        var options = new LittyLogsOptions { UseColors = false };
        configure(options);
        builder.AddProvider(new LittyLogsXunitProvider(output, options));
        return builder;
    }

    /// <summary>
    /// convenience one-liner that creates a litty logger directly from ITestOutputHelper.
    /// for when you just need a logger and dont wanna mess with LoggerFactory bestie 🔥
    /// </summary>
    public static ILogger<T> CreateLittyLogger<T>(this ITestOutputHelper output)
    {
        var factory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddLittyLogs(output);
        });
        return factory.CreateLogger<T>();
    }

    /// <summary>
    /// same as CreateLittyLogger but with a string category name instead of generic type
    /// </summary>
    public static ILogger CreateLittyLogger(this ITestOutputHelper output, string categoryName)
    {
        var factory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddLittyLogs(output);
        });
        return factory.CreateLogger(categoryName);
    }
}

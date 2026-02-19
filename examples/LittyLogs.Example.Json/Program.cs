using LittyLogs;
using Microsoft.Extensions.Logging;

// === JSON logging — structured output for log aggregators to eat GOOD 🍽️ ===
Console.WriteLine("=== JSON mode (machines eat good) ===");
Console.WriteLine();

using (var factory = LoggerFactory.Create(logging =>
{
    logging.SetMinimumLevel(LogLevel.Trace);
    logging.AddLittyJsonLogs();
}))
{
    var logger = factory.CreateLogger("LittyJson");

    // every log level as fire JSON — emojis serialize perfectly because JSON is UTF-8 native 🔥
    logger.LogTrace("this is trace level — lowkey peeking at everything 👀");
    logger.LogDebug("debug level — investigating the vibes bestie 🔍");
    logger.LogInformation("info level — everything is bussin fr fr 🔥");
    logger.LogWarning("warning level — something kinda sus rn 😤");
    logger.LogError("error level — big L detected 💀");
    logger.LogCritical("critical level — its giving death bestie ☠️");

    // framework message rewrite works in JSON too no cap
    logger.LogInformation("Application started. Press Ctrl+C to shut down.");

    // custom messages with emojis stay untouched
    logger.LogInformation("vibes are immaculate 💅✨ and the JSON is valid no cap");
}

Console.WriteLine();

// === timestamp-first + JSON — key order stays the same bestie ===
Console.WriteLine("=== timestamp-first + JSON (key order stays the same) ===");
Console.WriteLine();
Console.WriteLine("  fun fact: TimestampFirst only affects text output ordering (brackets).");
Console.WriteLine("  JSON always outputs timestamp as the first key regardless — thats just how JSON rolls 🍽️");
Console.WriteLine();

using (var factory = LoggerFactory.Create(logging =>
{
    logging.SetMinimumLevel(LogLevel.Trace);
    logging.AddLittyJsonLogs(opts => opts.TimestampFirst = true);
}))
{
    var logger = factory.CreateLogger("LittyJson");

    logger.LogInformation("timestamp key is already first in JSON no matter what 📊");
    logger.LogWarning("TimestampFirst is a text mode thing bestie 😤");
}

Console.WriteLine();
Console.WriteLine("^ all valid JSON bestie. emojis, rewrites, structured fields — the whole package no cap 📦🔥");

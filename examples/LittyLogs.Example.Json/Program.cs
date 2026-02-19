using LittyLogs;
using Microsoft.Extensions.Logging;

// JSON logging setup — one liner and your log aggregator is eating GOOD 🍽️
using var factory = LoggerFactory.Create(logging =>
{
    logging.SetMinimumLevel(LogLevel.Trace);
    logging.AddLittyJsonLogs();
});

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

Console.WriteLine();
Console.WriteLine("^ thats all valid JSON bestie. every line parses perfectly 🔥");
Console.WriteLine("  emojis, rewrites, structured fields — the whole package no cap 📦");

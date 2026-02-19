using LittyLogs;
using Microsoft.Extensions.Logging;

// === text mode — the OG litty-logs experience 🔥 ===
Console.WriteLine("=== text mode (the OG) ===");
Console.WriteLine();

using (var factory = LoggerFactory.Create(logging =>
{
    logging.SetMinimumLevel(LogLevel.Trace);
    logging.AddLittyLogs();
}))
{
    var logger = factory.CreateLogger("LittyScript");

    // every log level on display so you can see the full emoji spectrum bestie
    logger.LogTrace("this is trace level — for when you lowkey wanna see everything 👀");
    logger.LogDebug("debug level — investigating whats going on under the hood 🔍");
    logger.LogInformation("info level — everything is bussin and vibing fr fr 🔥");
    logger.LogWarning("warning level — something kinda sus but we not panicking yet 😤");
    logger.LogError("error level — something took a fat L and we gotta deal with it 💀");
    logger.LogCritical("critical level — absolute disaster mode, its giving catastrophe ☠️");

    // framework message rewrite in action
    logger.LogInformation("Application started. Press Ctrl+C to shut down.");
}

Console.WriteLine();

// === JSON mode — structured output for log aggregators to eat 🍽️ ===
Console.WriteLine("=== JSON mode (machines eat good) ===");
Console.WriteLine();

using (var factory = LoggerFactory.Create(logging =>
{
    logging.SetMinimumLevel(LogLevel.Trace);
    logging.AddLittyJsonLogs();
}))
{
    var logger = factory.CreateLogger("LittyScript");

    // same messages, now as fire JSON with emojis
    logger.LogTrace("this is trace level — for when you lowkey wanna see everything 👀");
    logger.LogDebug("debug level — investigating whats going on under the hood 🔍");
    logger.LogInformation("info level — everything is bussin and vibing fr fr 🔥");
    logger.LogWarning("warning level — something kinda sus but we not panicking yet 😤");
    logger.LogError("error level — something took a fat L and we gotta deal with it 💀");
    logger.LogCritical("critical level — absolute disaster mode, its giving catastrophe ☠️");

    // framework message rewrite lands in JSON too no cap
    logger.LogInformation("Application started. Press Ctrl+C to shut down.");
}

Console.WriteLine();

// === timestamp-first mode — observability style for the sort key besties 📊 ===
Console.WriteLine("=== timestamp-first mode (observability style) ===");
Console.WriteLine();

using (var factory = LoggerFactory.Create(logging =>
{
    logging.SetMinimumLevel(LogLevel.Trace);
    logging.AddLittyLogs(opts => opts.TimestampFirst = true);
}))
{
    var logger = factory.CreateLogger("LittyScript");

    // same messages but timestamp leads — RFC 5424 who? observability era rn
    logger.LogTrace("this is trace level — timestamp comes first for the sort key besties 👀");
    logger.LogInformation("info level — timestamp-first is bussin for log aggregators 🔥");
    logger.LogError("error level — even Ls look organized with timestamp-first 💀");

    // framework message rewrite still slaps in this mode too
    logger.LogInformation("Application started. Press Ctrl+C to shut down.");
}

Console.WriteLine();
Console.WriteLine("three modes, all bussin — text, JSON, and timestamp-first 🔥🍽️📊");

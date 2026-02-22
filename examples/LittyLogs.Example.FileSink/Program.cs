using LittyLogs;
using LittyLogs.File;
using Microsoft.Extensions.Logging;

// meta logger for structural output — we eat our own dogfood bestie 🐕
using var meta = LoggerFactory.Create(l => l.AddLittyLogs());
var log = meta.CreateLogger("FileSinkExample");

var logDir = Path.Combine(AppContext.BaseDirectory, "logs");

log.LogInformation("litty-logs file sink example — logs hitting disk with emojis no cap 📁");

// === text file output (level-first, RFC 5424 default) ===
log.LogInformation("=== text file output (level-first) ===");
var textLogPath = Path.Combine(logDir, "text", "app.log");

using (var factory = LoggerFactory.Create(logging =>
{
    logging.SetMinimumLevel(LogLevel.Trace);
    logging.AddLittyFileLogs(opts =>
    {
        opts.FilePath = textLogPath;
        opts.OutputFormat = LittyFileOutputFormat.Text;
        opts.RollingInterval = LittyRollingInterval.Daily;
    });
}))
{
    var logger = factory.CreateLogger("FileSinkDemo");

    logger.LogTrace("trace level — lowkey peeking at everything 👀");
    logger.LogDebug("debug level — investigating the vibes 🔍");
    logger.LogInformation("info level — everything is bussin fr fr 🔥");
    logger.LogWarning("warning level — something kinda sus 😤");
    logger.LogError("error level — big L detected 💀");
    logger.LogCritical("critical level — its giving death ☠️");

    // framework message rewrite works in files too
    logger.LogInformation("Application started. Press Ctrl+C to shut down.");
}

// give the async writer a moment to flush
await Task.Delay(300);

// read it back to flex what we wrote
var resolvedTextPath = Directory.GetFiles(Path.Combine(logDir, "text"), "*.log").FirstOrDefault();
if (resolvedTextPath is not null)
{
    log.LogInformation("wrote to: {Path}", resolvedTextPath);
    log.LogInformation("--- file contents ---");
    foreach (var line in System.IO.File.ReadAllLines(resolvedTextPath).TakeLast(7))
        log.LogDebug("{Line}", line);
    log.LogInformation("---");
}

// === timestamp-first text output (observability style) ===
log.LogInformation("=== text file output (timestamp-first) ===");
var tsFirstLogPath = Path.Combine(logDir, "timestamp-first", "app.log");

using (var factory = LoggerFactory.Create(logging =>
{
    logging.SetMinimumLevel(LogLevel.Trace);
    logging.AddLittyFileLogs(opts =>
    {
        opts.FilePath = tsFirstLogPath;
        opts.OutputFormat = LittyFileOutputFormat.Text;
        opts.RollingInterval = LittyRollingInterval.Daily;
        opts.TimestampFirst = true; // observability style — timestamp leads 📊
    });
}))
{
    var logger = factory.CreateLogger("FileSinkDemo");

    logger.LogInformation("timestamp leads for the sort key besties 📊");
    logger.LogWarning("same vibes different ordering 😤");
    logger.LogError("even Ls look organized with timestamp-first 💀");
}

await Task.Delay(300);

var resolvedTsFirstPath = Directory.GetFiles(Path.Combine(logDir, "timestamp-first"), "*.log").FirstOrDefault();
if (resolvedTsFirstPath is not null)
{
    log.LogInformation("wrote to: {Path}", resolvedTsFirstPath);
    log.LogInformation("--- file contents ---");
    foreach (var line in System.IO.File.ReadAllLines(resolvedTsFirstPath).TakeLast(3))
        log.LogDebug("{Line}", line);
    log.LogInformation("---");
}

// === JSON file output ===
log.LogInformation("=== JSON file output ===");
var jsonLogPath = Path.Combine(logDir, "json", "app.log");

using (var factory = LoggerFactory.Create(logging =>
{
    logging.SetMinimumLevel(LogLevel.Trace);
    logging.AddLittyFileLogs(opts =>
    {
        opts.FilePath = jsonLogPath;
        opts.OutputFormat = LittyFileOutputFormat.Json;
        opts.RollingInterval = LittyRollingInterval.Daily;
    });
}))
{
    var logger = factory.CreateLogger("FileSinkDemo");

    logger.LogInformation("JSON vibes hitting disk 🔥");
    logger.LogWarning("this warning is structured bestie 😤");
    logger.LogError("structured errors for the log aggregator 💀");
}

await Task.Delay(300);

var resolvedJsonPath = Directory.GetFiles(Path.Combine(logDir, "json"), "*.log").FirstOrDefault();
if (resolvedJsonPath is not null)
{
    log.LogInformation("wrote to: {Path}", resolvedJsonPath);
    log.LogInformation("--- file contents ---");
    foreach (var line in System.IO.File.ReadAllLines(resolvedJsonPath).TakeLast(3))
        log.LogDebug("{Line}", line);
    log.LogInformation("---");
}

log.LogInformation("text, timestamp-first, and JSON logs all on disk bestie. the full drip 🔥📁📊");

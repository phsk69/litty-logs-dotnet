using LittyLogs.File;
using Microsoft.Extensions.Logging;

var logDir = Path.Combine(AppContext.BaseDirectory, "logs");

Console.WriteLine("🔥 litty-logs file sink example — logs hitting disk with emojis no cap 📁");
Console.WriteLine();

// === text file output (level-first, RFC 5424 default) ===
Console.WriteLine("=== text file output (level-first) ===");
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
    Console.WriteLine($"  wrote to: {resolvedTextPath}");
    Console.WriteLine("  --- file contents ---");
    foreach (var line in File.ReadAllLines(resolvedTextPath).TakeLast(7))
        Console.WriteLine($"  {line}");
    Console.WriteLine("  ---");
}

Console.WriteLine();

// === timestamp-first text output (observability style) ===
Console.WriteLine("=== text file output (timestamp-first) ===");
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
    Console.WriteLine($"  wrote to: {resolvedTsFirstPath}");
    Console.WriteLine("  --- file contents ---");
    foreach (var line in File.ReadAllLines(resolvedTsFirstPath).TakeLast(3))
        Console.WriteLine($"  {line}");
    Console.WriteLine("  ---");
}

Console.WriteLine();

// === JSON file output ===
Console.WriteLine("=== JSON file output ===");
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
    Console.WriteLine($"  wrote to: {resolvedJsonPath}");
    Console.WriteLine("  --- file contents ---");
    foreach (var line in File.ReadAllLines(resolvedJsonPath).TakeLast(3))
        Console.WriteLine($"  {line}");
    Console.WriteLine("  ---");
}

Console.WriteLine();
Console.WriteLine("text, timestamp-first, and JSON logs all on disk bestie. the full drip 🔥📁📊");

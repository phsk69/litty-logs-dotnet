using LittyLogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// === startup demo — showing both timestamp modes before the service boots 🔥 ===

// level-first (RFC 5424 default)
Console.WriteLine("=== level-first (RFC 5424 default) ===");
Console.WriteLine();

using (var factory = LoggerFactory.Create(logging =>
{
    logging.AddLittyLogs();
}))
{
    var logger = factory.CreateLogger("Demo");
    logger.LogInformation("level comes first, thats RFC 5424 energy 🔥");
    logger.LogWarning("warnings hit different with the emoji prefix 😤");
}

Console.WriteLine();

// timestamp-first (observability style)
Console.WriteLine("=== timestamp-first (observability style) ===");
Console.WriteLine();

using (var factory = LoggerFactory.Create(logging =>
{
    logging.AddLittyLogs(opts => opts.TimestampFirst = true);
}))
{
    var logger = factory.CreateLogger("Demo");
    logger.LogInformation("timestamp leads for the sort key besties 📊");
    logger.LogWarning("same vibes different ordering 😤");
}

Console.WriteLine();
Console.WriteLine("=== hosted service running with default config — vibe checks incoming 🚀 ===");
Console.WriteLine();

// actual hosted service with default litty-logs config
var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        // one line to litty-fy ALL logs in a hosted service no cap 🔥
        logging.AddLittyLogs();
    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<LittyBackgroundService>();
    })
    .Build();

await host.RunAsync();

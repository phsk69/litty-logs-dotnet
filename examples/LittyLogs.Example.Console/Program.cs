using LittyLogs;
using Microsoft.Extensions.Logging;

// the simplest possible litty-logs setup — just create a factory and go 🔥
using var factory = LoggerFactory.Create(logging =>
{
    logging.SetMinimumLevel(LogLevel.Trace);
    logging.AddLittyLogs();
});

var logger = factory.CreateLogger("LittyScript");

// every log level on display so you can see the full emoji spectrum bestie
logger.LogTrace("this is trace level — for when you lowkey wanna see everything 👀");
logger.LogDebug("debug level — investigating whats going on under the hood 🔍");
logger.LogInformation("info level — everything is bussin and vibing fr fr 🔥");
logger.LogWarning("warning level — something kinda sus but we not panicking yet 😤");
logger.LogError("error level — something took a fat L and we gotta deal with it 💀");
logger.LogCritical("critical level — absolute disaster mode, its giving catastrophe ☠️");

logger.LogInformation("and thats the whole spectrum bestie, litty-logs stays winning no cap 💅");

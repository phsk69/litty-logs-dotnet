using LittyLogs;
using Microsoft.Extensions.Logging;

// === startup demo — showing all three modes before the server boots 🔥 ===

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

// JSON mode
Console.WriteLine("=== JSON mode (machines eat good) ===");
Console.WriteLine();

using (var factory = LoggerFactory.Create(logging =>
{
    logging.AddLittyJsonLogs();
}))
{
    var logger = factory.CreateLogger("Demo");
    logger.LogInformation("structured JSON with emojis bestie 🍽️");
}

Console.WriteLine();
Console.WriteLine("=== server running with default config — hit the endpoints bestie 🚀 ===");
Console.WriteLine();

// actual web api server with default litty-logs config
var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddLittyLogs();

var app = builder.Build();

// some endpoints so we can see request logging get litty-fied too
app.MapGet("/", () => "litty-logs is bussin fr fr 🔥");

app.MapGet("/health", () => Results.Ok(new
{
    status = "bussin",
    vibe = "immaculate",
    cap = false
}));

app.MapGet("/yeet", (ILogger<Program> logger) =>
{
    // custom log messages pass through with the litty formatting but no rewriting
    logger.LogInformation("someone hit the /yeet endpoint and thats lowkey iconic 💅");
    return Results.Ok(new { yeeted = true });
});

app.MapGet("/error", (ILogger<Program> logger) =>
{
    // show how errors look with the litty formatter
    logger.LogError("something went wrong but we stay unbothered bestie 💀");
    return Results.StatusCode(500);
});

app.Run();

// needed for integration test access
public partial class Program;

using LittyLogs;

var builder = WebApplication.CreateBuilder(args);

// one line to litty-fy ALL your logs no cap 🔥
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

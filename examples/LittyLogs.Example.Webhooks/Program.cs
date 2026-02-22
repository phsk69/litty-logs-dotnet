using System.Net;
using System.Text;
using System.Text.Json;
using LittyLogs.Webhooks;
using Microsoft.Extensions.Logging;

Console.WriteLine("🪝 litty-logs webhook sink example — yeet logs to chat no cap 🔥");
Console.WriteLine();

// === spin up a mock webhook listener so we can capture payloads without a real matrix server ===
// this is the same vibe as the file sink example — self-contained, no external deps bestie 💅
var webhookUrl = "http://localhost:19380/webhook/";
var capturedPayloads = new List<string>();
using var listener = new HttpListener();
listener.Prefixes.Add(webhookUrl);
listener.Start();
Console.WriteLine($"  mock webhook listener vibing on {webhookUrl} 🎧");
Console.WriteLine();

// handle requests in the background — capture them POST payloads
var listenerTask = Task.Run(async () =>
{
    while (true)
    {
        try
        {
            var ctx = await listener.GetContextAsync();
            if (ctx.Request.HttpMethod == "POST" && ctx.Request.HasEntityBody)
            {
                using var reader = new StreamReader(ctx.Request.InputStream, Encoding.UTF8);
                var body = await reader.ReadToEndAsync();
                capturedPayloads.Add(body);
            }
            ctx.Response.StatusCode = 200;
            ctx.Response.Close();
        }
        catch (HttpListenerException)
        {
            break; // listener stopped, we out 🫡
        }
    }
});

// === demo: webhook sink with all the features ===
// short batch interval for the demo — in prod you'd use the 2s default no cap
Console.WriteLine("=== webhook sink demo (Warning+ to chat, 200ms batch interval) ===");
Console.WriteLine("  logging at all levels but only Warning+ should reach the webhook...");
Console.WriteLine();

using (var factory = LoggerFactory.Create(logging =>
{
    logging.SetMinimumLevel(LogLevel.Trace);
    logging.AddLittyMatrixLogs(webhookUrl, opts =>
    {
        opts.BatchInterval = TimeSpan.FromMilliseconds(200);
        opts.BatchSize = 10;
    });
}))
{
    var logger = factory.CreateLogger("WebhookDemo");

    // these should get FILTERED — below MinimumLevel (Warning)
    logger.LogTrace("this trace wont hit chat — too lowkey 👀");
    logger.LogDebug("this debug stays local — chat dont need this 🔍");
    logger.LogInformation("this info is bussin but chat dont care 🔥");

    // these should REACH the webhook — at or above Warning
    logger.LogWarning("yo something sus just happened bestie 😤");
    logger.LogError("big L detected in the pipeline 💀");
    logger.LogCritical("EVERYTHING IS ON FIRE WE ARE SO COOKED ☠️");

    // framework message rewrite works in webhooks too
    logger.LogWarning("Application is shutting down...");

    // exception with markdown code block
    try
    {
        throw new InvalidOperationException("database connection is mega bricked fr fr");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "caught an L trying to query the database");
    }

    // wait for batches to flush BEFORE factory disposal
    // the async writer batches on interval (200ms) — gotta let it cook
    await Task.Delay(600);
}

// extra wait for the HTTP round-trip to the mock listener
await Task.Delay(200);

Console.WriteLine($"  captured {capturedPayloads.Count} webhook payload(s) 📦");
Console.WriteLine();

// === display the raw JSON hookshot would receive ===
Console.WriteLine("=== raw JSON payload (what matrix hookshot receives) ===");
Console.WriteLine();

for (var i = 0; i < capturedPayloads.Count; i++)
{
    Console.WriteLine($"  --- payload #{i + 1} ---");
    var doc = JsonDocument.Parse(capturedPayloads[i]);
    var prettyJson = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
    foreach (var line in prettyJson.Split('\n'))
        Console.WriteLine($"  {line}");
    Console.WriteLine();
}

// === display what this looks like in chat ===
Console.WriteLine("=== what this looks like in matrix chat ===");
Console.WriteLine();

foreach (var payload in capturedPayloads)
{
    var doc = JsonDocument.Parse(payload);
    if (doc.RootElement.TryGetProperty("text", out var textElement))
    {
        foreach (var line in textElement.GetString()!.Split('\n'))
            Console.WriteLine($"  {line}");
    }
}

Console.WriteLine();

// === demo 2: custom username and timestamp-first ordering ===
Console.WriteLine("=== custom config demo (Trace+, custom username, timestamp-first) ===");
capturedPayloads.Clear();

using (var factory = LoggerFactory.Create(logging =>
{
    logging.SetMinimumLevel(LogLevel.Trace);
    logging.AddLittyMatrixLogs(webhookUrl, opts =>
    {
        opts.MinimumLevel = LogLevel.Trace; // everything goes to chat for this demo
        opts.Username = "LittyBot9000";
        opts.BatchSize = 3;
        opts.BatchInterval = TimeSpan.FromMilliseconds(200);
        opts.TimestampFirst = true; // observability style — timestamp leads
    });
}))
{
    var logger = factory.CreateLogger("CustomDemo");

    logger.LogTrace("peeking at everything with trace 👀");
    logger.LogDebug("investigating the vibes 🔍");
    logger.LogInformation("everything is bussin bestie 🔥");
    logger.LogWarning("batch size is 3 so this starts a new batch 😤");

    // wait for batches to flush
    await Task.Delay(600);
}

await Task.Delay(200);

Console.WriteLine($"  captured {capturedPayloads.Count} payload(s) 📦");
Console.WriteLine();

foreach (var payload in capturedPayloads)
{
    var doc = JsonDocument.Parse(payload);
    Console.WriteLine($"  username: {doc.RootElement.GetProperty("username").GetString()}");
    if (doc.RootElement.TryGetProperty("text", out var textElement))
    {
        Console.WriteLine("  messages:");
        foreach (var line in textElement.GetString()!.Split('\n'))
            Console.WriteLine($"    {line}");
    }
    Console.WriteLine();
}

// === cleanup ===
listener.Stop();
try { await listenerTask; } catch { /* listener cleanup 🫡 */ }

Console.WriteLine("webhook sink demo complete bestie — your logs are hitting chat with full drip 🪝🔥💅");

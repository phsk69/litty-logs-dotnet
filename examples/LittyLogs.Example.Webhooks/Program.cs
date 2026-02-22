using System.Net;
using System.Text;
using System.Text.Json;
using LittyLogs.Webhooks;
using Microsoft.Extensions.Logging;

Console.WriteLine("🪝 litty-logs webhook sink example — yeet logs to chat no cap 🔥");
Console.WriteLine();

// === mode selection — live hookshot or mock listener ===
// set HOOKSHOT_URL env var (or put it in .env) to hit a real Matrix hookshot
// without it we spin up a local mock listener so the example is fully self-contained 💅
var hookshotUrl = Environment.GetEnvironmentVariable("HOOKSHOT_URL");
var liveMode = !string.IsNullOrWhiteSpace(hookshotUrl);

string webhookUrl;
var capturedPayloads = new List<string>();
HttpListener? listener = null;
Task? listenerTask = null;

if (liveMode)
{
    webhookUrl = hookshotUrl!;
    Console.WriteLine("  🟢 LIVE MODE — hitting a real Matrix hookshot, check your room bestie 🔥");
    Console.WriteLine($"  webhook: {webhookUrl[..webhookUrl.LastIndexOf('/')]}/<redacted> 🔒");
    Console.WriteLine();
}
else
{
    // spin up a mock webhook listener so we can capture payloads without a real matrix server
    webhookUrl = "http://localhost:19380/webhook/";
    listener = new HttpListener();
    listener.Prefixes.Add(webhookUrl);
    listener.Start();
    Console.WriteLine("  🟡 MOCK MODE — no HOOKSHOT_URL set, using local mock listener");
    Console.WriteLine($"  set HOOKSHOT_URL in .env to go live with a real hookshot bestie 💅");
    Console.WriteLine($"  mock listener vibing on {webhookUrl} 🎧");
    Console.WriteLine();

    // handle requests in the background — capture them POST payloads
    listenerTask = Task.Run(async () =>
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
}

// === demo 1: webhook sink with all the features ===
// short batch interval for the demo — in prod you'd use the 2s default no cap
Console.WriteLine("=== demo 1: webhook sink (Warning+ to chat, 200ms batch interval) ===");
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

// extra wait for the HTTP round-trip
await Task.Delay(200);

if (liveMode)
{
    Console.WriteLine("  messages yeeted to hookshot — check your Matrix room bestie 🔥");
}
else
{
    Console.WriteLine($"  captured {capturedPayloads.Count} webhook payload(s) 📦");
    Console.WriteLine();

    // display the raw JSON hookshot would receive
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

    // display what this looks like in chat
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
}

Console.WriteLine();

// === demo 2: custom username and timestamp-first ordering ===
Console.WriteLine("=== demo 2: custom config (Trace+, custom username, timestamp-first) ===");
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

if (liveMode)
{
    Console.WriteLine("  all messages yeeted to hookshot with custom username LittyBot9000 🤖");
}
else
{
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
}

// === cleanup ===
if (listener is not null)
{
    listener.Stop();
    try { await listenerTask!; } catch { /* listener cleanup 🫡 */ }
}

Console.WriteLine();
if (liveMode)
    Console.WriteLine("webhook sink demo complete — your logs just hit a REAL Matrix room, go check it out bestie 🪝🔥💅");
else
    Console.WriteLine("webhook sink demo complete bestie — set HOOKSHOT_URL to go live next time 🪝🔥💅");

using System.Net;
using System.Text;
using System.Text.Json;
using LittyLogs;
using LittyLogs.Webhooks;
using Microsoft.Extensions.Logging;

// load .env file if it exists — .NET dont do this natively so we roll our own 💅
// looks for .env in the repo root (walks up from the binary dir)
var dir = Directory.GetCurrentDirectory();
while (dir is not null)
{
    var envFile = Path.Combine(dir, ".env");
    if (File.Exists(envFile))
    {
        foreach (var line in File.ReadAllLines(envFile))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
                continue;
            var eq = trimmed.IndexOf('=');
            if (eq <= 0)
                continue;
            var key = trimmed[..eq].Trim();
            var val = trimmed[(eq + 1)..].Trim();
            Environment.SetEnvironmentVariable(key, val);
        }
        break;
    }
    dir = Directory.GetParent(dir)?.FullName;
}

// meta logger for structural output — we eat our own dogfood bestie 🐕
using var meta = LoggerFactory.Create(l => l.AddLittyLogs());
var log = meta.CreateLogger("WebhookExample");

log.LogInformation("litty-logs webhook sink example — yeet logs to chat no cap 🪝🔥");

// === detect which sinks are live vs mock ===
// set HOOKSHOT_URL and/or TEAMS_WEBHOOK_URL in .env to go live
// without em we spin up a local mock listener — fully self-contained bestie 💅
var hookshotUrl = Environment.GetEnvironmentVariable("HOOKSHOT_URL");
var teamsUrl = Environment.GetEnvironmentVariable("TEAMS_WEBHOOK_URL");
var matrixLive = !string.IsNullOrWhiteSpace(hookshotUrl);
var teamsLive = !string.IsNullOrWhiteSpace(teamsUrl);
var needsMock = !matrixLive || !teamsLive;

// mock listener URL — used for any sink that aint got a real webhook URL
const string mockUrl = "http://localhost:19380/webhook/";
var capturedPayloads = new List<string>();
HttpListener? listener = null;
Task? listenerTask = null;

if (needsMock)
{
    listener = new HttpListener();
    listener.Prefixes.Add(mockUrl);
    listener.Start();

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

// show which sinks are live vs mock
var matrixTarget = matrixLive ? hookshotUrl! : mockUrl;
var teamsTarget = teamsLive ? teamsUrl! : mockUrl;

log.LogInformation("sink status:");
if (matrixLive)
    log.LogInformation("  🟣 Matrix  — LIVE (hookshot) 🔥");
else
    log.LogInformation("  🟣 Matrix  — MOCK (set HOOKSHOT_URL in .env to go live)");
if (teamsLive)
    log.LogInformation("  🟦 Teams   — LIVE (adaptive cards) 🔥");
else
    log.LogInformation("  🟦 Teams   — MOCK (set TEAMS_WEBHOOK_URL in .env to go live)");

// === demo 1: matrix-only sink ===
log.LogInformation("=== demo 1: matrix sink (Warning+, 200ms batch interval) ===");
log.LogInformation("logging at all levels but only Warning+ should reach the webhook...");
capturedPayloads.Clear();

using (var factory = LoggerFactory.Create(logging =>
{
    logging.SetMinimumLevel(LogLevel.Trace);
    logging.AddLittyMatrixLogs(matrixTarget, opts =>
    {
        opts.BatchInterval = TimeSpan.FromMilliseconds(200);
        opts.BatchSize = 10;
    });
}))
{
    var logger = factory.CreateLogger("MatrixDemo");

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

    await Task.Delay(600);
}

await Task.Delay(200);
PrintPayloads(log, "matrix", capturedPayloads, matrixLive);

// === demo 2: teams-only sink ===
log.LogInformation("=== demo 2: teams sink (Warning+, Adaptive Cards v1.5) ===");
log.LogInformation("severity-colored containers — your Teams chat lowkey looks like a dashboard 💅");
capturedPayloads.Clear();

using (var factory = LoggerFactory.Create(logging =>
{
    logging.SetMinimumLevel(LogLevel.Trace);
    logging.AddLittyTeamsLogs(teamsTarget, opts =>
    {
        opts.BatchInterval = TimeSpan.FromMilliseconds(200);
        opts.BatchSize = 10;
    });
}))
{
    var logger = factory.CreateLogger("TeamsDemo");

    logger.LogWarning("yo something sus just happened bestie 😤");
    logger.LogError("big L detected in the pipeline 💀");
    logger.LogCritical("EVERYTHING IS ON FIRE WE ARE SO COOKED ☠️");

    try
    {
        throw new InvalidOperationException("database connection is mega bricked fr fr");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "caught an L trying to query the database");
    }

    await Task.Delay(600);
}

await Task.Delay(200);
PrintPayloads(log, "teams", capturedPayloads, teamsLive);

// === demo 3: DUAL MODE — both sinks firing simultaneously 🔥🔥🔥 ===
log.LogInformation("=== demo 3: DUAL MODE — matrix + teams at the same time bestie 💅 ===");
log.LogInformation("separate providers, separate channels, zero interference no cap");
capturedPayloads.Clear();

using (var factory = LoggerFactory.Create(logging =>
{
    logging.SetMinimumLevel(LogLevel.Trace);

    // matrix sink — its own provider, its own channel, its own HTTP client 🟣
    logging.AddLittyMatrixLogs(matrixTarget, opts =>
    {
        opts.MinimumLevel = LogLevel.Warning;
        opts.BatchInterval = TimeSpan.FromMilliseconds(200);
        opts.BatchSize = 10;
        opts.Username = "MatrixBot";
    });

    // teams sink — totally independent, vibing on its own 🟦
    logging.AddLittyTeamsLogs(teamsTarget, opts =>
    {
        opts.MinimumLevel = LogLevel.Error; // only big Ls go to teams
        opts.BatchInterval = TimeSpan.FromMilliseconds(200);
        opts.BatchSize = 10;
        opts.Username = "TeamsBot";
    });
}))
{
    var logger = factory.CreateLogger("DualDemo");

    // Warning → Matrix only (Teams min is Error)
    logger.LogWarning("this warning hits Matrix but not Teams — different thresholds bestie 😤");
    // Error → BOTH sinks
    logger.LogError("this error hits BOTH sinks simultaneously no cap 💀");

    try
    {
        throw new InvalidOperationException("dual webhook mode is bussin fr fr");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "critical with exception — both sinks get this");
    }

    await Task.Delay(600);
}

await Task.Delay(200);
PrintPayloads(log, "dual (matrix + teams)", capturedPayloads, matrixLive && teamsLive);

// === cleanup ===
if (listener is not null)
{
    listener.Stop();
    try { await listenerTask!; } catch { /* listener cleanup 🫡 */ }
}

var liveCount = (matrixLive ? 1 : 0) + (teamsLive ? 1 : 0);
log.LogInformation(liveCount switch
{
    2 => "webhook sink demo complete — both Matrix AND Teams went live, go check your rooms bestie 🪝🟣🟦🔥",
    1 => "webhook sink demo complete — one sink went live, set both URLs in .env for full dual mode bestie 🪝🔥",
    _ => "webhook sink demo complete — set HOOKSHOT_URL and/or TEAMS_WEBHOOK_URL in .env to go live bestie 🪝🔥💅"
});

// === helper to display captured payloads or tell user to check their chat ===
void PrintPayloads(ILogger metaLog, string sinkName, List<string> payloads, bool isLive)
{
    if (isLive)
    {
        metaLog.LogInformation("messages yeeted to {SinkName} — check your chat bestie 🔥", sinkName);
        return;
    }

    metaLog.LogInformation("captured {Count} {SinkName} payload(s) 📦", payloads.Count, sinkName);

    var prettyOptions = new JsonSerializerOptions { WriteIndented = true };
    for (var i = 0; i < payloads.Count; i++)
    {
        metaLog.LogInformation("--- {SinkName} payload #{Number} ---", sinkName, i + 1);
        var doc = JsonDocument.Parse(payloads[i]);
        var prettyJson = JsonSerializer.Serialize(doc, prettyOptions);
        foreach (var line in prettyJson.Split('\n'))
            metaLog.LogDebug("{Line}", line);
    }
}

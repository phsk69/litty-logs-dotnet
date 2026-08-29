using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using LittyLogs;
using LittyLogs.Webhooks;
using Microsoft.Extensions.Logging;

// load the nearest .env because .NET leaves that vibe to us bestie 💅🔥
var directory = Directory.GetCurrentDirectory();
while (directory is not null)
{
    var envFile = Path.Combine(directory, ".env");
    if (File.Exists(envFile))
    {
        foreach (var line in File.ReadAllLines(envFile))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
                continue;

            var separator = trimmed.IndexOf('=');
            if (separator <= 0)
                continue;

            Environment.SetEnvironmentVariable(
                trimmed[..separator].Trim(),
                trimmed[(separator + 1)..].Trim());
        }
        break;
    }

    directory = Directory.GetParent(directory)?.FullName;
}

using var meta = LoggerFactory.Create(logging => logging.AddLittyLogs());
var log = meta.CreateLogger("WebhookExample");
log.LogInformation("litty webhook demo is serving Matrix + Slack dual-sink rizz 🪝🔥");

var hookshotUrl = Environment.GetEnvironmentVariable("HOOKSHOT_URL");
var slackUrl = Environment.GetEnvironmentVariable("SLACK_WEBHOOK_URL");
var matrixLive = !string.IsNullOrWhiteSpace(hookshotUrl);
var slackLive = !string.IsNullOrWhiteSpace(slackUrl);
var needsMock = !matrixLive || !slackLive;

const string mockUrl = "http://localhost:19380/webhook/";
var capturedPayloads = new ConcurrentQueue<string>();
HttpListener? listener = null;
Task? listenerTask = null;

if (needsMock)
{
    listener = new HttpListener();
    listener.Prefixes.Add(mockUrl);
    listener.Start();
    listenerTask = Task.Run(async () =>
    {
        while (true)
        {
            try
            {
                var context = await listener.GetContextAsync();
                if (context.Request.HttpMethod == "POST" && context.Request.HasEntityBody)
                {
                    using var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
                    capturedPayloads.Enqueue(await reader.ReadToEndAsync());
                }

                context.Response.StatusCode = 200;
                context.Response.Close();
            }
            catch (HttpListenerException)
            {
                break; // mock listener clocked out cleanly bestie 🫡🔥
            }
        }
    });
}

var matrixTarget = matrixLive ? hookshotUrl! : mockUrl;
var slackTarget = slackLive ? slackUrl! : mockUrl;

log.LogInformation("Matrix is {Mode} 🟣🔥", matrixLive ? "LIVE" : "MOCK — set HOOKSHOT_URL to go live");
log.LogInformation("Slack is {Mode} 🟢🔥", slackLive ? "LIVE" : "MOCK — set SLACK_WEBHOOK_URL to go live");

// demo 1: Matrix hookshot keeps its existing safe HTML + text fallback energy 🟣🔥
log.LogInformation("demo 1: Matrix Warning+ sink is cooking 🟣🔥");
Clear(capturedPayloads);
using (var factory = LoggerFactory.Create(logging =>
{
    logging.SetMinimumLevel(LogLevel.Trace);
    logging.AddLittyMatrixLogs(matrixTarget, options =>
    {
        options.BatchInterval = TimeSpan.FromMilliseconds(200);
        options.BatchSize = 10;
    });
}))
{
    var logger = factory.CreateLogger("MatrixDemo");
    logger.LogInformation("this stays local because Warning is the chat floor 🔍🔥");
    logger.LogWarning("Matrix caught a suspicious vibe 😤🔥");
    logger.LogError("Matrix caught a certified L 💀🔥");
    await Task.Delay(600);
}
await Task.Delay(200);
PrintPayloads(log, "matrix", capturedPayloads, matrixLive);

// demo 2: Slack uses safe plain-text Block Kit, so user-shaped markdown stays literal 🟢🔥
log.LogInformation("demo 2: Slack plain-text Block Kit sink is cooking 🟢🔥");
Clear(capturedPayloads);
using (var factory = LoggerFactory.Create(logging =>
{
    logging.SetMinimumLevel(LogLevel.Trace);
    logging.AddLittySlackLogs(slackTarget, options =>
    {
        options.BatchInterval = TimeSpan.FromMilliseconds(200);
        options.BatchSize = 10;
        options.Username = "Deploy Alerts";
    });
}))
{
    var logger = factory.CreateLogger("SlackDemo");
    logger.LogWarning("Slack sees this as literal plain text: <!channel> *not bold* 🔒🔥");
    try
    {
        throw new InvalidOperationException("database connection is mega bricked fr fr");
    }
    catch (Exception exception)
    {
        logger.LogError(exception, "Slack gets the stack trace without markdown fences 💀🔥");
    }
    await Task.Delay(600);
}
await Task.Delay(200);
PrintPayloads(log, "slack", capturedPayloads, slackLive);

// demo 3: generic providers can run together with separate thresholds and channels 🪝🔥
log.LogInformation("demo 3: Matrix + Slack dual mode has entered the chat 🟣🟢🔥");
Clear(capturedPayloads);
using (var factory = LoggerFactory.Create(logging =>
{
    logging.SetMinimumLevel(LogLevel.Trace);
    logging.AddLittyMatrixLogs(matrixTarget, options =>
    {
        options.MinimumLevel = LogLevel.Warning;
        options.BatchInterval = TimeSpan.FromMilliseconds(200);
        options.Username = "Matrix Alerts";
    });
    logging.AddLittySlackLogs(slackTarget, options =>
    {
        options.MinimumLevel = LogLevel.Error;
        options.BatchInterval = TimeSpan.FromMilliseconds(200);
        options.Username = "Slack Alerts";
    });
}))
{
    var logger = factory.CreateLogger("DualDemo");
    logger.LogWarning("this warning only reaches the Matrix threshold 😤🔥");
    logger.LogError("this error hits both independent webhook providers 💀🔥");
    await Task.Delay(600);
}
await Task.Delay(200);
PrintPayloads(log, "dual", capturedPayloads, matrixLive && slackLive);

if (listener is not null)
{
    listener.Stop();
    try
    {
        await listenerTask!;
    }
    catch
    {
        // mock cleanup catching a shutdown race is totally chill 🫡🔥
    }
}

var liveCount = (matrixLive ? 1 : 0) + (slackLive ? 1 : 0);
log.LogInformation(liveCount switch
{
    2 => "both live sinks got the payloads, go check the rooms bestie 🪝🔥",
    1 => "one live sink got the goods; set both URLs for maximum dual-mode rizz 🪝🔥",
    _ => "mock demo ate; set HOOKSHOT_URL and SLACK_WEBHOOK_URL when live chat is the vibe 🪝🔥"
});

static void Clear(ConcurrentQueue<string> payloads)
{
    while (payloads.TryDequeue(out _))
    {
        // dequeue until the next demo has a clean slate 🧹🔥
    }
}

static void PrintPayloads(
    ILogger logger,
    string sinkName,
    ConcurrentQueue<string> payloads,
    bool isLive)
{
    if (isLive)
    {
        logger.LogInformation("payloads yeeted to live {SinkName}, check the chat bestie 🔥", sinkName);
        return;
    }

    var snapshot = payloads.ToArray();
    logger.LogInformation("captured {Count} {SinkName} payloads in mock mode 📦🔥", snapshot.Length, sinkName);
    var prettyOptions = new JsonSerializerOptions { WriteIndented = true };
    for (var index = 0; index < snapshot.Length; index++)
    {
        using var document = JsonDocument.Parse(snapshot[index]);
        var prettyJson = JsonSerializer.Serialize(document, prettyOptions);
        logger.LogDebug("{SinkName} payload #{Number}:\n{Payload}", sinkName, index + 1, prettyJson);
    }
}

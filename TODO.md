# the litty-logs glow up list 🔥✨

stuff thats coming, stuff we're manifesting, and stuff that would go crazy if someone built it fr fr

---

## up next — webhook sink (`LittyLogs.Webhooks`) 🪝

yeet your logs straight to where the squad is. critical error? dont wait for someone to open grafana — it lands in the chat room formatted all nice

### Matrix (hookshot) — first priority 🟣

- `AddLittyMatrixLogs("https://hookshot.example.com/webhook/abc123")` — one liner
- hookshot webhook format with markdown formatting
- emoji + level + category + message, exceptions in code blocks
- configurable `MinimumLevel` (default: `Warning`) so chat dont get spammed

### Teams (Adaptive Cards) — second priority 🟦

- `AddLittyTeamsLogs("https://outlook.office.com/webhook/...")` — one liner
- Adaptive Card JSON with colored containers per severity
- same level filtering and formatting as Matrix

### architecture

- new package: `LittyLogs.Webhooks`
- follows the file sink pattern: `ILoggerProvider` + `ILogger` + async `Channel<T>` writer
- `IHttpClientFactory` with named client — proper socket management no cap
- `Microsoft.Extensions.Http.Resilience` (Polly) — retry with exponential backoff, circuit breaker, per-request timeout
- batching: groups messages by interval (2s default) or batch size (10 default) to avoid spamming
- best-effort delivery — if webhook is bricked after retries, drop the message, dont crash the app
- platform-specific payload formatters behind `IWebhookPayloadFormatter` interface

### project structure

```
src/LittyLogs.Webhooks/
├── LittyWebhookProvider.cs       — ILoggerProvider
├── LittyWebhookLogger.cs         — ILogger with min level filtering
├── LittyWebhookWriter.cs         — async Channel + HttpClient + batching
├── LittyWebhookOptions.cs        — url, platform, min level, batch config
├── LittyWebhookExtensions.cs     — AddLittyMatrixLogs() / AddLittyTeamsLogs()
├── WebhookPlatform.cs            — enum: Matrix, Teams
└── Formatters/
    ├── IWebhookPayloadFormatter.cs
    ├── MatrixPayloadFormatter.cs  — hookshot JSON + markdown
    └── TeamsPayloadFormatter.cs   — Adaptive Card JSON
```

---

## manifesting these features 🧠✨

stuff that would go absolutely crazy but aint started yet. vibes only rn

- 💬 **Slack webhook sink** — Block Kit formatter, similar pattern to Matrix/Teams
- 🟣 **Matrix Client-Server API** — direct room messages with access token + room ID for power users who want full HTML control instead of hookshot
- 🎨 **custom webhook templates** — user-defined message format strings so you can make it look however you want
- 🗜️ **zstd compression** — for file sink rotation (gzip is cool but zstd is faster and smaller)
- 📊 **structured log enrichment** — auto-attach machine name, environment, correlation IDs to webhook messages

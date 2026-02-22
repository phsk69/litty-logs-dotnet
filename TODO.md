# the litty-logs glow up list 🔥✨

stuff thats coming, stuff we're manifesting, and stuff that would go crazy if someone built it fr fr

---

## shipped — webhook sink (`LittyLogs.Webhooks`) 🪝✅

Matrix hookshot support is LIVE. Teams stub is ready to cook when we get to it

### whats in the package rn
- `AddLittyMatrixLogs(url)` — one liner to yeet logs to Matrix via hookshot
- `AddLittyMatrixLogs(url, opts => ...)` — full control over MinimumLevel, batch config, username
- `IWebhookPayloadFormatter` interface for platform-specific formatters
- `MatrixPayloadFormatter` — hookshot JSON with markdown, emojis survive serialization
- `TeamsPayloadFormatter` — stub ready for Adaptive Cards implementation
- async `Channel<T>` batching (2s interval / 10 messages)
- `IHttpClientFactory` + Polly resilience (retry, circuit breaker, timeout)
- best-effort delivery — never crashes your app over a failed webhook

---

## up next — teams adaptive cards 🟦

- implement `TeamsPayloadFormatter` with colored containers per severity
- `AddLittyTeamsLogs("https://outlook.office.com/webhook/...")` already wired up, just needs the formatter
- same architecture as Matrix, just different payload JSON

---

## manifesting these features 🧠✨

stuff that would go absolutely crazy but aint started yet. vibes only rn

- 💬 **Slack webhook sink** — Block Kit formatter, similar pattern to Matrix/Teams
- 🟣 **Matrix Client-Server API** — direct room messages with access token + room ID for power users who want full HTML control instead of hookshot
- 🎨 **custom webhook templates** — user-defined message format strings so you can make it look however you want
- 🗜️ **zstd compression** — for file sink rotation (gzip is cool but zstd is faster and smaller)
- 📊 **structured log enrichment** — auto-attach machine name, environment, correlation IDs to webhook messages

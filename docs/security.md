# security — litty-logs keeps it locked down bestie 🔒🔥

litty-logs is a logging library so it touches your log messages, webhook URLs, file paths, and terminal output. heres how we keep all that secure no cap

## whats protected

### webhook URL validation (SSRF prevention) 🔒

webhook URLs get validated at registration time — not at 3am when a batch tries to POST and you find out your config was bricked:

- empty/whitespace URLs? instant `ArgumentException`, thats not it
- `Uri.TryCreate()` must succeed with `UriKind.Absolute`
- scheme MUST be `http` or `https` — `file://`, `ftp://`, `gopher://` and other sketchy schemes get yeeted immediately
- this blocks SSRF attempts where a misconfigured URL could make your app hit internal endpoints or exfiltrate data

see: `LittyWebhookExtensions.cs` — validation happens in `AddLittyWebhookLogs()` before any services are registered

### log injection prevention 🔒

newlines in log messages could create fake log entries in text output (JSON format is already safe because `Utf8JsonWriter` escapes everything):

- `\r\n`, `\n`, and `\r` in message text get replaced with spaces
- fast path — if theres no newlines, zero extra allocations (the common case)
- exception stack traces are intentionally multiline (they get their own section after the log line)
- this blocks log injection attacks where an attacker could craft a message like `legit msg\n[🔥 info] [timestamp] [FakeCategory] injected entry` to create fake log entries

see: `LittyLogsFormatHelper.cs` — `FormatLogLine()` sanitizes the message before appending

### content injection prevention (webhook sink) 🔒

webhook messages get rendered by Matrix hookshot or Slack. malicious log messages could embed:

- **XSS**: `<script>alert('pwned')</script>` — could execute JavaScript in web-based clients
- **tracking pixels**: `![pixel](https://evil.com/track.png)` — renders as an invisible image, leaks IP addresses
- **phishing links**: `[click here](https://evil.com/steal-creds)` — renders as a clickable link
- **formatting spam**: `# HUGE TEXT` or `**bold spam**` — disrupts the chat channel

litty-logs sends an `html` field in the webhook payload (hookshot prefers it over `text` for rendering). all message content goes through the tiny `HtmlEscape()` boundary that encodes `<`, `>`, `&`, `"`, and `'` while preserving literal UTF-8 emojis. content renders as text instead of executable HTML or markdown, and exception stack traces get wrapped in `<pre><code>` blocks for proper monospace energy 🔒🔥

the `text` field (markdown fallback for clients without HTML support) uses `\n\n` paragraph breaks between messages so they render as separate blocks per the CommonMark spec

see: `Formatters/MatrixPayloadFormatter.cs` — `MessageToHtml()` method uses `HtmlEscape()`

#### Slack Block Kit — plain text stays literal 🟢🔒🔥

Slack payloads use a top-level fallback with `mrkdwn: false` plus Block Kit `plain_text` objects for both the header and every log section. user-shaped markup never becomes active formatting no cap:

- no XSS risk — `<script>alert('pwned')</script>` renders as literal text, not executable HTML
- no tracking pixels — `![pixel](url)` renders as literal text, not an image
- no phishing links — `[click here](url)` renders as literal text, not a clickable link
- surprise mentions like `<!channel>` stay visible text instead of pinging the room
- `Utf8JsonWriter` handles JSON serialization safely while emojis stay literal UTF-8 🔥
- exact exception fences added by the shared logger are removed before plain-text delivery, while the multiline exception content survives
- Unicode-scalar truncation respects Slack's 150-character header, 3000-character section, and 50-block message limits 🔒

the incoming webhook URL is the Slack credential. keep it in secret storage, never commit it, and remember `Username` only labels the message header — Slack controls the installed app identity. use the [Slack incoming webhook setup](slack-webhook-setup.md) for a safe local test-channel flow 🔒🔥

see: `Formatters/SlackPayloadFormatter.cs` — safe plain-text Block Kit without a Slack SDK dependency 🔥

### HTTP category filtering (infinite loop + URL exposure prevention) 🔒

when your app uses the webhook sink, the HTTP client that sends webhook requests generates its own log entries (`System.Net.Http`, `Microsoft.Extensions.Http`, `Polly`). without filtering:

1. HTTP client logs a trace message about the webhook request
2. that message gets sent to... the webhook
3. which triggers another HTTP request, which generates another log
4. infinite recursion until everything is cooked 💀

plus those HTTP client logs might contain the webhook URL (which has your secret token in the path). thats not it

litty-logs routes all HTTP-related categories to `NullLogger.Instance` in the webhook provider:
- `System.Net.Http.*`
- `Microsoft.Extensions.Http.*`
- `Polly.*`

this prevents both the infinite loop AND accidental token exposure no cap

see: `LittyWebhookProvider.cs` — `IsHttpClientCategory()` check in `CreateLogger()`

**heads up**: if you add OTHER logging providers alongside the webhook sink (like a file sink), those providers will still see HTTP client logs. filter those categories in your logging config if you dont want webhook URLs showing up in file logs

## trust model

litty-logs treats these as developer-controlled configuration, same trust model as Serilog and other .NET logging libraries:

- **webhook URLs** — set by the developer in code or app config. validated for scheme and format but not for destination (your app might legit need to hit internal URLs). if your config source is untrusted thats a you problem bestie
- **file sink paths** — `FilePath` is developer config. no path traversal protection because the developer controls where logs go, same as `Serilog.Sinks.File`
- **log directory** — file rotation uses `File.Move()` and `File.Delete()` without symlink checks. if an attacker has filesystem access to your log directory you got bigger problems fr fr

## security reporting

found a vulnerability? dont yeet it in a public issue bestie. reach out privately:

- open a [security advisory](https://github.com/phsk69/litty-logs-dotnet/security/advisories/new) on GitHub
- or email the maintainer directly

we'll get back to you faster than a Polly retry with zero backoff no cap 🔒

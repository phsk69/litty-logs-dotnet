# litty-logs 🔥

yo your .NET logs are giving corporate dystopia energy rn and thats not it bestie. litty-logs fully rewrites all them boring built-in framework messages into gen alpha slang while also blessing your terminal with emojis and ANSI colors no cap

## before (deadass boring) 💀

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /app
```

## after (absolutely bussin) 🔥

```
[🔥 info] [2026-02-18T21:45:00.420Z] [Lifetime] we vibing on http://localhost:5000 fr fr 🎧
[🔥 info] [2026-02-18T21:45:00.421Z] [Lifetime] app is bussin and ready to slay bestie 💅 yeet Ctrl+C to dip out no cap
[🔥 info] [2026-02-18T21:45:00.421Z] [Lifetime] content root living at /app bestie 📁
```

## installation

```bash
dotnet add package LittyLogs

# for xUnit test output (optional, separate package)
dotnet add package LittyLogs.Xunit

# for file sink with rotation and gzip compression (optional, separate package)
dotnet add package LittyLogs.File

# for webhook sink — yeet logs to Matrix, Teams, etc (optional, separate package)
dotnet add package LittyLogs.Webhooks

# for the CLI tool that litty-fies build, test, publish, and pack output
dotnet tool install --global LittyLogs.Tool
```

## usage — one line thats it thats the tweet

### web api

```csharp
using LittyLogs;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddLittyLogs(); // thats it bestie 🔥
var app = builder.Build();
app.Run();
```

### hosted service / background worker

```csharp
using LittyLogs;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging => logging.AddLittyLogs())
    .ConfigureServices(services => services.AddHostedService<MyService>())
    .Build();

await host.RunAsync();
```

### console script (the ten-liner speedrun)

```csharp
using LittyLogs;
using Microsoft.Extensions.Logging;

using var factory = LoggerFactory.Create(logging =>
{
    logging.SetMinimumLevel(LogLevel.Trace);
    logging.AddLittyLogs();
});

var logger = factory.CreateLogger("MyScript");
logger.LogInformation("we in here bestie 🔥");
```

### xUnit tests

```csharp
using LittyLogs.Xunit;
using Xunit;
using Xunit.Abstractions;

public class MyTests
{
    private readonly ILogger<MyTests> _logger;

    public MyTests(ITestOutputHelper output)
    {
        // one line to litty-fy your test output bestie 💅
        _logger = output.CreateLittyLogger<MyTests>();
    }

    [Fact]
    public void MyTest()
    {
        _logger.LogInformation("this shows up litty-fied in test output 🔥");
    }
}
```

### JSON structured logging — for when machines need to eat too 🍽️

same litty rewrites and emojis, but as valid JSON. your log aggregator is gonna love this no cap

```csharp
using LittyLogs;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddLittyJsonLogs(); // structured JSON with emojis bestie 🔥
var app = builder.Build();
app.Run();
```

output:
```json
{"timestamp":"2026-02-19T10:45:00.420Z","level":"info","emoji":"🔥","category":"Lifetime","message":"app is bussin and ready to slay bestie 💅 yeet Ctrl+C to dip out no cap"}
```

emojis in JSON? absolutely bussin — JSON is UTF-8 native so every parser on earth handles it perfectly 🏆

### file sink — yeet logs to disk with rotation and compression 📁

```csharp
using LittyLogs.File;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddLittyFileLogs(opts =>
{
    opts.FilePath = "logs/app.log";
    opts.OutputFormat = LittyFileOutputFormat.Text;  // or Json for structured output
    opts.RollingInterval = LittyRollingInterval.Daily;
    opts.MaxFileSizeBytes = 10 * 1024 * 1024;        // 10MB then rotate
    opts.CompressionMode = LittyCompressionMode.Gzip; // compress rotated files 🗜️
});
var app = builder.Build();
app.Run();
```

features that go hard:
- **async I/O** — `Channel<string>` based, your app thread never blocks on disk writes 👑
- **text or JSON** — human-readable or machine-parseable, your choice bestie
- **size + time rotation** — daily, hourly, or size-based. rotated files get timestamps in the name
- **gzip compression** — old rotated files auto-compress to `.gz`, active file stays uncompressed
- **startup safeguard** — never auto-rotates on startup, only rotates before writing the next entry 🔒
- **no ANSI codes** — files never get terminal escape chars, thats cursed 💀

### webhook sink — yeet logs to Matrix chat 🪝

critical error hits? your group chat knows about it instantly, formatted all nice with emojis

```csharp
using LittyLogs.Webhooks;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddLittyMatrixLogs("https://hookshot.example.com/webhook/abc123"); // one liner bestie 🟣
var app = builder.Build();
app.Run();
```

with full options:
```csharp
builder.Logging.AddLittyMatrixLogs("https://hookshot.example.com/webhook/abc123", opts =>
{
    opts.MinimumLevel = LogLevel.Warning;   // only Warning+ goes to chat (default)
    opts.Username = "LittyLogs";            // bot display name in chat
    opts.BatchSize = 10;                    // max messages per batch
    opts.BatchInterval = TimeSpan.FromSeconds(2); // flush interval
});
```

features that go hard:
- **async batching** — `Channel<T>` based, groups messages by interval (2s) or count (10), your app thread never blocks 👑
- **Polly resilience** — retry with exponential backoff, circuit breaker, per-request timeout via `Microsoft.Extensions.Http.Resilience` 🔒
- **best-effort** — if the webhook is bricked after retries, we drop the batch and keep vibing. never crashes your app no cap
- **min level filtering** — default `Warning` so your chat dont get spammed with trace logs 💀
- **IHttpClientFactory** — proper socket management, named client `"LittyWebhooks"` for custom config
- **Matrix hookshot format** — markdown messages with emojis, exceptions in code blocks

## what gets litty-fied

all the boring framework messages you see every `dotnet run`:

| boring version 💀 | litty version 🔥 |
|---|---|
| Application started. Press Ctrl+C to shut down. | app is bussin and ready to slay bestie 💅 yeet Ctrl+C to dip out no cap |
| Now listening on: {url} | we vibing on {url} fr fr 🎧 |
| Content root path: {path} | content root living at {path} bestie 📁 |
| Hosting environment: {env} | we in our {env} era rn ✨ |
| Application is shutting down... | app said aight imma head out 💀 |
| Request starting {details} | yo a request just slid in: {details} 👀 |
| Request finished {details} | request finished cooking: {details} 🍳 |

plus hosting lifecycle, endpoint routing, and more fr fr

## log level emojis

| level | emoji | vibe |
|---|---|---|
| Trace | 👀 | lowkey peeking |
| Debug | 🔍 | investigating bestie |
| Information | 🔥 | bussin as usual |
| Warning | 😤 | not it |
| Error | 💀 | big L |
| Critical | ☠️ | its giving death |

## options — configure the vibe

```csharp
builder.Logging.AddLittyLogs(options =>
{
    options.RewriteMessages = true;     // rewrite framework messages (default: true, thats the whole point)
    options.UseColors = true;           // ANSI colors (default: true)
    options.ShortenCategories = true;   // yeet namespace bloat (default: true)
    options.UseUtcTimestamp = true;     // UTC timestamps (default: true, international rizz)
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffK"; // ISO 8601 with milliseconds (default)
    options.TimestampFirst = false;     // false = RFC 5424 (level first), true = observability style (timestamp first)
});
```

## `dotnet litty` CLI tool — litty-fy your build, test, publish, and pack output 🧪

your app logs are litty but `dotnet build`, `dotnet test`, `dotnet publish`, and `dotnet pack` output is still giving corporate energy? install the tool and never look at boring terminal output again no cap

```bash
# install the tool
dotnet tool install --global LittyLogs.Tool

# litty-fy your test output (auto-shows ITestOutputHelper output too)
dotnet litty test

# litty-fy your build output
dotnet litty build

# litty-fy your publish output
dotnet litty publish

# litty-fy your pack output — nupkgs go brrr 📦
dotnet litty pack

# all args pass through to the underlying dotnet command
dotnet litty test --filter "FullyQualifiedName~MyTests"
dotnet litty build -c Release
dotnet litty publish -c Release --self-contained
dotnet litty pack -c Release
```

### before (boring test output) 💀

```
Passed!  - Failed:     0, Passed:    66, Skipped:     0, Total:    66, Duration: 80 ms - LittyLogs.Tests.dll (net10.0)
```

### after (bussin test output) 🔥

```
  [xUnit.net] scouting for tests in LittyLogs.Tests 🔍
  [xUnit.net] found the squad in LittyLogs.Tests 📋
  [xUnit.net] lets gooo LittyLogs.Tests is cooking 🔥
  ✅ LittyLogs.Tests.MyTest.SomeTest [26 ms]
  [xUnit.net] LittyLogs.Tests absolutely ate no crumbs 💅

  all tests ate and left no crumbs 🏆
  total vibes checked: 66
  ate: 66 ✅
  cooked in 0.5 Seconds ⏱️
```

## your own logs stay untouched

litty-logs only rewrites known framework messages. your custom log messages pass through with the bussin formatting (emojis, colors, short categories) but the actual message text stays exactly how you wrote it no cap

```csharp
logger.LogInformation("my custom message stays exactly like this");
// output: [🔥 info] [2026-02-18T21:45:00.420Z] [MyService] my custom message stays exactly like this
```

## examples

seven example projects in `examples/` so you can see litty-logs in every scenario:

| example | what it shows | run it |
|---|---|---|
| `WebApi` | startup demo with level-first → timestamp-first → JSON, then server runs | `just example web` |
| `HostedService` | startup demo with both timestamp modes, then background service vibes | `just example hosted` |
| `Console` | side-by-side text + JSON output comparison | `just example console` |
| `Xunit` | litty-fied xUnit test output with all log levels + TimestampFirst test | `just example xunit` |
| `Json` | structured JSON logging with both timestamp configs | `just example json` |
| `FileSink` | file sink with level-first → timestamp-first → JSON, reads em all back | `just example filesink` |
| `Webhooks` | webhook sink with mock listener or live hookshot — set `HOOKSHOT_URL` to go live | `just example webhooks` |

every example auto-showcases ALL the modes when you run it — no hidden flags, no secret handshakes. you run it, you see everything 💅

the webhooks example has a special trick tho — set `HOOKSHOT_URL` env var (or put it in `.env`) and it hits a real Matrix hookshot instead of the mock listener. logs actually land in your room bestie 🪝🔥

## development — for the contributing besties 🛠️

### just recipes

this project uses [just](https://just.systems) as the task runner. here are the vibes:

| recipe | what it does |
|---|---|
| `just build` | build the whole solution |
| `just test` | run all tests |
| `just litty-build` | build with litty-fied output 🔥 |
| `just litty-test` | test with litty-fied output 🔥 |
| `just litty-publish` | publish with litty-fied output 📤 |
| `just litty-pack` | pack with litty-fied output 📦 |
| `just pack` | pack all five NuGet packages |
| `just clean` | yeet all build artifacts |
| `just bump patch` | bump the patch version (also: `minor`, `major`) |
| `just bump-pre dev.1` | slap a pre-release label on (e.g. `0.1.0-dev.1`) |
| `just release patch` | full gitflow release — bump, branch, finish, push 🚀 |
| `just release-current` | gitflow release without bumping (for first release etc.) |
| `just re-release` | nuke old releases + tags everywhere, re-do the current version 🔄 |
| `just release-dev patch` | dev/pre-release — bump + label + ship (e.g. `0.1.1-dev`) 🧪 |
| `just hotfix patch` | start a gitflow hotfix branch off main 🚑 |
| `just finish` | finish whatever gitflow branch youre on (hotfix/release/support) + push 🏁 |
| `just nuget-push` | manually push packages to nuget.org |
| `just example <name>` | run an example — `web`, `hosted`, `console`, `xunit`, `json`, `filesink`, `webhooks` 🔥 |
| `just setup-completions` | install shell tab-completions for `just example <tab>` |

### shell completions

tab-complete `just example <tab>` to see all available examples. works with zsh and bash:

```bash
# auto-install to your shell rc file
just setup-completions

# or source manually
source completions/just.zsh   # zsh
source completions/just.bash  # bash
```

### versioning

version lives in one place: `Directory.Build.props`. all five packages inherit from it. we use [gitflow](https://nvie.com/posts/a-successful-git-branching-model/) via the `git flow` CLI — `main` is production, `develop` is the integration branch, releases and hotfixes get their own branches 🔥

### release flow (gitflow)

```bash
# from develop — full gitflow ceremony (bump, branch, finish, push — all in one command)
just release patch    # 0.1.0 → 0.1.1, pushes everything, pipeline goes brrr
just release minor    # 0.1.0 → 0.2.0
just release major    # 0.1.0 → 1.0.0

# dev/pre-release for testing the pipeline
just release-dev patch         # 0.1.0 → 0.1.1-dev
just release-dev minor beta.1  # 0.1.0 → 0.2.0-beta.1

# release the current version without bumping (e.g. first release)
just release-current
```

all release commands auto-push develop + main + tag to origin when done. no manual `git push` needed fr fr 🔥

### hotfix flow

```bash
# from main — start a hotfix when something is bricked in prod
just hotfix patch

# make your fix, commit it, then finish + push
just finish
```

`just finish` auto-detects if youre on a hotfix, release, or support branch, does `git flow finish`, and pushes everything. one command to rule them all 🏁

### CI/CD — triple release pipeline 🚀

forgejo actions on a self-hosted runner handles the whole squad:

- **CI** (`ci.yml`) — builds, tests (with litty output 🔥), and packs on every push/PR to `develop` and `main`. if this fails your code is bricked and you should not merge no cap
- **Release** (`release.yml`) — triggered by `v*` tags. the full pipeline hits THREE destinations:
  1. **nuget.org** — all five `.nupkg` files with `--skip-duplicate` so retries dont catch Ls
  2. **forgejo releases** — via Gitea API with `.nupkg` assets attached 🏠
  3. **github mirror releases** — via `gh` CLI with `.nupkg` assets on the [mirror repo](https://github.com/phsk69/litty-logs-dotnet/releases) 🐙

pipeline features that go hard:
- **fully retryable** — every step checks if work is already done before doing it again. re-run from the forgejo UI all day, zero errors 🔄
- **pre-release auto-detection** — versions with `-` (like `0.1.0-dev`, `1.0.0-beta.1`) auto-flag as pre-release on both platforms 🧪
- **changelog extraction** — release notes auto-pulled from `CHANGELOG.md` for that professional rizz 📜
- **version sanity check** — tag must match `Directory.Build.props` or the pipeline tells you its not it 💀

see [`docs/runner-setup.md`](docs/runner-setup.md) for runner setup and required secrets no cap

## manifesting these features 🧠✨

stuff that would go absolutely crazy but aint started yet. vibes only rn no cap

- 🟦 **Teams Adaptive Cards** — colored containers per severity, webhook sink already has the stub ready to cook
- 💬 **Slack webhook sink** — Block Kit formatter for the Slack besties
- 🟣 **Matrix Client-Server API** — direct room messages for power users who want full HTML control instead of hookshot
- 🎨 **custom webhook templates** — user-defined message format strings so you can make it look however you want
- 🗜️ **zstd compression** — for file sink rotation (gzip is cool but zstd is faster and smaller fr fr)
- 📊 **structured log enrichment** — auto-attach machine name, environment, correlation IDs to webhook messages

wanna see one of these happen? PRs are open bestie, or just vibe in the issues 💅

## security 🔒

litty-logs takes security seriously even though we dont take ourselves seriously no cap. heres the tldr:

- **webhook URL validation** — SSRF prevention, only `http`/`https` schemes allowed
- **log injection prevention** — newlines in messages get sanitized to spaces in text output
- **markdown injection prevention** — webhook messages escape markdown syntax so no tracking pixels or phishing links in your chat
- **HTTP category filtering** — prevents infinite recursion AND accidental webhook URL token exposure

full details in [`docs/security.md`](docs/security.md)

found a vulnerability? dont yeet it in a public issue — open a [security advisory](https://github.com/phsk69/litty-logs-dotnet/security/advisories/new) instead bestie 🔒

## license

MIT — share the vibes bestie ✌️

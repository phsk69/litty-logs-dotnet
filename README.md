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
[🔥 INFO] [2026-02-18T21:45:00.420Z] [Lifetime] we vibing on http://localhost:5000 fr fr 🎧
[🔥 INFO] [2026-02-18T21:45:00.421Z] [Lifetime] app is bussin and ready to slay bestie 💅 yeet Ctrl+C to dip out no cap
[🔥 INFO] [2026-02-18T21:45:00.421Z] [Lifetime] content root living at /app bestie 📁
```

## installation

```bash
dotnet add package LittyLogs

# for xUnit test output (optional, separate package)
dotnet add package LittyLogs.Xunit

# for the CLI tool that litty-fies build and test output
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
});
```

## `dotnet litty` CLI tool — litty-fy your build and test output 🧪

your app logs are litty but `dotnet build` and `dotnet test` output is still giving corporate energy? install the tool and never look at boring terminal output again no cap

```bash
# install the tool
dotnet tool install --global LittyLogs.Tool

# litty-fy your test output (auto-shows ITestOutputHelper output too)
dotnet litty test

# litty-fy your build output
dotnet litty build

# all args pass through to the underlying dotnet command
dotnet litty test --filter "FullyQualifiedName~MyTests"
dotnet litty build -c Release
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
// output: [🔥 INFO] [2026-02-18T21:45:00.420Z] [MyService] my custom message stays exactly like this
```

## examples

four example projects in `examples/` so you can see litty-logs in every scenario:

| example | what it shows | run it |
|---|---|---|
| `LittyLogs.Example.WebApi` | ASP.NET Core minimal api with request logging | `just example-web` |
| `LittyLogs.Example.HostedService` | background service doing vibe checks in a loop | `just example-hosted` |
| `LittyLogs.Example.Console` | simplest possible setup, logs at every level then dips | `just example-console` |
| `LittyLogs.Example.Xunit` | litty-fied xUnit test output with all log levels | `just example-xunit` |

## development — for the contributing besties 🛠️

### just recipes

this project uses [just](https://just.systems) as the task runner. here are the vibes:

| recipe | what it does |
|---|---|
| `just build` | build the whole solution |
| `just test` | run all tests |
| `just litty-build` | build with litty-fied output 🔥 |
| `just litty-test` | test with litty-fied output 🔥 |
| `just pack` | pack all three NuGet packages |
| `just bump patch` | bump the patch version (also: `minor`, `major`) |
| `just bump-pre dev.1` | slap a pre-release label on (e.g. `0.1.0-dev.1`) |
| `just release patch` | gitflow release — bump + `git flow release start/finish` 🚀 |
| `just release-current` | gitflow release without bumping (for first release etc.) |
| `just hotfix patch` | start a gitflow hotfix branch off main 🚑 |
| `just hotfix-finish` | finish a hotfix — `git flow hotfix finish` |
| `just nuget-push` | manually push packages to nuget.org |

### versioning

version lives in one place: `Directory.Build.props`. all three packages inherit from it. we use [gitflow](https://nvie.com/posts/a-successful-git-branching-model/) via the `git flow` CLI — `main` is production, `develop` is the integration branch, releases and hotfixes get their own branches 🔥

### release flow (gitflow)

```bash
# from develop — full gitflow ceremony (bump, release branch, merge, tag, cleanup)
just release patch    # 0.1.0 → 0.1.1
just release minor    # 0.1.0 → 0.2.0
just release major    # 0.1.0 → 1.0.0

# or release the current version without bumping (e.g. first release)
just release-current

# push everything to trigger the CI/CD pipeline
git push origin develop main v0.1.1
```

### hotfix flow

```bash
# from main — start a hotfix when something is bricked in prod
just hotfix patch

# make your fix, commit it, then finish
just hotfix-finish

# push everything
git push origin develop main v0.1.1
```

### CI/CD

forgejo actions handles the pipeline on a self-hosted runner:

- **CI** — builds, tests (with litty output 🔥), and packs on every push/PR to `develop` and `main`
- **Release** — triggered by `v*` tags. builds, tests, packs, pushes to [nuget.org](https://nuget.org), and creates a GitHub release with the `.nupkg` files

see [`docs/runner-setup.md`](docs/runner-setup.md) for runner setup instructions no cap

## license

MIT — share the vibes bestie ✌️
